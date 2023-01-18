using NUnit.Framework;
using System;
namespace BugsnagPerformance.Tests
{
    [TestFixture]
    public class ConfigurationTests
    {
        [Test]
        public void SetInvalidApiKey()
        {
            Assert.Throws(typeof(Exception), ()=>
            {
               new PerformanceConfiguration("test");
            });
        }

        [Test]
        public void SetValidApiKey()
        {
            new PerformanceConfiguration("227df1042bc7772c321dbde3b31a03c2");
        }
    }
}
