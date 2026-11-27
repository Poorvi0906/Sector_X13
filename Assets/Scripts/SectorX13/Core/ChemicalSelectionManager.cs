using System;
using SectorX13.Data;
using UnityEngine;

namespace SectorX13.Core
{
    /// <summary>
    /// Manages deliberate two-beaker reagent selection and library inspection state.
    /// Never defaults both slots to index 0; clears authorization whenever selection changes.
    /// </summary>
    public class ChemicalSelectionManager : MonoBehaviour
    {
        private static ChemicalSelectionManager _instance;

        public string ChemicalAId { get; private set; }
        public string ChemicalBId { get; private set; }
        public string InspectedChemicalId { get; private set; } = "CH-01";
        public bool IncludeSyntheticTestRecords { get; set; }

        public event Action OnSelectionChanged;

        public static ChemicalSelectionManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<ChemicalSelectionManager>();
                    if (_instance == null)
                    {
                        var go = new GameObject("X13_ChemicalSelectionManager");
                        _instance = go.AddComponent<ChemicalSelectionManager>();
                    }
                }
                return _instance;
            }
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public bool HasBothSelections => !string.IsNullOrEmpty(ChemicalAId) && !string.IsNullOrEmpty(ChemicalBId);

        public void InspectChemical(string chemicalId)
        {
            if (!string.IsNullOrEmpty(chemicalId))
            {
                InspectedChemicalId = chemicalId;
            }
        }

        public void SelectChemicalA(string chemicalId)
        {
            if (ChemicalAId == chemicalId) return;
            ChemicalAId = chemicalId;
            OnSelectionChanged?.Invoke();
        }

        public void SelectChemicalB(string chemicalId)
        {
            if (ChemicalBId == chemicalId) return;
            ChemicalBId = chemicalId;
            OnSelectionChanged?.Invoke();
        }

        public void SelectPair(string chemicalAId, string chemicalBId)
        {
            ChemicalAId = chemicalAId;
            ChemicalBId = chemicalBId;
            OnSelectionChanged?.Invoke();
        }

        public void ClearSelection()
        {
            ChemicalAId = null;
            ChemicalBId = null;
            OnSelectionChanged?.Invoke();
        }

        public ChemicalData GetChemicalA() => ChemicalDatabase.Load()?.GetChemicalById(ChemicalAId);
        public ChemicalData GetChemicalB() => ChemicalDatabase.Load()?.GetChemicalById(ChemicalBId);
        public ChemicalData GetInspectedChemical() => ChemicalDatabase.Load()?.GetChemicalById(InspectedChemicalId);
    }
}
