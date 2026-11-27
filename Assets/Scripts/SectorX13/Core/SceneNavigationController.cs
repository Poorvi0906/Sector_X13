using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SectorX13.Core
{
    /// <summary>
    /// Persistent singleton managing canonical scene transitions, back-stack history,
    /// and safety-gate navigation guards.
    /// </summary>
    public class SceneNavigationController : MonoBehaviour
    {
        private static SceneNavigationController _instance;
        private readonly Stack<string> _history = new Stack<string>();
        private string _currentScene;

        /// <summary>
        /// Optional gate predicate evaluated before entering restricted scenes (e.g. AR Laboratory, Results).
        /// Return false to block navigation and redirect to Safety Assessment.
        /// </summary>
        public Func<string, bool> CanEnterScene { get; set; }

        public string CurrentScene => _currentScene;
        public int HistoryCount => _history.Count;

        public static SceneNavigationController Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<SceneNavigationController>();
                    if (_instance == null)
                    {
                        var go = new GameObject("X13_NavigationController");
                        _instance = go.AddComponent<SceneNavigationController>();
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
            _currentScene = SceneManager.GetActiveScene().name;
        }

        public bool NavigateTo(string targetScene, bool recordHistory = true)
        {
            if (string.IsNullOrEmpty(targetScene)) return false;

            if (CanEnterScene != null && !CanEnterScene(targetScene))
            {
                Debug.LogWarning($"[X13 Nav] Blocked navigation to '{targetScene}' by safety gate.");
                return false;
            }

            string active = SceneManager.GetActiveScene().name;
            if (active == targetScene) return true;

            if (recordHistory && !string.IsNullOrEmpty(active) && active != X13SceneIds.Splash)
            {
                if (_history.Count == 0 || _history.Peek() != active)
                {
                    _history.Push(active);
                }
            }

            _currentScene = targetScene;
            SceneManager.LoadScene(targetScene);
            return true;
        }

        public void GoBack()
        {
            while (_history.Count > 0)
            {
                string previous = _history.Pop();
                if (previous != SceneManager.GetActiveScene().name && previous != X13SceneIds.Splash)
                {
                    if (CanEnterScene != null && !CanEnterScene(previous))
                    {
                        continue;
                    }
                    _currentScene = previous;
                    SceneManager.LoadScene(previous);
                    return;
                }
            }

            GoHome(clearHistory: true);
        }

        public void GoHome(bool clearHistory = false)
        {
            if (clearHistory) _history.Clear();
            NavigateTo(X13SceneIds.Home, recordHistory: false);
        }

        public void ClearHistory()
        {
            _history.Clear();
        }
    }
}
