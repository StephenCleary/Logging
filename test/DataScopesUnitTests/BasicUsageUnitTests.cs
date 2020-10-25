using System;
using System.Collections.Generic;
using DataScopesUnitTests.Utility;
using Microsoft.Extensions.Logging;
using Nito.Logging;
using Xunit;

namespace DataScopesUnitTests
{
    public class BasicUsageUnitTests
    {
        [Fact]
        public void OneTuple() => LoggingTestUtility.InitializeLogs((logs, logger) =>
        {
            using (logger.BeginDataScope(("test", 13)))
            {
                logger.LogError("message");
            }

            Assert.Collection(logs.Messages,
                message => Assert.Equal(13, Assert.Contains("test", message.ScopeValues)));
        });

        [Fact]
        public void TwoTuples() => LoggingTestUtility.InitializeLogs((logs, logger) =>
        {
            using (logger.BeginDataScope(("test", 13), ("test2", 17)))
            {
                logger.LogError("message");
            }

            Assert.Collection(logs.Messages,
                message =>
                {
                    Assert.Equal(13, Assert.Contains("test", message.ScopeValues));
                    Assert.Equal(17, Assert.Contains("test2", message.ScopeValues));
                });
        });

        [Fact]
        public void ThreeTuples() => LoggingTestUtility.InitializeLogs((logs, logger) =>
        {
            using (logger.BeginDataScope(("test", 13), ("test2", 17), ("test3", 19)))
            {
                logger.LogError("message");
            }

            Assert.Collection(logs.Messages,
                message =>
                {
                    Assert.Equal(13, Assert.Contains("test", message.ScopeValues));
                    Assert.Equal(17, Assert.Contains("test2", message.ScopeValues));
                    Assert.Equal(19, Assert.Contains("test3", message.ScopeValues));
                });
        });

        [Fact]
        public void Dictionary() => LoggingTestUtility.InitializeLogs((logs, logger) =>
        {
            using (logger.BeginDataScope(new Dictionary<string, object>
            {
                { "test", 13 },
                { "test2", 17 },
            }))
            {
                logger.LogError("message");
            }

            Assert.Collection(logs.Messages,
                message =>
                {
                    Assert.Equal(13, Assert.Contains("test", message.ScopeValues));
                    Assert.Equal(17, Assert.Contains("test2", message.ScopeValues));
                });
        });

        [Fact]
        public void AnonymousObject() => LoggingTestUtility.InitializeLogs((logs, logger) =>
        {
            using (logger.BeginDataScope(new
            {
                test = 13,
                test2 = 17,
            }))
            {
                logger.LogError("message");
            }

            Assert.Collection(logs.Messages,
                message =>
                {
                    Assert.Equal(13, Assert.Contains("test", message.ScopeValues));
                    Assert.Equal(17, Assert.Contains("test2", message.ScopeValues));
                });
        });
    }
}
