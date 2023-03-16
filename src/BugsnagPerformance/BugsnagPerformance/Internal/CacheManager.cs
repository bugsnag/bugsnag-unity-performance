using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace BugsnagUnityPerformance
{
    internal class CacheManager
    {

        private static string _cacheDirectory
        {
            get { return Application.persistentDataPath + "/bugsnag-performance/v1"; }
        }

        private const string BATCH_FILE_SUFFIX = ".json";


        public static void CacheBatch(TracePayload payload)
        {
            var existingBatches = GetCachedBatchPaths();
            foreach (var path in existingBatches)
            {
                if (path.Contains(payload.PayloadId))
                {
                    return;
                }
            }
            var newPath = _cacheDirectory + "/" + payload.PayloadId + BATCH_FILE_SUFFIX;
            WriteFile(newPath, payload.GetJsonBody());
        }

        public static List<TracePayload> GetCachedBatchesForDelivery()
        {
            RemoveExpiredPayloads();
            var existingBatches = GetCachedBatchPaths();
            var payloads = new List<TracePayload>();
            foreach (var path in existingBatches)
            {
                var json = GetJsonFromCachePath(path);
                var id = Path.GetFileNameWithoutExtension(path);
                var payload = new TracePayload(json,id);
                payloads.Add(payload);
            }
            return payloads;
        }

        public static void RemoveCachedBatch(string id)
        {
            var paths = GetCachedBatchPaths();
            foreach (var path in paths)
            {
                if (path.Contains(id))
                {
                    DeleteFile(path);
                }
            }
        }

        private static void DoFileSystemChecks()
        {
            CheckForDirectoryCreation();
        }

        private static void RemoveExpiredPayloads()
        {
            var paths = GetCachedBatchPaths();
            foreach (var path in paths)
            {
                var creationTime = File.GetCreationTimeUtc(path);
                var timeSinceCreation = DateTimeOffset.UtcNow - creationTime;
                if (timeSinceCreation.TotalSeconds > PerformanceConfiguration.MaxPersistedBatchAgeSeconds)
                {
                    DeleteFile(path);
                }
            }
        }

        private static void DeleteFile(string path)
        {
            DoFileSystemChecks();
            try
            {
                File.Delete(path);
            }
            catch { }
        }

        private static void WriteFile(string path, string data)
        {
            DoFileSystemChecks();
            try
            {
                File.WriteAllText(path, data);
            }
            catch { }
        }

        private static string GetJsonFromCachePath(string path)
        {
            DoFileSystemChecks();
            try
            {
                if (File.Exists(path))
                {
                    return File.ReadAllText(path);
                }
            }
            catch { }
            return null;
        }

        private static string[] GetCachedBatchPaths()
        {
            if (Directory.Exists(_cacheDirectory))
            {
                var filePaths = Directory.GetFiles(_cacheDirectory, "*" + BATCH_FILE_SUFFIX);
                return filePaths.OrderBy(file => File.GetCreationTimeUtc(file)).ToArray();
            }
            return new string[] { };
        }

        private static void CheckForDirectoryCreation()
        {
            try
            {
                if (!Directory.Exists(_cacheDirectory))
                {
                    Directory.CreateDirectory(_cacheDirectory);
                }
            }
            catch
            {
                //not possible in unit tests
            }

        }

    }
}
