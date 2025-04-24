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
    public static class ProtoLevelDataFactory
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

            try
            {
                LevelChunk chunk = ProtoBufHelper.Deserialize<LevelChunk>(data);
                foreach (var levelData in chunk.Levels)
                {
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
            catch (Exception ex)
            {
                Debug.LogError($"Error deserializing level data: {ex.Message}");
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

                try
                {
                    LevelChunk chunk = ProtoBufHelper.Deserialize<LevelChunk>(data);
                    foreach (var levelData in chunk.Levels)
                    {
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
                catch (Exception ex)
                {
                    Debug.LogError($"Error deserializing chunk data: {ex.Message}");
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

        public static List<ABTestType> GetAvailableABTestTypes()
        {
            List<ABTestType> availableTypes = new List<ABTestType>();
            try
            {
                Array abTestValues = Enum.GetValues(typeof(ABTestType));

                foreach (ABTestType abTestType in abTestValues)
                {
                    if (IsABTestTypeExist(abTestType))
                        availableTypes.Add(abTestType);
                }
                Debug.Log($"Found {availableTypes.Count} available ABTest types");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error getting available ABTest types: {ex.Message}");
            }

            return availableTypes;
        }

        public static bool IsABTestTypeExist(ABTestType abTestType)
        {
            try
            {
                string resourcePath = $"LevelData/{abTestType}/{abTestType}_manifest";
                TextAsset manifestAsset = Resources.Load<TextAsset>(resourcePath);
                return manifestAsset != null;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error checking if ABTest type {abTestType} exists: {ex.Message}");
                return false;
            }
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

            LevelChunk chunkData = new LevelChunk();

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

                        try
                        {
                            chunkData = ProtoBufHelper.Deserialize<LevelChunk>(data);
                            chunkData.Levels.RemoveAll(l => l.level == levelData.level && l.levelType == levelData.levelType);
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError($"Error deserializing chunk file: {ex.Message}");
                            chunkData = new LevelChunk();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error reading chunk file: {ex.Message}");
                    chunkData = new LevelChunk();
                }
            }

            chunkData.Levels.Add(levelData);

            try
            {
                chunkData.Levels.Sort((a, b) => a.level.CompareTo(b.level));

                byte[] protoData = ProtoBufHelper.Serialize(chunkData);

                if (UseCompression)
                    protoData = CompressionAlgo.Compress(protoData);

                if (UseEncryption)
                    protoData = EncryptionAlgo.Encrypt(protoData);

                using (FileStream fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    fileStream.Write(protoData, 0, protoData.Length);
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

                            try
                            {
                                LevelChunk chunk = ProtoBufHelper.Deserialize<LevelChunk>(data);
                                totalLevels += chunk.Levels.Count;
                            }
                            catch (Exception ex)
                            {
                                Debug.LogError($"Error deserializing chunk for manifest update: {ex.Message}");
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

        private static void SaveGlobalManifest(ABTestType abTestType, int totalLevels, Dictionary<string, TypeManifestData> typeData,
    int repeatLastLevelsCount = 50, int playSpecialLevelsCount = 8)
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
                    LevelTypes = typeData,
                    RepeatLastLevelsCountAfterGameFinish = repeatLastLevelsCount,
                    PlaySpecialLevelAfterEveryLevelsCount = playSpecialLevelsCount
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
}
