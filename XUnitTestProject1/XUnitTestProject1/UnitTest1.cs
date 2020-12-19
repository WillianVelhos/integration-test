using System;
using System.Management.Automation.Runspaces;
using System.Text;
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
            // (_dockerContainerId, _dockerSqlPort) = await DockerSqlDatabaseUtilities.EnsureDockerStartedAndGetContainerIdAndPortAsync();

            var runspace = RunspaceFactory.CreateRunspace();
            runspace.Open();
            var pipeline = runspace.CreatePipeline();
            pipeline.Commands.AddScript("");
            pipeline.Commands.Add("Out-String");
            var results = pipeline.Invoke();
            runspace.Close();

            var stringBuilder = new StringBuilder();
            foreach (var result in results)
                stringBuilder.AppendLine(result.ToString());

            Console.WriteLine(stringBuilder.ToString());
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
