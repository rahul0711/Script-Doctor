using MySql.Data.MySqlClient;
using Pharma_Script.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

namespace Pharma_Script.Repositories.Implementations
{
    public abstract class RepositoryBase<T> : IRepository<T> where T : class
    {
        protected readonly MySqlConnection Connection;
        protected readonly Func<MySqlTransaction?> TransactionProvider;

        protected MySqlTransaction? Transaction => TransactionProvider();

        protected RepositoryBase(MySqlConnection connection, Func<MySqlTransaction?> transactionProvider)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            TransactionProvider = transactionProvider ?? throw new ArgumentNullException(nameof(transactionProvider));
        }

        protected abstract string TableName { get; }
        protected abstract string PrimaryKeyName { get; }
        protected abstract T Map(DbDataReader reader);

        protected MySqlCommand CreateCommand(string query)
        {
            var cmd = Connection.CreateCommand();
            cmd.CommandText = query;
            var tx = Transaction;
            if (tx != null)
            {
                cmd.Transaction = tx;
            }
            return cmd;
        }

        protected async Task EnsureConnectionOpenAsync()
        {
            if (Connection.State != ConnectionState.Open)
            {
                await Connection.OpenAsync();
            }
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            var list = new List<T>();
            var query = $"SELECT * FROM {TableName}";
            
            await EnsureConnectionOpenAsync();
            using (var cmd = CreateCommand(query))
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    list.Add(Map(reader));
                }
            }
            return list;
        }

        public virtual async Task<T?> GetByIdAsync(int id)
        {
            var query = $"SELECT * FROM {TableName} WHERE {PrimaryKeyName} = @Id";
            
            await EnsureConnectionOpenAsync();
            using (var cmd = CreateCommand(query))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return Map(reader);
                    }
                }
            }
            return null;
        }

        public abstract Task<int> AddAsync(T entity);
        public abstract Task<bool> UpdateAsync(T entity);

        public virtual async Task<bool> DeleteAsync(int id)
        {
            var query = $"DELETE FROM {TableName} WHERE {PrimaryKeyName} = @Id";
            
            await EnsureConnectionOpenAsync();
            using (var cmd = CreateCommand(query))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                var rows = await cmd.ExecuteNonQueryAsync();
                return rows > 0;
            }
        }
    }
}
