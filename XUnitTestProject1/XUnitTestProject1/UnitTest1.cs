using System;
using System.Data.SqlClient;
using System.Linq;
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
        public const string SQLSERVER_SA_PASSWORD = "yourStrong(!)Password";


        public UnitTest1()
        {
        }

        public async Task InitializeAsync()
        {
            // (_dockerContainerId, _dockerSqlPort) = await DockerSqlDatabaseUtilities.EnsureDockerStartedAndGetContainerIdAndPortAsync();


            var containerName = "IntegrationTests" + Guid.NewGuid();

            var runspace = RunspaceFactory.CreateRunspace();
            runspace.Open();
            var pipeline = runspace.CreatePipeline();
            pipeline.Commands.AddScript($"docker run --name {containerName}  -e 'ACCEPT_EULA=Y' -e 'SA_PASSWORD={SQLSERVER_SA_PASSWORD}' -p 1433:1433 -d mcr.microsoft.com/mssql/server:2017-latest");
            pipeline.Commands.Add("Out-String");
            var results = pipeline.Invoke();
            runspace.Close();

            var dale = results.First();
             _dockerContainerId = results.First().ToString().Replace("\r\n","");

            Console.WriteLine("ContainerId: " _dockerContainerId);

            await WaitUntilDatabaseAvailableAsync("1433");
        }

        [Fact]
        public void Test1()
        {

        }

        private static async Task WaitUntilDatabaseAvailableAsync(string databasePort)
        {
            var start = DateTime.UtcNow;
            const int maxWaitTimeSeconds = 300;
            var connectionEstablised = false;
            while (!connectionEstablised && start.AddSeconds(maxWaitTimeSeconds) > DateTime.UtcNow)
            {
                try
                {
                    var sqlConnectionString = $"Data Source=localhost,{databasePort};Integrated Security=False;User ID=SA;Password={SQLSERVER_SA_PASSWORD}";
                    using var sqlConnection = new SqlConnection(sqlConnectionString);
                    await sqlConnection.OpenAsync();
                    connectionEstablised = true;

                    Console.WriteLine("Conexão aberta!");
                }
                catch
                {
                    // If opening the SQL connection fails, SQL Server is not ready yet
                    await Task.Delay(500);
                }
            }

            if (!connectionEstablised)
            {
                throw new Exception("Connection to the SQL docker database could not be established within 60 seconds.");
            }

            return;
        }

        public Task DisposeAsync()
        {
            return DockerSqlDatabaseUtilities.EnsureDockerStoppedAndRemovedAsync(_dockerContainerId);
        }
    }
}
