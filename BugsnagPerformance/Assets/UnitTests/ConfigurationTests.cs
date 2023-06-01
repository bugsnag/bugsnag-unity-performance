using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using BugsnagUnityPerformance;
using System;

namespace Tests
{
    public class ConfigurationTests
    {

        private const string VALID_API_KEY = "227df1042bc7772c321dbde3b31a03c2";


        [Test]
        public void SetInvalidApiKey()
        {

            Assert.Throws(typeof(Exception), () =>
            {
                new PerformanceConfiguration("test");
            });
        }

        [Test]
        public void SetValidApiKey()
        {
            new PerformanceConfiguration(VALID_API_KEY);
        }

        [Test]
        public void ReleaseStage()
        {
            var config = new PerformanceConfiguration(VALID_API_KEY);
            Assert.AreEqual("development", config.ReleaseStage);
            config.ReleaseStage = "test";
            Assert.AreEqual("test", config.ReleaseStage);
        }

    }
}
