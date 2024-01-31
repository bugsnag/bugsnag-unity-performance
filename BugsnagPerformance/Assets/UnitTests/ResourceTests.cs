using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using BugsnagUnityPerformance;
using System;
using System.Text.RegularExpressions;

namespace Tests
{
    public class ResourceTests
    {
        [Test]
        public void FormatWindowsVersion()
        {
            var version1 = "Microsoft Windows NT 10.0.17134.0";
            var matches1 = Regex.Match(version1, "\\A(?<osName>[a-zA-Z ]*) (?<osVersion>[\\d\\.]*)\\z");
            Assert.IsTrue(matches1.Success);
            Assert.AreEqual(matches1.Groups["osVersion"].Value, "10.0.17134.0");

            var version2 = "Microsoft Windows NT 10.0.19044.0";
            var matches2 = Regex.Match(version2, "\\A(?<osName>[a-zA-Z ]*) (?<osVersion>[\\d\\.]*)\\z");
            Assert.IsTrue(matches2.Success);
            Assert.AreEqual(matches2.Groups["osVersion"].Value, "10.0.19044.0");
        }
    }
}