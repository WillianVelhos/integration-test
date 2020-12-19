using System.Threading.Tasks;
using Xunit;

namespace XUnitTestProject1
{
    public abstract class IntegrationTestBase : IAsyncLifetime, IAssemblyFixture<SqlServerDockerCollectionFixture>
    {
        protected readonly SqlServerDockerCollectionFixture _fixture;
        protected TestHelper _testHelper;

        protected IntegrationTestBase(SqlServerDockerCollectionFixture fixture)
        {
            _fixture = fixture;
        }

        public virtual async Task InitializeAsync()
        {
            var sqlConnectionString = _fixture.GetSqlConnectionString();
            _testHelper = new TestHelper(sqlConnectionString);
            await _testHelper.InitializeTestServer();
        }

        public virtual async Task DisposeAsync()
        {
            await _testHelper.CleanupTestsAndDropDatabaseAsync();
        }
    }
}
