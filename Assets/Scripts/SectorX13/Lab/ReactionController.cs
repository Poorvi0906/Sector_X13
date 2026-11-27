using System;
using System.Collections;
using SectorX13.Core;
using UnityEngine;

namespace SectorX13.Lab
{
    /// <summary>
    /// Controls the 3-vessel 3D virtual laboratory (Beaker A, Reaction Chamber, Beaker B)
    /// and executes safety-gated Pour and Mix animations.
    /// </summary>
    public class ReactionController : MonoBehaviour
    {
        [Header("3D Vessels & Liquids")]
        public Transform beakerA;
        public Transform beakerB;
        public Transform reactionChamber;
        public Transform liquidA;
        public Transform liquidB;
        public Transform liquidMix;
        public Transform streamA;
        public Transform streamB;
        public ParticleSystem bubbleParticles;
        public Renderer liquidARenderer;
        public Renderer liquidBRenderer;
        public Renderer liquidMixRenderer;
        public Renderer reactionGlowRenderer;

        public bool IsSimulating { get; private set; }
        public bool HasPoured { get; private set; }
        public bool HasMixed { get; private set; }
        public float Progress { get; private set; }
        public string StatusMessage { get; private set; } = "VESSELS PLACED";

        public event Action OnLabStateChanged;

        private Coroutine _activeRoutine;
        private Vector3 _initialBeakerAPos, _initialBeakerBPos;
        private Vector3 _initialLiquidAScale, _initialLiquidBScale, _initialLiquidMixScale;
        private Color _colorA = new Color(0.10f, 0.82f, 0.95f, 1f);
        private Color _colorB = new Color(0.12f, 0.85f, 0.65f, 1f);

        private void Awake()
        {
            if (beakerA != null) _initialBeakerAPos = beakerA.localPosition;
            if (beakerB != null) _initialBeakerBPos = beakerB.localPosition;
            if (liquidA != null) _initialLiquidAScale = liquidA.localScale;
            if (liquidB != null) _initialLiquidBScale = liquidB.localScale;
            if (liquidMix != null) _initialLiquidMixScale = liquidMix.localScale;
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
            if (chemA != null) _colorA = chemA.liquidColor;
            if (chemB != null) _colorB = chemB.liquidColor;
            if (liquidARenderer != null) liquidARenderer.material.color = _colorA;
            if (liquidBRenderer != null) liquidBRenderer.material.color = _colorB;
            if (streamA != null)
            {
                var r = streamA.GetComponent<Renderer>();
                if (r != null) r.material.color = _colorA;
            }
            if (streamB != null)
            {
                var r = streamB.GetComponent<Renderer>();
                if (r != null) r.material.color = _colorB;
            }
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
            Progress = 0.05f;
            StatusMessage = "POURING REAGENTS INTO CHAMBER";
            OnLabStateChanged?.Invoke();

            // Phase 1: Lift and tilt Beaker A & B toward center Reaction Chamber
            float liftTime = 0.55f;
            float elapsed = 0f;
            Vector3 targetAPos = _initialBeakerAPos + new Vector3(0.32f, 0.52f, 0f);
            Vector3 targetBPos = _initialBeakerBPos + new Vector3(-0.32f, 0.52f, 0f);
            while (elapsed < liftTime)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / liftTime);
                if (beakerA != null)
                {
                    beakerA.localPosition = Vector3.Lerp(_initialBeakerAPos, targetAPos, t);
                    beakerA.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(0f, -42f, t));
                }
                if (beakerB != null)
                {
                    beakerB.localPosition = Vector3.Lerp(_initialBeakerBPos, targetBPos, t);
                    beakerB.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(0f, 42f, t));
                }
                yield return null;
            }

            // Phase 2: Stream A & B into central Reaction Chamber while liquid levels transfer
            if (streamA != null) streamA.gameObject.SetActive(true);
            if (streamB != null) streamB.gameObject.SetActive(true);
            if (liquidMix != null) liquidMix.gameObject.SetActive(true);

            float pourTime = 1.6f;
            elapsed = 0f;
            Color blended = Color.Lerp(_colorA, _colorB, 0.5f);
            while (elapsed < pourTime)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / pourTime);
                Progress = Mathf.Lerp(0.1f, 0.55f, t);

                if (liquidA != null)
                {
                    liquidA.localScale = new Vector3(_initialLiquidAScale.x, Mathf.Lerp(_initialLiquidAScale.y, 0.04f, t), _initialLiquidAScale.z);
                }
                if (liquidB != null)
                {
                    liquidB.localScale = new Vector3(_initialLiquidBScale.x, Mathf.Lerp(_initialLiquidBScale.y, 0.04f, t), _initialLiquidBScale.z);
                }
                if (liquidMix != null)
                {
                    float targetY = _initialLiquidMixScale.y > 0f ? _initialLiquidMixScale.y : 0.34f;
                    liquidMix.localScale = new Vector3(_initialLiquidMixScale.x, Mathf.Lerp(0.02f, targetY, t), _initialLiquidMixScale.z);
                }
                if (liquidMixRenderer != null)
                {
                    liquidMixRenderer.material.color = blended;
                }
                OnLabStateChanged?.Invoke();
                yield return null;
            }

            if (streamA != null) streamA.gameObject.SetActive(false);
            if (streamB != null) streamB.gameObject.SetActive(false);

            // Phase 3: Return Beaker A & B smoothly to bench
            elapsed = 0f;
            while (elapsed < liftTime)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / liftTime);
                if (beakerA != null)
                {
                    beakerA.localPosition = Vector3.Lerp(targetAPos, _initialBeakerAPos, t);
                    beakerA.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(-42f, 0f, t));
                }
                if (beakerB != null)
                {
                    beakerB.localPosition = Vector3.Lerp(targetBPos, _initialBeakerBPos, t);
                    beakerB.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(42f, 0f, t));
                }
                yield return null;
            }

            HasPoured = true;
            IsSimulating = false;
            Progress = 0.6f;
            StatusMessage = "POUR COMPLETE · READY TO MIX";
            OnLabStateChanged?.Invoke();
        }

        private IEnumerator MixRoutine()
        {
            IsSimulating = true;
            var assessment = ExperimentManager.Instance.CurrentAssessment;
            string effectId = assessment?.BoundExperiment?.effectId ?? "neutralisation_glow";
            StatusMessage = $"MIXING · {effectId.ToUpperInvariant()}";
            OnLabStateChanged?.Invoke();

            if (liquidMix != null) liquidMix.gameObject.SetActive(true);
            if (effectId == "effervescence_co2" && bubbleParticles != null)
            {
                bubbleParticles.Play();
            }
            if (reactionGlowRenderer != null)
            {
                reactionGlowRenderer.gameObject.SetActive(true);
            }

            Color startCol = liquidMixRenderer != null ? liquidMixRenderer.material.color : _colorA;
            Color reactionCol = effectId == "effervescence_co2"
                ? new Color(0.15f, 0.92f, 0.98f, 1f)
                : new Color(0.12f, 0.90f, 0.55f, 1f);

            float mixTime = 2.2f;
            float elapsed = 0f;
            while (elapsed < mixTime)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / mixTime);
                Progress = Mathf.Lerp(0.6f, 1.0f, t);

                if (reactionChamber != null)
                {
                    float sway = Mathf.Sin(elapsed * 10f) * 2.5f * (1f - t);
                    reactionChamber.localRotation = Quaternion.Euler(0f, 0f, sway);
                }
                if (liquidMixRenderer != null)
                {
                    liquidMixRenderer.material.color = Color.Lerp(startCol, reactionCol, t);
                }
                OnLabStateChanged?.Invoke();
                yield return null;
            }

            if (reactionChamber != null) reactionChamber.localRotation = Quaternion.identity;
            if (bubbleParticles != null) bubbleParticles.Stop();

            HasMixed = true;
            IsSimulating = false;
            Progress = 1.0f;

            StatusMessage = ExperimentManager.Instance.CompleteSimulation()
                ? "SIMULATION COMPLETE"
                : "AUTHORIZATION LOST";
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
            Progress = 0f;
            StatusMessage = "VESSELS PLACED";

            if (beakerA != null)
            {
                beakerA.localPosition = _initialBeakerAPos;
                beakerA.localRotation = Quaternion.identity;
            }
            if (beakerB != null)
            {
                beakerB.localPosition = _initialBeakerBPos;
                beakerB.localRotation = Quaternion.identity;
            }
            if (reactionChamber != null) reactionChamber.localRotation = Quaternion.identity;
            if (liquidA != null && _initialLiquidAScale != Vector3.zero) liquidA.localScale = _initialLiquidAScale;
            if (liquidB != null && _initialLiquidBScale != Vector3.zero) liquidB.localScale = _initialLiquidBScale;
            if (liquidMix != null)
            {
                liquidMix.localScale = new Vector3(_initialLiquidMixScale.x > 0f ? _initialLiquidMixScale.x : 0.42f, 0.02f, _initialLiquidMixScale.z > 0f ? _initialLiquidMixScale.z : 0.42f);
                liquidMix.gameObject.SetActive(false);
            }
            if (streamA != null) streamA.gameObject.SetActive(false);
            if (streamB != null) streamB.gameObject.SetActive(false);
            if (reactionGlowRenderer != null) reactionGlowRenderer.gameObject.SetActive(false);
            if (bubbleParticles != null) bubbleParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            OnLabStateChanged?.Invoke();
        }
    }
}
