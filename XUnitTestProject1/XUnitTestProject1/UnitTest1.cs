using System;
using System.Data.SqlClient;
using System.Linq;
using System.Management.Automation.Runspaces;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace XUnitTestProject1
{
    public class UnitTest1 : IAsyncLifetime
    {
        public const string DATABASE_NAME_PLACEHOLDER = "@@databaseName@@";
        private string _dockerContainerId;
        public const string SQLSERVER_SA_PASSWORD = "yourStrong(!)Password";
        private Runspace _runspace;

        public UnitTest1()
        {
        }

        public async Task InitializeAsync()
        {
            // (_dockerContainerId, _dockerSqlPort) = await DockerSqlDatabaseUtilities.EnsureDockerStartedAndGetContainerIdAndPortAsync();


            var containerName = "IntegrationTests" + Guid.NewGuid();
            var freePort = GetFreePort();

            _runspace = RunspaceFactory.CreateRunspace();
            _runspace.Open();
            var pipeline = _runspace.CreatePipeline();
            pipeline.Commands.AddScript($"docker run --name {containerName} -e 'ACCEPT_EULA=Y' -e 'SA_PASSWORD={SQLSERVER_SA_PASSWORD}' -p {freePort}:1433 -d mcr.microsoft.com/mssql/server:2017-latest");
            pipeline.Commands.Add("Out-String");
            var results = pipeline.Invoke();

            var dale = results.First();
            _dockerContainerId = results.First().ToString().Replace("\r\n", "");

            Console.WriteLine("ContainerId: " + _dockerContainerId);

            await WaitUntilDatabaseAvailableAsync(freePort);
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

        private static string GetFreePort()
        {
            var tcpListener = new TcpListener(IPAddress.Loopback, 0);
            tcpListener.Start();
            var port = ((IPEndPoint)tcpListener.LocalEndpoint).Port;
            tcpListener.Stop();
            return port.ToString();
        }

        public Task DisposeAsync()
        {
            var pipeline = _runspace.CreatePipeline();
            pipeline.Commands.AddScript($"docker stop {_dockerContainerId} && docker rm {_dockerContainerId}");
            pipeline.InvokeAsync();

            _runspace.Close();
            Console.WriteLine("Container Excluido");
            return Task.FromResult(0);
        }
    }
}
