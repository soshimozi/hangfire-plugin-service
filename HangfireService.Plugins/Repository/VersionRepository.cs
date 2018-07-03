using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using HangfireService.Plugins.Model;


namespace HangfireService.Plugins.Repository
{
    class VersionRepository : IRepository<DocumentVersion, string>
    {
        private readonly string _connectionString;

        const string InsertStatement = "INSERT INTO Versions VALUES(@documentKey, @version)";
        const string DeleteStatement = "DELETE FROM Versions WHERE DocumentKey = @documentKey";
        const string FindByIdStatement = "SELECT DocumentKey, [Version] = DocumentVersion FROM Versions WHERE DocumentKey = @documentKey";
        const string FindAllStatement = "SELECT DocumentKey, [Version] = DocumentVersion FROM Versions";
        const string UpdateStatement = "UPDATE Versions SET DocumentKey = @documentKey, DocumentVersion = @version WHERE DocumentKey = @documentKey";

        public VersionRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task Add(DocumentVersion record)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.ExecuteAsync(InsertStatement, record, commandType: System.Data.CommandType.Text);
            }
        }

        public async Task Delete(DocumentVersion record)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.ExecuteAsync(DeleteStatement, record, commandType: System.Data.CommandType.Text);
            }
        }

        public async Task<IEnumerable<DocumentVersion>> FindAll()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                return await connection.QueryAsync<DocumentVersion>(FindAllStatement, commandType: System.Data.CommandType.Text);
            }
        }

        public async Task<DocumentVersion> FindById(string id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var result = await connection.QueryAsync<DocumentVersion>(FindByIdStatement, new { documentKey = id }, commandType: System.Data.CommandType.Text);
                return result.FirstOrDefault();
            }
        }

        public async Task Update(DocumentVersion record)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.ExecuteAsync(UpdateStatement, record, commandType: System.Data.CommandType.Text);
            }
        }
    }
}
