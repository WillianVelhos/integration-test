using System;
using System.Threading.Tasks;
using Xunit;

namespace XUnitTestProject1
{
    public class UnitTest1 : IAsyncLifetime
    {
        public const string DATABASE_NAME_PLACEHOLDER = "@@databaseName@@";
        private string _dockerContainerId;
        private string _dockerSqlPort;

        public UnitTest1()
        {
        }

        public async Task InitializeAsync()
        {
            (_dockerContainerId, _dockerSqlPort) = await DockerSqlDatabaseUtilities.EnsureDockerStartedAndGetContainerIdAndPortAsync();
        }


        [Fact]
        public void Test1()
        {

        }

        public Task DisposeAsync()
        {
            return DockerSqlDatabaseUtilities.EnsureDockerStoppedAndRemovedAsync(_dockerContainerId);
        }
    }
}
