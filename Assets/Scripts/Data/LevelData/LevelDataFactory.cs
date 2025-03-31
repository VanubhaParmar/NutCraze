using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Tag.NutSort
{
    public class LevelDataFactory
    {
        #region PRIVATE_VARIABLES

        private static Dictionary<string, LevelData> _levelCache = new Dictionary<string, LevelData>();
        private static Dictionary<string, List<LevelData>> _levelTypeCache = new Dictionary<string, List<LevelData>>();

        #endregion

        #region PROPERTIES
        public static int LevelsPerChunk { get; set; } = 100;
        public static bool UseCompression { get; set; } = false;
        public static bool UseEncryption { get; set; } = true;
        public static bool UseCache { get; set; } = false;
        #endregion

        #region PUBLIC_FUNCTIONS

        public static LevelData GetLevelData(ABTestType abTestType, LevelType type, int levelNumber)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            string cacheKey = MakeCacheKey(abTestType, type, levelNumber);

            if (UseCache && _levelCache.TryGetValue(cacheKey, out LevelData cachedLevel))
            {
                stopwatch.Stop();
                Debug.Log($"GetLevelData took cache {stopwatch.ElapsedMilliseconds}ms");
                return cachedLevel;
            }

            string resourcePath = GetResourcePathByLevel(abTestType, type, levelNumber);

            Debug.Log(resourcePath);

            if (File.Exists(resourcePath))
            {
                Debug.Log("File exists " + resourcePath);
            }

            TextAsset binAsset = Resources.Load<TextAsset>(resourcePath);

            if (binAsset == null)
            {
                Debug.LogWarning($"Level file not found in Resources: {resourcePath}");
                stopwatch.Stop();
                return null;
            }

            byte[] data = binAsset.bytes;

            if (UseEncryption)
            {
                data = EncryptionAlgo.Decrypt(data);
            }

            if (UseCompression)
            {
                data = CompressionAlgo.Decompress(data);
            }

            using (MemoryStream ms = new MemoryStream(data))
            using (BinaryReader reader = new BinaryReader(ms))
            {
                int levelCount = reader.ReadInt32();

                for (int i = 0; i < levelCount; i++)
                {
                    LevelData levelData = ReadLevelData(reader);

                    if (levelData.level == levelNumber && levelData.levelType == type)
                    {
                        if (UseCache)
                        {
                            _levelCache[cacheKey] = levelData;
                            AddToTypeCache(abTestType, levelData);
                        }
                        stopwatch.Stop();
                        Debug.Log($"GetLevelData took {stopwatch.ElapsedMilliseconds}ms");
                        return levelData;
                    }
                }
            }

            stopwatch.Stop();
            Debug.Log($"Level not found in chunk, time taken: {stopwatch.ElapsedMilliseconds}ms");
            return null;
        }

        public static List<LevelData> GetLevelsByType(ABTestType abTestType, LevelType type)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            string typeCacheKey = MakeTypeCacheKey(abTestType, type);

            if (UseCache && _levelTypeCache.TryGetValue(typeCacheKey, out List<LevelData> cachedLevels))
            {
                stopwatch.Stop();
                Debug.Log($"GetLevelsByType took cache {stopwatch.ElapsedMilliseconds}ms");
                return cachedLevels;
            }

            List<LevelData> result = new List<LevelData>();

            for (int chunkIndex = 0; chunkIndex < LevelsPerChunk; chunkIndex++)
            {
                string resourcePath = GetResourcePathByChunk(abTestType, type, chunkIndex);
                TextAsset binAsset = Resources.Load<TextAsset>(resourcePath);

                if (binAsset == null)
                {
                    break;
                }

                byte[] data = binAsset.bytes;

                if (UseEncryption)
                {
                    data = EncryptionAlgo.Decrypt(data);
                }

                if (UseCompression)
                {
                    data = CompressionAlgo.Decompress(data);
                }

                using (MemoryStream ms = new MemoryStream(data))
                using (BinaryReader reader = new BinaryReader(ms))
                {
                    int levelCount = reader.ReadInt32();

                    for (int i = 0; i < levelCount; i++)
                    {
                        LevelData levelData = ReadLevelData(reader);

                        if (levelData.levelType == type)
                        {
                            result.Add(levelData);

                            if (UseCache)
                            {
                                string cacheKey = MakeCacheKey(abTestType, type, levelData.level);
                                _levelCache[cacheKey] = levelData;
                            }
                        }
                    }
                }
            }

            if (UseCache)
            {
                _levelTypeCache[typeCacheKey] = result;
            }
            stopwatch.Stop();
            Debug.Log($"GetLevelsByType took {stopwatch.ElapsedMilliseconds}ms");
            return result;
        }

        public static async Task<LevelData> GetLevelDataAsync(ABTestType abTestType, LevelType type, int levelNumber)
        {
            return await Task.Run(() => GetLevelData(abTestType, type, levelNumber));
        }

        public static async Task<List<LevelData>> GetLevelsByTypeAsync(ABTestType abTestType, LevelType type)
        {
            return await Task.Run(() => GetLevelsByType(abTestType, type));
        }

        public static int GetTotalLevelCount(ABTestType abTestType, LevelType type)
        {
            try
            {
                string resourcePath = $"LevelData/{GetLevelTypeFolder(abTestType, type)}/{abTestType}_{type}_manifest";
                TextAsset manifestAsset = Resources.Load<TextAsset>(resourcePath);

                if (manifestAsset == null)
                {
                    Debug.LogWarning($"Type manifest not found in Resources for {abTestType} {type}");
                    return 0;
                }

                TypeManifestData manifest = JsonConvert.DeserializeObject<TypeManifestData>(manifestAsset.text);

                if (manifest == null)
                {
                    Debug.LogError($"Failed to parse JSON manifest for {abTestType} {type}");
                    return 0;
                }

                if (manifest.LevelType != type.ToString())
                {
                    Debug.LogError($"Type mismatch in manifest. Expected {type}, found {manifest.LevelType}");
                    return 0;
                }

                return manifest.TotalLevels;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error reading level count for {abTestType} {type}: {ex.Message}");
                return 0;
            }
        }

        public static void ClearCache()
        {
            _levelCache.Clear();
            _levelTypeCache.Clear();
        }

        public static void ClearCacheForAbTest(string abTestType)
        {
            List<string> keysToRemove = new List<string>();
            foreach (var key in _levelCache.Keys)
            {
                if (key.StartsWith(abTestType + "_"))
                    keysToRemove.Add(key);
            }

            foreach (var key in keysToRemove)
                _levelCache.Remove(key);

            keysToRemove.Clear();

            foreach (var key in _levelTypeCache.Keys)
            {
                if (key.StartsWith(abTestType + "_"))
                    keysToRemove.Add(key);
            }

            foreach (var key in keysToRemove)
                _levelTypeCache.Remove(key);
        }
        #endregion

        #region PRIVATE_FUNCTIONS
        private static string GetResourcePathByLevel(ABTestType abTestType, LevelType type, int levelNumber)
        {
            int chunkIndex = (levelNumber - 1) / LevelsPerChunk;
            return GetResourcePathByChunk(abTestType, type, chunkIndex);
        }

        private static string GetResourcePathByChunk(ABTestType abTestType, LevelType type, int chunkIndex)
        {
            return $"LevelData/{GetLevelTypeFolder(abTestType, type)}/{type}_chunk_{chunkIndex}";
        }

        private static string MakeCacheKey(ABTestType abTestType, LevelType type, int levelNumber)
        {
            return $"{abTestType}_{type}_{levelNumber}";
        }

        private static string MakeTypeCacheKey(ABTestType abTestType, LevelType type)
        {
            return $"{abTestType}_{type}";
        }

        private static string GetLevelTypeFolder(ABTestType abTestType, LevelType type)
        {
            return $"{abTestType}/{type}";
        }

        private static void WriteLevelData(BinaryWriter writer, LevelData levelData)
        {
            writer.Write(levelData.level);
            writer.Write((int)levelData.levelType);

            writer.Write(levelData.stages != null ? levelData.stages.Length : 0);
            if (levelData.stages != null)
            {
                foreach (var stage in levelData.stages)
                {
                    writer.Write(stage.arrangementId);
                    writer.Write(stage.screwDatas != null ? stage.screwDatas.Length : 0);
                    if (stage.screwDatas != null)
                    {
                        foreach (var screwData in stage.screwDatas)
                        {
                            writer.Write(screwData.id);
                            writer.Write(screwData.screwType);
                            writer.Write(screwData.size);

                            writer.Write(screwData.screwStages != null ? screwData.screwStages.Length : 0);
                            if (screwData.screwStages != null)
                            {
                                foreach (var screwStage in screwData.screwStages)
                                {
                                    writer.Write(screwStage.isStorage);
                                    writer.Write(screwStage.isRefresh);
                                    writer.Write(screwStage.isGenerator);
                                    writer.Write(screwStage.color);
                                    writer.Write(screwStage.curtainColor);

                                    writer.Write(screwStage.nutDatas != null ? screwStage.nutDatas.Length : 0);
                                    if (screwStage.nutDatas != null)
                                    {
                                        foreach (var nutData in screwStage.nutDatas)
                                        {
                                            writer.Write(nutData.nutType);
                                            writer.Write(nutData.nutColorTypeId);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private static LevelData ReadLevelData(BinaryReader reader)
        {
            LevelData levelData = new LevelData();

            levelData.level = reader.ReadInt32();
            levelData.levelType = (LevelType)reader.ReadInt32();

            int stageCount = reader.ReadInt32();
            if (stageCount > 0)
            {
                levelData.stages = new LevelStage[stageCount];

                for (int i = 0; i < stageCount; i++)
                {
                    LevelStage stage = new LevelStage();

                    stage.arrangementId = reader.ReadInt32();
                    int screwDataCount = reader.ReadInt32();
                    if (screwDataCount > 0)
                    {
                        stage.screwDatas = new ScrewData[screwDataCount];

                        for (int j = 0; j < screwDataCount; j++)
                        {
                            ScrewData screwData = new ScrewData();
                            screwData.id = reader.ReadInt32();
                            screwData.screwType = reader.ReadInt32();
                            screwData.size = reader.ReadInt32();

                            int screwStageCount = reader.ReadInt32();
                            if (screwStageCount > 0)
                            {
                                screwData.screwStages = new ScrewStage[screwStageCount];

                                for (int k = 0; k < screwStageCount; k++)
                                {
                                    ScrewStage screwStage = new ScrewStage();

                                    screwStage.isStorage = reader.ReadBoolean();
                                    screwStage.isRefresh = reader.ReadBoolean();
                                    screwStage.isGenerator = reader.ReadBoolean();
                                    screwStage.color = reader.ReadInt32();
                                    screwStage.curtainColor = reader.ReadInt32();

                                    int nutDataCount = reader.ReadInt32();
                                    if (nutDataCount > 0)
                                    {
                                        screwStage.nutDatas = new NutData[nutDataCount];

                                        for (int l = 0; l < nutDataCount; l++)
                                        {
                                            NutData nutData = new NutData();
                                            nutData.nutType = reader.ReadInt32();
                                            nutData.nutColorTypeId = reader.ReadInt32();

                                            screwStage.nutDatas[l] = nutData;
                                        }
                                    }
                                    else
                                    {
                                        screwStage.nutDatas = new NutData[0];
                                    }
                                    screwData.screwStages[k] = screwStage;
                                }
                            }
                            else
                            {
                                screwData.screwStages = new ScrewStage[0];
                            }
                            stage.screwDatas[j] = screwData;
                        }
                    }
                    else
                    {
                        stage.screwDatas = new ScrewData[0];
                    }

                    levelData.stages[i] = stage;
                }
            }
            else
            {
                levelData.stages = new LevelStage[0];
            }
            return levelData;
        }

        private static string GetTypeManifestName(ABTestType aBTestType, LevelType levelType)
        {
            return $"{aBTestType}_{levelType}_manifest.json";
        }

        private static string GetGlobalManifestName(ABTestType aBTestType)
        {
            return $"{aBTestType}_manifest.json";
        }

        private static void AddToTypeCache(ABTestType abTestType, LevelData levelData)
        {
            string typeCacheKey = MakeTypeCacheKey(abTestType, levelData.levelType);

            if (!_levelTypeCache.ContainsKey(typeCacheKey))
                _levelTypeCache[typeCacheKey] = new List<LevelData>();

            if (!_levelTypeCache[typeCacheKey].Exists(l => l.level == levelData.level))
                _levelTypeCache[typeCacheKey].Add(levelData);
        }
        #endregion

        #region UNITY_EDITOR
#if UNITY_EDITOR
        private static string LevelDataPath = Path.Combine(Application.dataPath, "Resources/LevelData");

        public static void SaveLevelData(ABTestType abTestType, LevelData levelData)
        {
            if (levelData == null)
            {
                Debug.LogError("No level data to save!");
                return;
            }

            int chunkIndex = (levelData.level - 1) / LevelsPerChunk;

            string typeFolder = GetLevelTypeFolder(abTestType, levelData.levelType);
            string basePath = LevelDataPath;
            string typePath = Path.Combine(basePath, typeFolder);

            if (!Directory.Exists(basePath))
            {
                Directory.CreateDirectory(basePath);
            }

            if (!Directory.Exists(typePath))
            {
                Directory.CreateDirectory(typePath);
            }

            string fileName = $"{levelData.levelType}_chunk_{chunkIndex}.txt";
            string filePath = Path.Combine(typePath, fileName);

            List<LevelData> chunkLevels = new List<LevelData>();

            if (File.Exists(filePath))
            {
                try
                {
                    using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                    {
                        byte[] data = new byte[fileStream.Length];
                        fileStream.Read(data, 0, data.Length);

                        if (UseEncryption)
                        {
                            data = EncryptionAlgo.Decrypt(data);
                        }

                        if (UseCompression)
                        {
                            data = CompressionAlgo.Decompress(data);
                        }

                        using (MemoryStream ms = new MemoryStream(data))
                        using (BinaryReader reader = new BinaryReader(ms))
                        {
                            int levelCount = reader.ReadInt32();

                            for (int i = 0; i < levelCount; i++)
                            {
                                LevelData existingLevelData = ReadLevelData(reader);

                                if (existingLevelData.level != levelData.level || existingLevelData.levelType != levelData.levelType)
                                {
                                    chunkLevels.Add(existingLevelData);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error reading chunk file: {ex.Message}");
                }
            }

            chunkLevels.Add(levelData);

            try
            {
                chunkLevels.Sort((a, b) => a.level.CompareTo(b.level));

                byte[] binaryData;
                using (MemoryStream ms = new MemoryStream())
                using (BinaryWriter writer = new BinaryWriter(ms))
                {
                    writer.Write(chunkLevels.Count);
                    foreach (var level in chunkLevels)
                        WriteLevelData(writer, level);
                    binaryData = ms.ToArray();
                }

                if (UseCompression)
                    binaryData = CompressionAlgo.Compress(binaryData);

                if (UseEncryption)
                    binaryData = EncryptionAlgo.Encrypt(binaryData);

                using (FileStream fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    fileStream.Write(binaryData, 0, binaryData.Length);
                }

                if (UseCache)
                {
                    string cacheKey = MakeCacheKey(abTestType, levelData.levelType, levelData.level);
                    _levelCache[cacheKey] = levelData;
                    AddToTypeCache(abTestType, levelData);
                }

                UpdateTypeManifest(abTestType, levelData.levelType);

                UpdateGlobalManifest(abTestType, levelData.levelType);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error saving level {levelData.level} of type {levelData.levelType} for AB test {abTestType}: {ex.Message}");
            }
        }

        private static void UpdateTypeManifest(ABTestType abTestType, LevelType type)
        {
            try
            {
                string typeFolder = GetLevelTypeFolder(abTestType, type);
                string directoryPath = Path.Combine(LevelDataPath, typeFolder);
                string jsonManifestPath = Path.Combine(directoryPath, GetTypeManifestName(abTestType, type));

                string[] chunkFiles = Directory.GetFiles(directoryPath, $"{type}_chunk_*.txt");
                int chunkCount = chunkFiles.Length;
                int totalLevels = 0;

                foreach (string filePath in chunkFiles)
                {
                    try
                    {
                        using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                        {
                            byte[] data = new byte[fileStream.Length];
                            fileStream.Read(data, 0, data.Length);

                            if (UseEncryption)
                                data = EncryptionAlgo.Decrypt(data);

                            if (UseCompression)
                                data = CompressionAlgo.Decompress(data);

                            using (MemoryStream ms = new MemoryStream(data))
                            using (BinaryReader reader = new BinaryReader(ms))
                            {
                                int levelCount = reader.ReadInt32();
                                totalLevels += levelCount;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Error reading chunk file for manifest update: {ex.Message}");
                    }
                }
                SaveTypeManifest(abTestType, type, chunkCount, totalLevels);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error updating type manifest for {abTestType} {type}: {ex.Message}");
            }
        }

        private static void UpdateGlobalManifest(ABTestType abTestType, LevelType levelType)
        {
            Debug.Log($"Updating global manifest for {abTestType} {levelType}");
            try
            {
                string abTestFolder = Path.Combine(LevelDataPath, abTestType.ToString());

                if (!Directory.Exists(abTestFolder))
                {
                    Debug.LogWarning($"AB test folder not found: {abTestFolder}");
                    return;
                }

                string[] typeFolders = Directory.GetDirectories(abTestFolder);
                Debug.Log($"Type folders Count: {typeFolders.Length}");
                int totalLevels = 0;

                Dictionary<string, TypeManifestData> typeData = new Dictionary<string, TypeManifestData>();

                foreach (string typeFolder in typeFolders)
                {
                    string typeName = Path.GetFileName(typeFolder);
                    Debug.Log($"typeFolder : {typeFolder}");
                    Debug.Log($"Type folder: {typeName}");

                    // Look for any type manifest in this folder, not just the one for levelType
                    string[] manifestFiles = Directory.GetFiles(typeFolder, "*_manifest.json");

                    foreach (string manifestPath in manifestFiles)
                    {
                        try
                        {
                            string jsonContent = File.ReadAllText(manifestPath);
                            TypeManifestData manifestData = JsonConvert.DeserializeObject<TypeManifestData>(jsonContent);

                            if (manifestData != null)
                            {
                                totalLevels += manifestData.TotalLevels;
                                typeData[typeName] = manifestData;
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError($"Error reading manifest file for global manifest update: {ex.Message} - {manifestPath}");
                        }
                    }
                }
                Debug.Log($"Total level types for {typeData.Count}: {typeFolders.Length}");
                SaveGlobalManifest(abTestType, totalLevels, typeData);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error updating global manifest for {abTestType}: {ex.Message}");
            }
        }

        private static void SaveTypeManifest(ABTestType abTestType, LevelType type, int chunkCount, int totalLevels)
        {
            try
            {
                string typeFolder = GetLevelTypeFolder(abTestType, type);
                string directoryPath = Path.Combine(LevelDataPath, typeFolder);
                string filePath = Path.Combine(directoryPath, GetTypeManifestName(abTestType, type));

                TypeManifestData manifest = new TypeManifestData
                {
                    LevelType = type.ToString(),
                    ChunkCount = chunkCount,
                    TotalLevels = totalLevels,
                    LevelsPerChunk = LevelsPerChunk,
                    LastUpdated = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };

                string jsonContent = JsonConvert.SerializeObject(manifest, formatting: Formatting.Indented);

                File.WriteAllText(filePath, jsonContent);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error saving JSON type manifest for {abTestType} {type}: {ex.Message}");
            }
        }
        private static void SaveGlobalManifest(ABTestType abTestType, int totalLevels, Dictionary<string, TypeManifestData> typeData)
        {
            try
            {
                string abTestFolder = Path.Combine(LevelDataPath, abTestType.ToString());
                string filePath = Path.Combine(abTestFolder, GetGlobalManifestName(abTestType));

                GlobalManifestData manifest = new GlobalManifestData
                {
                    ABTestType = abTestType.ToString(),
                    TotalLevels = totalLevels,
                    LevelsPerChunk = LevelsPerChunk,
                    LastUpdated = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    LevelTypes = typeData
                };

                string jsonContent = JsonConvert.SerializeObject(manifest, formatting: Formatting.Indented);

                File.WriteAllText(filePath, jsonContent);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error saving JSON global manifest for {abTestType}: {ex.Message}");
            }
        }
#endif
        #endregion

    }

    [Serializable]
    public class TypeManifestData
    {
        public string LevelType;
        public int ChunkCount;
        public int TotalLevels;
        public int LevelsPerChunk;
        public string LastUpdated;
    }

    [Serializable]
    public class GlobalManifestData
    {
        public string ABTestType;
        public int TotalLevels;
        public int LevelsPerChunk;
        public string LastUpdated;
        public Dictionary<string, TypeManifestData> LevelTypes;
    }
}
