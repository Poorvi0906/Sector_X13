using System;
using System.Collections.Generic;
using System.IO;
using SectorX13.Data;
using UnityEditor;
using UnityEngine;

namespace SectorX13.Editor
{
    /// <summary>
    /// Imports Assets/Data/x13_dataset.json into ScriptableObject assets under Assets/Resources/X13Data/.
    /// </summary>
    public static class X13DatasetImporter
    {
        private const string JsonPath = "Assets/Data/x13_dataset.json";
        private const string DataRoot = "Assets/Resources/X13Data";

        [Serializable]
        private class DatasetPayload
        {
            public ChemicalJson[] chemicals;
            public ExperimentJson[] experiments;
            public AlternativeJson[] alternatives;
        }

        [Serializable]
        private class ChemicalJson
        {
            public string chemicalId;
            public string chemicalName;
            public string formula;
            public string category;
            public string physicalForm;
            public string propertiesSummary;
            public string sourceReference;
            public string sdsReference;
            public string hazardInfo;
            public string precautions;
            public string reviewStatus;
            public string[] supportedScenarioIds;
            public string fixedConditionsNote;
            public string arVisualDescription;
            public string visualAssetId;
            public string liquidColorHex;
            public string riskAssessmentSummary;
            public bool isSyntheticTestRecord;
        }

        [Serializable]
        private class ExperimentJson
        {
            public string scenarioId;
            public string title;
            public string chemicalAId;
            public string chemicalBId;
            public string fixedConcentrationA;
            public string fixedQuantityA;
            public string fixedConcentrationB;
            public string fixedQuantityB;
            public string fixedConditions;
            public string reviewedRiskCategory;
            public string reviewStatus;
            public string reviewEvidence;
            public string dataVersion;
            public string[] controls;
            public string rationale;
            public string observations;
            public string equation;
            public string effectId;
            public string safetyNotes;
            public string educationalOutcome;
            public string[] alternativeIds;
            public bool isSyntheticTestRecord;
        }

        [Serializable]
        private class AlternativeJson
        {
            public string linkId;
            public string sourceScenarioId;
            public string sourceSelectionKey;
            public string targetScenarioId;
            public string reviewedRationale;
            public string reviewEvidence;
            public string reviewStatus;
            public bool isSyntheticTestRecord;
        }

        [MenuItem("SECTOR X-13/Phase 3/Import Dataset JSON")]
        public static void ImportDataset()
        {
            if (!File.Exists(JsonPath))
            {
                Debug.LogError($"[X13 Importer] Dataset file not found at {JsonPath}");
                return;
            }

            string raw = File.ReadAllText(JsonPath);
            var payload = JsonUtility.FromJson<DatasetPayload>(raw);
            if (payload == null)
            {
                Debug.LogError("[X13 Importer] Failed to parse x13_dataset.json");
                return;
            }

            EnsureFolder(DataRoot);
            EnsureFolder($"{DataRoot}/Chemicals");
            EnsureFolder($"{DataRoot}/Experiments");
            EnsureFolder($"{DataRoot}/Alternatives");

            var dbPath = $"{DataRoot}/X13Database.asset";
            var db = AssetDatabase.LoadAssetAtPath<ChemicalDatabase>(dbPath);
            if (db == null)
            {
                db = ScriptableObject.CreateInstance<ChemicalDatabase>();
                AssetDatabase.CreateAsset(db, dbPath);
            }

            db.chemicals = new List<ChemicalData>();
            db.experiments = new List<ExperimentData>();
            db.alternatives = new List<AlternativeLink>();

            if (payload.chemicals != null)
            {
                foreach (var c in payload.chemicals)
                {
                    var asset = LoadOrCreate<ChemicalData>($"{DataRoot}/Chemicals/{c.chemicalId}.asset");
                    asset.chemicalId = c.chemicalId;
                    asset.chemicalName = c.chemicalName;
                    asset.formula = c.formula;
                    asset.category = c.category;
                    asset.physicalForm = c.physicalForm;
                    asset.propertiesSummary = c.propertiesSummary;
                    asset.sourceReference = c.sourceReference;
                    asset.sdsReference = c.sdsReference;
                    asset.hazardInfo = c.hazardInfo;
                    asset.precautions = c.precautions;
                    asset.reviewStatus = ParseEnum(c.reviewStatus, ReviewStatus.PendingReview);
                    asset.supportedScenarioIds = c.supportedScenarioIds ?? Array.Empty<string>();
                    asset.fixedConditionsNote = c.fixedConditionsNote;
                    asset.arVisualDescription = c.arVisualDescription;
                    asset.visualAssetId = c.visualAssetId;
                    asset.liquidColor = ColorUtility.TryParseHtmlString(c.liquidColorHex, out var col) ? col : new Color(0.1f, 0.7f, 0.9f, 0.85f);
                    asset.riskAssessmentSummary = c.riskAssessmentSummary;
                    asset.isSyntheticTestRecord = c.isSyntheticTestRecord;
                    EditorUtility.SetDirty(asset);
                    db.chemicals.Add(asset);
                }
            }

            if (payload.experiments != null)
            {
                foreach (var e in payload.experiments)
                {
                    var asset = LoadOrCreate<ExperimentData>($"{DataRoot}/Experiments/{e.scenarioId}.asset");
                    asset.scenarioId = e.scenarioId;
                    asset.title = e.title;
                    asset.chemicalAId = e.chemicalAId;
                    asset.chemicalBId = e.chemicalBId;
                    asset.fixedConcentrationA = e.fixedConcentrationA;
                    asset.fixedQuantityA = e.fixedQuantityA;
                    asset.fixedConcentrationB = e.fixedConcentrationB;
                    asset.fixedQuantityB = e.fixedQuantityB;
                    asset.fixedConditions = e.fixedConditions;
                    asset.reviewedRiskCategory = ParseEnum(e.reviewedRiskCategory, RiskCategory.Unknown);
                    asset.reviewStatus = ParseEnum(e.reviewStatus, ReviewStatus.PendingReview);
                    asset.reviewEvidence = e.reviewEvidence;
                    asset.dataVersion = e.dataVersion;
                    asset.controls = e.controls ?? Array.Empty<string>();
                    asset.rationale = e.rationale;
                    asset.observations = e.observations;
                    asset.equation = e.equation;
                    asset.effectId = e.effectId;
                    asset.safetyNotes = e.safetyNotes;
                    asset.educationalOutcome = e.educationalOutcome;
                    asset.alternativeIds = e.alternativeIds ?? Array.Empty<string>();
                    asset.isSyntheticTestRecord = e.isSyntheticTestRecord;
                    EditorUtility.SetDirty(asset);
                    db.experiments.Add(asset);
                }
            }

            if (payload.alternatives != null)
            {
                foreach (var a in payload.alternatives)
                {
                    var asset = LoadOrCreate<AlternativeLink>($"{DataRoot}/Alternatives/{a.linkId}.asset");
                    asset.linkId = a.linkId;
                    asset.sourceScenarioId = a.sourceScenarioId;
                    asset.sourceSelectionKey = a.sourceSelectionKey;
                    asset.targetScenarioId = a.targetScenarioId;
                    asset.reviewedRationale = a.reviewedRationale;
                    asset.reviewEvidence = a.reviewEvidence;
                    asset.reviewStatus = ParseEnum(a.reviewStatus, ReviewStatus.PendingReview);
                    asset.isSyntheticTestRecord = a.isSyntheticTestRecord;
                    EditorUtility.SetDirty(asset);
                    db.alternatives.Add(asset);
                }
            }

            EditorUtility.SetDirty(db);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[X13 Importer] Imported {db.chemicals.Count} chemicals, {db.experiments.Count} experiments, {db.alternatives.Count} alternatives.");
        }

        private static T LoadOrCreate<T>(string path) where T : ScriptableObject
        {
            var existing = AssetDatabase.LoadAssetAtPath<T>(path);
            if (existing != null) return existing;
            var instance = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(instance, path);
            return instance;
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = Path.GetDirectoryName(path)?.Replace("\\", "/");
            string name = Path.GetFileName(path);
            AssetDatabase.CreateFolder(parent, name);
        }

        private static TEnum ParseEnum<TEnum>(string value, TEnum fallback) where TEnum : struct
        {
            return Enum.TryParse<TEnum>(value, true, out var parsed) ? parsed : fallback;
        }
    }
}
