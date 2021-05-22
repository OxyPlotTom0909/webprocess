using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

using WebProcess.Helper;

namespace WebProcess.CloudStorage
{
    public class TableStorage<T> where T : TableEntity, new()
    {
        private CloudTableClient _tableClient;
        private CloudTable _table;

        protected TableStorage(string connectionString)
        {
            var strageAccount = CloudStorageAccount.Parse(connectionString);
            _tableClient = strageAccount.CreateCloudTableClient();
        }

        protected async Task InitializeAsync(string tableName)
        {
            _table = _tableClient.GetTableReference(tableName);
            await _table.CreateIfNotExistsAsync();
        }

        public static async Task<TableStorage<T>> CreateTable(string connectionString, string tableName)
        {
            var result = new TableStorage<T>(connectionString);
            await result.InitializeAsync(tableName);

            return result;
        }

        public async Task AddAsync(string partitionKey, string rowKey)
        {
            if ((await FindAsync(partitionKey, rowKey)) != null) { return; }

            var newItem = new T() { PartitionKey = partitionKey, RowKey = rowKey };
            await _table.ExecuteAsync(TableOperation.Insert(newItem));
        }

        public async Task AddAsync(T item)
        {
            await UpdateAsync(item);
        }

        public async Task UpdateAsync(T item)
        {
            await _table.ExecuteAsync(TableOperation.InsertOrReplace(item));
        }

        public async Task<T> FindAsync(string partitionKey, string rowKey)
        {
            var ope = TableOperation.Retrieve<T>(partitionKey, rowKey);
            var retrieveResult = await _table.ExecuteAsync(ope);
            if (retrieveResult.Result == null) { return null; }
            return (T)(retrieveResult.Result);
        }

        public async Task<List<T>> FindAsync(string partitionKey)
        {
            var result = new List<T>();
            var query = new TableQuery<T>().Where(TableQuery.GenerateFilterCondition(
                "PartitionKey", QueryComparisons.Equal, partitionKey));

            TableContinuationToken tableContinuationToken = null;
            do
            {
                var queryResponse = await _table.ExecuteQuerySegmentedAsync(query, tableContinuationToken);
                tableContinuationToken = queryResponse.ContinuationToken;
                result.AddRange(queryResponse.Results);
            } while (tableContinuationToken != null);

            return result;
        }

        public async Task<List<T>> FindAsync(string propertName, HelperUtilities.SelectCondition condition, string givenValue)
        {
            string operation = string.Empty;
            switch (condition)
            {
                case HelperUtilities.SelectCondition.Equal:
                    operation = QueryComparisons.Equal;
                    break;
                case HelperUtilities.SelectCondition.GreaterThan:
                    operation = QueryComparisons.GreaterThan;
                    break;
                case HelperUtilities.SelectCondition.GreaterThanOrEqual:
                    operation = QueryComparisons.GreaterThanOrEqual;
                    break;
                case HelperUtilities.SelectCondition.LessThan:
                    operation = QueryComparisons.LessThan;
                    break;
                case HelperUtilities.SelectCondition.LessThanOrEqual:
                    operation = QueryComparisons.LessThanOrEqual;
                    break;
                case HelperUtilities.SelectCondition.NotEqual:
                    operation = QueryComparisons.NotEqual;
                    break;
            }

            var filter = TableQuery.GenerateFilterCondition(propertName, operation, givenValue);
            var result = new List<T>();
            var query = new TableQuery<T>().Where(filter);

            TableContinuationToken tableContinuationToken = null;
            do
            {
                var queryResponse = await _table.ExecuteQuerySegmentedAsync(query, tableContinuationToken);
                tableContinuationToken = queryResponse.ContinuationToken;
                result.AddRange(queryResponse.Results);
            } while (tableContinuationToken != null);

            return result;
        }

        public async Task DeleteAsync(string partitionKey, string rowKey)
        {
            var item = await FindAsync(partitionKey, rowKey);
            if (item == null) { return; }

            var ope = TableOperation.Delete(item);
            await _table.ExecuteAsync(ope);
        }
    }
}
