using System;
using System.Collections;
using SectorX13.Core;
using UnityEngine;

namespace SectorX13.Lab
{
    /// <summary>
    /// Controls the 3D virtual laboratory vessels and authorized reaction animations.
    /// Enforces safety-gate verification before any pour/mix and prevents duplicate effects.
    /// </summary>
    public class ReactionController : MonoBehaviour
    {
        [Header("3D Vessels & Visuals")]
        public Transform beakerA;
        public Transform beakerB;
        public Transform liquidA;
        public Transform liquidB;
        public Transform pourStream;
        public ParticleSystem bubbleParticles;
        public Renderer liquidARenderer;
        public Renderer liquidBRenderer;
        public Renderer reactionGlowRenderer;

        public bool IsSimulating { get; private set; }
        public bool HasPoured { get; private set; }
        public bool HasMixed { get; private set; }
        public string StatusMessage { get; private set; } = "VESSELS READY";

        public event Action OnLabStateChanged;

        private Coroutine _activeRoutine;
        private Vector3 _initialLiquidAScale;
        private Vector3 _initialLiquidBScale;

        private void Awake()
        {
            if (liquidA != null) _initialLiquidAScale = liquidA.localScale;
            if (liquidB != null) _initialLiquidBScale = liquidB.localScale;
            ResetVisuals();
        }

        private void Start()
        {
            ApplySelectedChemicalColors();
        }

        public void ApplySelectedChemicalColors()
        {
            var sel = ChemicalSelectionManager.Instance;
            var chemA = sel.GetChemicalA();
            var chemB = sel.GetChemicalB();
            if (liquidARenderer != null && chemA != null) liquidARenderer.material.color = chemA.liquidColor;
            if (liquidBRenderer != null && chemB != null) liquidBRenderer.material.color = chemB.liquidColor;
        }

        public bool TriggerPour()
        {
            if (IsSimulating || HasPoured) return false;
            if (!ExperimentManager.Instance.VerifyAuthorizationForExecution())
            {
                StatusMessage = "BLOCKED: UNAUTHORIZED";
                OnLabStateChanged?.Invoke();
                return false;
            }

            if (!ExperimentManager.Instance.BeginSimulation()) return false;
            _activeRoutine = StartCoroutine(PourRoutine());
            return true;
        }

        public bool TriggerMix()
        {
            if (IsSimulating || HasMixed) return false;
            if (!ExperimentManager.Instance.VerifyAuthorizationForExecution())
            {
                StatusMessage = "BLOCKED: UNAUTHORIZED";
                OnLabStateChanged?.Invoke();
                return false;
            }

            if (!ExperimentManager.Instance.BeginSimulation()) return false;
            _activeRoutine = StartCoroutine(MixRoutine());
            return true;
        }

        private IEnumerator PourRoutine()
        {
            IsSimulating = true;
            StatusMessage = "TRANSFERRING REAGENT A -> B";
            OnLabStateChanged?.Invoke();

            if (pourStream != null) pourStream.gameObject.SetActive(true);
            float duration = 1.6f;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                if (liquidA != null)
                {
                    liquidA.localScale = new Vector3(_initialLiquidAScale.x, Mathf.Lerp(_initialLiquidAScale.y, _initialLiquidAScale.y * 0.25f, t), _initialLiquidAScale.z);
                }
                if (liquidB != null)
                {
                    liquidB.localScale = new Vector3(_initialLiquidBScale.x, Mathf.Lerp(_initialLiquidBScale.y, _initialLiquidBScale.y * 1.45f, t), _initialLiquidBScale.z);
                }
                yield return null;
            }

            if (pourStream != null) pourStream.gameObject.SetActive(false);
            HasPoured = true;
            IsSimulating = false;
            StatusMessage = "POUR COMPLETE · READY TO MIX";
            OnLabStateChanged?.Invoke();
        }

        private IEnumerator MixRoutine()
        {
            IsSimulating = true;
            var assessment = ExperimentManager.Instance.CurrentAssessment;
            string effectId = assessment?.BoundExperiment?.effectId ?? "neutralisation_glow";
            StatusMessage = $"SIMULATING: {effectId.ToUpperInvariant()}";
            OnLabStateChanged?.Invoke();

            if (effectId == "effervescence_co2" && bubbleParticles != null)
            {
                bubbleParticles.Play();
            }
            if (reactionGlowRenderer != null)
            {
                reactionGlowRenderer.gameObject.SetActive(true);
            }

            yield return new WaitForSeconds(2.0f);

            if (bubbleParticles != null) bubbleParticles.Stop();
            HasMixed = true;
            IsSimulating = false;

            if (ExperimentManager.Instance.CompleteSimulation())
            {
                StatusMessage = "SIMULATION COMPLETE";
            }
            else
            {
                StatusMessage = "AUTHORIZATION LOST";
            }
            OnLabStateChanged?.Invoke();
        }

        public void ResetVisuals()
        {
            if (_activeRoutine != null)
            {
                StopCoroutine(_activeRoutine);
                _activeRoutine = null;
            }
            IsSimulating = false;
            HasPoured = false;
            HasMixed = false;
            StatusMessage = "VESSELS READY";
            if (liquidA != null && _initialLiquidAScale != Vector3.zero) liquidA.localScale = _initialLiquidAScale;
            if (liquidB != null && _initialLiquidBScale != Vector3.zero) liquidB.localScale = _initialLiquidBScale;
            if (pourStream != null) pourStream.gameObject.SetActive(false);
            if (reactionGlowRenderer != null) reactionGlowRenderer.gameObject.SetActive(false);
            if (bubbleParticles != null) bubbleParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            OnLabStateChanged?.Invoke();
        }
    }
}
