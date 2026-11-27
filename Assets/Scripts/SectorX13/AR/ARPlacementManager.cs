using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace SectorX13.AR
{
    /// <summary>
    /// Manages ARSession / XROrigin plane raycast placement on Android
    /// and provides a zero-tracking Editor simulation fallback.
    /// </summary>
    public class ARPlacementManager : MonoBehaviour
    {
        public Transform labRigRoot;
        public ARRaycastManager raycastManager;
        public ARPlaneManager planeManager;
        public Camera arCamera;

        public bool IsPlaced { get; private set; }
        public bool IsARTrackingActive => !Application.isEditor && raycastManager != null && ARSession.state >= ARSessionState.SessionTracking;

        private static readonly List<ARRaycastHit> Hits = new List<ARRaycastHit>();

        private void Start()
        {
            if (Application.isEditor || raycastManager == null)
            {
                // Editor simulation path: place lab rig immediately in front of camera.
                if (labRigRoot != null)
                {
                    labRigRoot.gameObject.SetActive(true);
                    labRigRoot.position = Vector3.zero;
                    labRigRoot.rotation = Quaternion.identity;
                }
                IsPlaced = true;
            }
            else
            {
                if (labRigRoot != null) labRigRoot.gameObject.SetActive(false);
                IsPlaced = false;
            }
        }

        private void Update()
        {
            if (Application.isEditor || IsPlaced || raycastManager == null) return;

            var touchscreen = Touchscreen.current;
            if (touchscreen != null && touchscreen.primaryTouch.press.wasPressedThisFrame)
            {
                Vector2 pos = touchscreen.primaryTouch.position.ReadValue();
                TryPlaceAtScreenPoint(pos);
            }
        }

        public bool TryPlaceAtScreenPoint(Vector2 screenPosition)
        {
            if (raycastManager == null || labRigRoot == null) return false;
            if (raycastManager.Raycast(screenPosition, Hits, TrackableType.PlaneWithinPolygon))
            {
                Pose hitPose = Hits[0].pose;
                labRigRoot.position = hitPose.position;
                labRigRoot.rotation = hitPose.rotation;
                labRigRoot.gameObject.SetActive(true);
                IsPlaced = true;
                return true;
            }
            return false;
        }

        public void ResetPlacement()
        {
            if (Application.isEditor)
            {
                IsPlaced = true;
                return;
            }
            IsPlaced = false;
            if (labRigRoot != null) labRigRoot.gameObject.SetActive(false);
        }
    }
}
