using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using GreenEnergyHub.Aggregation.Domain.DTOs.MetaData;

namespace GreenEnergyHub.Aggregation.Infrastructure
{
    public class MetaDataDataAccess : IMetaDataDataAccess
    {
        private readonly string _connectionString;

        public MetaDataDataAccess(string connectionString)
        {
            _connectionString = connectionString;
        }

        public Task CreateJobAsync(Job job)
        {
            throw new System.NotImplementedException();
        }

        public Task UpdateAsync(Job job)
        {
            throw new System.NotImplementedException();
        }

        public Task CreateResultItemAsync(string resultId, Result result)
        {
            throw new System.NotImplementedException();
        }

        public Task UpdateResultItemAsync(Result result)
        {
            throw new System.NotImplementedException();
        }

        private async Task<SqlConnection> GetConnectionAsync()
        {
            var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            return conn;
        }
    }
}
