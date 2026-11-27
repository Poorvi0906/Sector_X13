using System;
using SectorX13.Data;
using UnityEngine;

namespace SectorX13.Core
{
    public enum ExperimentLifecycleState
    {
        Idle = 0,
        Selected = 1,
        Reviewing = 2,
        Authorized = 3,
        Prohibited = 4,
        Simulating = 5,
        Completed = 6
    }

    /// <summary>
    /// Owns experiment lifecycle state transitions, scenario-bound authorization,
    /// and navigation safety-gate enforcement.
    /// </summary>
    public class ExperimentManager : MonoBehaviour
    {
        private static ExperimentManager _instance;

        public ExperimentLifecycleState State { get; private set; } = ExperimentLifecycleState.Idle;
        public SafetyAssessmentResult CurrentAssessment { get; private set; }
        public string AcknowledgedScenarioId { get; private set; }
        public string AcknowledgedDataVersion { get; private set; }
        public bool HasCompletedScenario => State == ExperimentLifecycleState.Completed && CurrentAssessment?.BoundExperiment != null;

        public event Action OnStateChanged;

        public static ExperimentManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<ExperimentManager>();
                    if (_instance == null)
                    {
                        var go = new GameObject("X13_ExperimentManager");
                        _instance = go.AddComponent<ExperimentManager>();
                    }
                }
                return _instance;
            }
        }

        private bool _initialized;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            if (Application.isPlaying) DontDestroyOnLoad(gameObject);
            EnsureInitialized();
        }

        public void EnsureInitialized()
        {
            if (_initialized) return;
            _initialized = true;
            ChemicalSelectionManager.Instance.OnSelectionChanged += HandleSelectionChanged;
            SceneNavigationController.Instance.CanEnterScene = CanEnterScene;
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                var sel = FindFirstObjectByType<ChemicalSelectionManager>();
                if (sel != null) sel.OnSelectionChanged -= HandleSelectionChanged;
            }
        }

        private void HandleSelectionChanged()
        {
            InvalidateAuthorization();
            State = ChemicalSelectionManager.Instance.HasBothSelections
                ? ExperimentLifecycleState.Selected
                : ExperimentLifecycleState.Idle;
            OnStateChanged?.Invoke();
        }

        public void InvalidateAuthorization()
        {
            CurrentAssessment = null;
            AcknowledgedScenarioId = null;
            AcknowledgedDataVersion = null;
        }

        public SafetyAssessmentResult AssessCurrentSelection()
        {
            InvalidateAuthorization();
            State = ExperimentLifecycleState.Reviewing;

            var sel = ChemicalSelectionManager.Instance;
            var db = ChemicalDatabase.Load();
            CurrentAssessment = SafetyAssessment.Evaluate(
                db,
                sel.ChemicalAId,
                sel.ChemicalBId,
                sel.IncludeSyntheticTestRecords);

            State = CurrentAssessment.IsAuthorizedByRule
                ? ExperimentLifecycleState.Authorized
                : ExperimentLifecycleState.Prohibited;

            OnStateChanged?.Invoke();
            return CurrentAssessment;
        }

        public bool AcknowledgeMediumRisk(string scenarioId, string dataVersion)
        {
            if (CurrentAssessment == null) return false;
            if (CurrentAssessment.Risk != RiskCategory.Medium) return false;
            if (CurrentAssessment.ScenarioId != scenarioId || CurrentAssessment.DataVersion != dataVersion) return false;

            AcknowledgedScenarioId = scenarioId;
            AcknowledgedDataVersion = dataVersion;
            State = ExperimentLifecycleState.Authorized;
            OnStateChanged?.Invoke();
            return true;
        }

        /// <summary>
        /// Rechecks all preconditions immediately before simulation effects or results.
        /// </summary>
        public bool VerifyAuthorizationForExecution()
        {
            if (CurrentAssessment == null || CurrentAssessment.BoundExperiment == null) return false;
            var sel = ChemicalSelectionManager.Instance;
            if (sel.ChemicalAId != CurrentAssessment.ChemicalAId || sel.ChemicalBId != CurrentAssessment.ChemicalBId)
            {
                InvalidateAuthorization();
                State = ExperimentLifecycleState.Selected;
                return false;
            }

            if (CurrentAssessment.Risk == RiskCategory.Low && CurrentAssessment.IsAuthorizedByRule)
            {
                return State == ExperimentLifecycleState.Authorized || State == ExperimentLifecycleState.Simulating || State == ExperimentLifecycleState.Completed;
            }

            if (CurrentAssessment.Risk == RiskCategory.Medium)
            {
                bool ackMatches = AcknowledgedScenarioId == CurrentAssessment.ScenarioId
                    && AcknowledgedDataVersion == CurrentAssessment.DataVersion;
                return ackMatches && (State == ExperimentLifecycleState.Authorized || State == ExperimentLifecycleState.Simulating || State == ExperimentLifecycleState.Completed);
            }

            return false;
        }

        public bool BeginSimulation()
        {
            if (!VerifyAuthorizationForExecution()) return false;
            State = ExperimentLifecycleState.Simulating;
            OnStateChanged?.Invoke();
            return true;
        }

        public bool CompleteSimulation()
        {
            if (!VerifyAuthorizationForExecution()) return false;
            State = ExperimentLifecycleState.Completed;
            OnStateChanged?.Invoke();
            return true;
        }

        public void ResetSession()
        {
            InvalidateAuthorization();
            ChemicalSelectionManager.Instance.ClearSelection();
            State = ExperimentLifecycleState.Idle;
            OnStateChanged?.Invoke();
        }

        public bool CanEnterScene(string targetScene)
        {
            if (targetScene == X13SceneIds.ARLaboratory)
            {
                return VerifyAuthorizationForExecution()
                    && (State == ExperimentLifecycleState.Authorized || State == ExperimentLifecycleState.Simulating);
            }
            if (targetScene == X13SceneIds.ExperimentResults)
            {
                return VerifyAuthorizationForExecution() && State == ExperimentLifecycleState.Completed;
            }
            return true;
        }
    }
}
