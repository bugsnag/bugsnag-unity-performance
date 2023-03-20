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

        private static string _deviceidFilePath
        {
            get { return Application.persistentDataPath + "/Bugsnag/deviceId.txt"; }
        }

        private const string BATCH_FILE_SUFFIX = ".json";


        public static string GetDeviceId()
        {
            try
            {
                if (File.Exists(_deviceidFilePath))
                {
                    // return existing cached device id
                    return File.ReadAllText(_deviceidFilePath);
                }

                // create and cache new random device id
                var newDeviceId = Guid.NewGuid().ToString();
                WriteFile(_deviceidFilePath, _deviceidFilePath);
                return newDeviceId;
            }
            catch
            {
                // not possible in unit tests
                return string.Empty;
            }
        }

        

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
            try
            {
                File.Delete(path);
            }
            catch
            {
                // Filesystem integrity never 100% reliable, ignore any errors that may occur
            }
        }

        private static void WriteFile(string path, string data)
        {
            try
            {
                Directory.CreateDirectory(_cacheDirectory);
                File.WriteAllText(path, data);
            }
            catch { }
        }

        private static string GetJsonFromCachePath(string path)
        {
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

       

    }
}
