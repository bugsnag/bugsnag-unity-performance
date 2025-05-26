using System;
using System.IO;
using System.Threading;
using Newtonsoft.Json;
using UnityEngine;

namespace BugsnagUnityPerformance
{
    [JsonObject(MemberSerialization.OptIn)]
    public class PersistentState : IPhasedStartup
    {
        private CacheManager _cacheManager;
        bool _isStarted = false;

        private Mutex _fileStreamMutex = new Mutex();

        [JsonProperty("probability")]
        private double _probability = -1;

        public double Probability
        {
            get
            {
                return _probability;
            }
            set
            {
                _probability = value;
                if (_isStarted)
                {
                    Save();

                }
            }
        }

        public PersistentState(CacheManager cacheManager)
        {
            _cacheManager = cacheManager;
        }

        public void Configure(PerformanceConfiguration config)
        {
            // Nothing to do
        }

        public void Start()
        {
            Load();
            _isStarted = true;
        }

        private void Load()
        {
            try
            {
                _fileStreamMutex.WaitOne();
                string serialized = File.ReadAllText(_cacheManager.PersistentStateFilePath);
                if (serialized != null)
                {
                    JsonConvert.PopulateObject(serialized, this);
                }
            }
            catch (Exception)
            {
                // If anything goes wrong, do nothing.
            }
            finally
            {
                _fileStreamMutex.ReleaseMutex();
            }
#if BUGSNAG_DEBUG
            Logger.I("Persistence loaded with probability: " + Probability);
#endif
        }

        private void Save()
        {
            try
            {
                _fileStreamMutex.WaitOne();
                var serialized = JsonConvert.SerializeObject(this);
                if (serialized != null)
                {
                    // File.WriteAllText doesn't overwrite an existing file like the documentation says.
                    // Instead, it throws a sharing violation exception.
                    if (File.Exists(_cacheManager.PersistentStateFilePath))
                    {
                        File.Delete(_cacheManager.PersistentStateFilePath);
                    }
                    var parent = Directory.GetParent(_cacheManager.PersistentStateFilePath);
                    File.WriteAllText(_cacheManager.PersistentStateFilePath, serialized);
                }
            }
            catch (Exception e)
            {
                MainThreadDispatchBehaviour.LogWarning("Failed to save persistent state: " + e);
            }
            finally
            {
                _fileStreamMutex.ReleaseMutex();
            }
        }
    }
}
