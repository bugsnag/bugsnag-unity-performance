using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BugsnagUnityPerformance
{
    public class CacheManager : IPhasedStartup
    {
        private int _maxPersistedBatchAgeSeconds;
        private string _cacheDirectory;
        private string _deviceidFilePath;
        private string _persistentStateFilePath;

        private const string BATCH_FILE_SUFFIX = ".json";

        public string PersistentStateFilePath  {
            get
            {
                return _persistentStateFilePath;
            }
        }

        public CacheManager(string basePath)
        {
            _cacheDirectory = basePath + "/bugsnag-performance/v1";
            _persistentStateFilePath = _cacheDirectory + "/persistent-state.json";
            // Must be set to this path to share with the bugsnag unity notifier
            _deviceidFilePath = basePath + "/Bugsnag/deviceId.txt";
        }

        public void Configure(PerformanceConfiguration config)
        {
            _maxPersistedBatchAgeSeconds = config.MaxPersistedBatchAgeSeconds;
        }

        public void Start()
        {
            if (!Directory.Exists(_cacheDirectory))
            {
                Directory.CreateDirectory(_cacheDirectory);
            }
        }

        public string GetDeviceId()
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

        public void CacheBatch(TracePayload payload)
        {
            var existingBatches = GetCachedBatchPaths();
            foreach (var path in existingBatches)
            {
                if (path.Contains(payload.PayloadId))
                {
                    return;
                }
            }
            var newPath = _cacheDirectory + Path.DirectorySeparatorChar + payload.PayloadId + BATCH_FILE_SUFFIX;
            var stream = new FileStream(newPath, FileMode.Create, FileAccess.Write);
            payload.Serialize(stream);
        }

        public List<TracePayload> GetCachedBatchesForDelivery()
        {
            RemoveExpiredPayloads();
            var existingBatches = GetCachedBatchPaths();
            var payloads = new List<TracePayload>();
            foreach (var path in existingBatches)
            {
                var id = Path.GetFileNameWithoutExtension(path);
                try {
                    var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
                    var payload = TracePayload.Deserialize(id, stream);
                    payloads.Add(payload);
                } catch
                {
                    // Ignore
                }
            }
            return payloads;
        }

        public void RemoveCachedBatch(string id)
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

        public void Clear()
        {
            if (Directory.Exists(_cacheDirectory))
            {
                Directory.Delete(_cacheDirectory, true);
            }
            Directory.CreateDirectory(_cacheDirectory);
        }

        private void RemoveExpiredPayloads()
        {
            var paths = GetCachedBatchPaths();
            foreach (var path in paths)
            {
                var creationTime = File.GetCreationTimeUtc(path);
                var timeSinceCreation = DateTimeOffset.UtcNow - creationTime;
                if (timeSinceCreation.TotalSeconds > _maxPersistedBatchAgeSeconds)
                {
                    DeleteFile(path);
                }
            }
        }

        private void DeleteFile(string path)
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

        private void WriteFile(string path, string data)
        {
            try
            {
                Directory.CreateDirectory(_cacheDirectory);
                File.WriteAllText(path, data);
            }
            catch { }
        }

        private string GetJsonFromCachePath(string path)
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

        private string[] GetCachedBatchPaths()
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
