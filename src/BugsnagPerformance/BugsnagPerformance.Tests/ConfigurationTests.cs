using NUnit.Framework;
using System;
namespace BugsnagPerformance.Tests
{
    [TestFixture]
    public class ConfigurationTests
    {
        [Test]
        public void DefaultConfigurationValues()
        {
            Assert.IsTrue(BugsnagPerformance.TestMethod());
        }
    }
}
