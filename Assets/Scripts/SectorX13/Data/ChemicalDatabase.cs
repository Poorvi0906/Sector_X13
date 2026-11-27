using System.Collections.Generic;
using UnityEngine;

namespace SectorX13.Data
{
    /// <summary>
    /// Central registry of ChemicalData, ExperimentData, and AlternativeLink records.
    /// Loaded from Resources/X13Data/X13Database at runtime.
    /// </summary>
    [CreateAssetMenu(fileName = "X13Database", menuName = "SECTOR X-13/Database")]
    public class ChemicalDatabase : ScriptableObject
    {
        public const string ResourcesPath = "X13Data/X13Database";

        public List<ChemicalData> chemicals = new List<ChemicalData>();
        public List<ExperimentData> experiments = new List<ExperimentData>();
        public List<AlternativeLink> alternatives = new List<AlternativeLink>();

        private static ChemicalDatabase _cached;

        public static ChemicalDatabase Load()
        {
            if (_cached == null)
            {
                _cached = Resources.Load<ChemicalDatabase>(ResourcesPath);
            }
            return _cached;
        }

        public static void ClearCache()
        {
            _cached = null;
        }

        public List<ChemicalData> GetCatalogue(bool includeSynthetic = false)
        {
            var list = new List<ChemicalData>();
            foreach (var c in chemicals)
            {
                if (c == null) continue;
                if (!includeSynthetic && c.isSyntheticTestRecord) continue;
                list.Add(c);
            }
            return list;
        }

        public ChemicalData GetChemicalById(string chemicalId)
        {
            if (string.IsNullOrEmpty(chemicalId)) return null;
            foreach (var c in chemicals)
            {
                if (c != null && c.chemicalId == chemicalId) return c;
            }
            return null;
        }

        public ExperimentData GetExperimentById(string scenarioId)
        {
            if (string.IsNullOrEmpty(scenarioId)) return null;
            foreach (var e in experiments)
            {
                if (e != null && e.scenarioId == scenarioId) return e;
            }
            return null;
        }

        /// <summary>
        /// Finds all experiments matching the unordered chemical pair (A, B).
        /// Returns multiple if ambiguous so SafetyAssessment can reject ambiguity.
        /// </summary>
        public List<ExperimentData> FindExperimentsForPair(string idA, string idB, bool includeSynthetic = false)
        {
            var matches = new List<ExperimentData>();
            if (string.IsNullOrEmpty(idA) || string.IsNullOrEmpty(idB)) return matches;
            foreach (var e in experiments)
            {
                if (e == null) continue;
                if (!includeSynthetic && e.isSyntheticTestRecord) continue;
                if (e.MatchesPair(idA, idB)) matches.Add(e);
            }
            return matches;
        }

        public static string BuildSelectionKey(string idA, string idB)
        {
            if (string.IsNullOrEmpty(idA) || string.IsNullOrEmpty(idB)) return string.Empty;
            return string.CompareOrdinal(idA, idB) <= 0 ? $"{idA}+{idB}" : $"{idB}+{idA}";
        }
    }
}
