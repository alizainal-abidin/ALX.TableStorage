using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;

using ALX.TableStorage.Infrastructure.Configuration;
using ALX.TableStorage.Infrastructure.Interface;
using Microsoft.WindowsAzure.Storage;
using ALX.TableStorage.Infrastructure.Utility;

namespace ALX.TableStorage.Infrastructure
{
    public class TableStorageGenericRepository<T> : ITableStorageGenericRepository<T> where T : TableStorageBaseEntity, new()
    {
        #region Members

        private readonly TableStorageConfiguration _configs;

        private readonly Task<CloudTable> _cloudTable;

        #endregion

        #region Constructor

        public TableStorageGenericRepository(TableStorageConfiguration configuration)
        {
            _configs = configuration;
            _cloudTable = GetTableAsync();
        }

        #endregion

        #region ITableStorageGenericRepository

        public async Task<List<T>> All(int? size)
        {            
            // get table.
            var table = await _cloudTable;

            // build query.
            var query = new TableQuery<T> { TakeCount = size };

            // execute query.
            return await ExecuteQuery(table, query);
        }

        public async Task<List<T>> GetListByPartitionKey(string partitionKey)
        {
            // validation.
            if (string.IsNullOrEmpty(partitionKey))
                throw new ArgumentNullException("PartitionKey and RowKey are null.");

            // get table.
            var table = await _cloudTable;

            // build query.
            var query = new TableQuery<T>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey));

            return await ExecuteQuery(table, query);
        }

        public async Task<T> Get(string partitionKey, string rowKey)
        {
            // validation.
            if (string.IsNullOrEmpty(partitionKey) && string.IsNullOrEmpty(rowKey))
                throw new ArgumentNullException("PartitionKey and RowKey are null.");

            // get table.
            var table = await _cloudTable;

            // get by partition-key and row-key.
            if (!string.IsNullOrEmpty(partitionKey) && !string.IsNullOrEmpty(rowKey))
            {
                var operation = TableOperation.Retrieve<T>(partitionKey, rowKey);

                // execute.
                var result = await table.ExecuteAsync(operation);

                return (T)(dynamic)result.Result;
            }
            // get by row-key.
            else if (string.IsNullOrEmpty(partitionKey))
            {
                // build query.
                var query = new TableQuery<T>().Where(TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, rowKey));
                query.TakeCount = 1;

                // execute query.
                var entity = await ExecuteQuery(table, query);

                return entity.FirstOrDefault();
            }

            // no result.
            return null;
        }

        public async Task InsertOrUpdate(T item)
        {
            // validation.
            if (item == null)
                throw new ArgumentNullException($"{typeof(T).Name} is null.");

            // get table.
            var table = await _cloudTable;

            // table operation.
            var operation = TableOperation.InsertOrReplace(item);

            // execute.
            await table.ExecuteAsync(operation);
        }

        public async Task Delete(T item)
        {
            // get table.
            var table = await _cloudTable;

            // table operation.
            var operation = TableOperation.Delete(item);

            // execute.
            await table.ExecuteAsync(operation);
        }

        public async Task Delete(string partitionKey, string rowKey)
        {
            // get item by partition key and/or row key.
            T item = await Get(partitionKey, rowKey);

            // get table.
            var table = await _cloudTable;

            // table operation.
            var operation = TableOperation.Delete(item);

            // execute.
            await table.ExecuteAsync(operation);
        } 

        #endregion

        #region Private Methods

        private async Task<CloudTable> GetTableAsync()
        {
            // connect to Azure table storage account.
            var storageAccount = CloudStorageAccount.Parse(_configs.ConnectionStrings);      
            
            // get table name.
            var tableName = typeof(T).GetAttributeValue((TableStorageAttribute x) => x.TableName) ?? typeof(T).Name;

            // get table.            
            var table = storageAccount.CreateCloudTableClient().GetTableReference(tableName);

            // check whether table already created or not, if yes return it otherwise create new one.
            await table.CreateIfNotExistsAsync();

            return table;
        }

        private static async Task<List<T>> ExecuteQuery(CloudTable table, TableQuery<T> query)
        {
            var results = new List<T>();
            TableContinuationToken continuationToken = null;
            do
            {
                // execute query.
                var queryResults = await table.ExecuteQuerySegmentedAsync(query, continuationToken);

                continuationToken = queryResults.ContinuationToken;
                results.AddRange(queryResults.Results);

            } while (continuationToken != null);

            return results;
        }

        #endregion
    }
}