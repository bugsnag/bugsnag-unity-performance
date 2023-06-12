using System;
using System.IO;
using Newtonsoft.Json;

namespace BugsnagUnityPerformance
{
    [JsonObject(MemberSerialization.OptIn)]
    public class PersistentState : IPhasedStartup
    {
        private CacheManager _cacheManager;

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
                Save();
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
        }

        private void Load()
        {
            try
            {
                using (StreamReader sw = new StreamReader(_cacheManager.OpenPersistentStateStream()))
                using (JsonReader reader = new JsonTextReader(sw))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Populate(reader, this);
                }
            }
            catch (Exception)
            {
                // If anything goes wrong, do nothing.
            }
        }

        private void Save()
        {
            using (StreamWriter sw = new StreamWriter(_cacheManager.OpenPersistentStateStream()))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.NullValueHandling = NullValueHandling.Ignore;
                serializer.Serialize(writer, this);
                writer.Flush();
            }
        }
    }
}
