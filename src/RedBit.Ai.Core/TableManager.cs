using Microsoft.Azure.Cosmos.Table;
using RedBit.Ai.Models;
using System;
using System.Threading.Tasks;

// TODO : this is hardcoded to work for demo, might want to refactor if you want to make generic or something else

namespace RedBit.Ai.Core
{
    /// <summary>
    /// Manages access to the table for image processing
    /// </summary>
    public class TableManager
    {
        private readonly string _connectionString;
        private readonly string _imageTableName = "imagetable";

        public TableManager(string connectionString)
        {
            _connectionString = connectionString;
        }

        private async Task Initialzie()
        {
            if (StorageAccount == null)
            {
                // Retrieve storage account from connection string.
                StorageAccount = CloudStorageAccount.Parse(_connectionString);

                // Create the table client
                TableClient = StorageAccount.CreateCloudTableClient();

                // Retrieve reference to a previously created container.
                CloudTable = TableClient.GetTableReference(_imageTableName);

                // Create the container if it doesn't already exist.
                await CloudTable.CreateIfNotExistsAsync();
            }
        }

        public CloudStorageAccount StorageAccount { get; private set; }
        public CloudTableClient TableClient { get; set; }
        public CloudTable CloudTable { get; private set; }

        public async Task<ImageEntity> AddOriginalImage(string url)
        {
            // initialize
            await Initialzie();

            // compose the record and add it
            var record = new ImageEntity { OriginalImageUrl = url };

            // todo add some handling for invalid results
            await CloudTable.ExecuteAsync(TableOperation.Insert(record));

            // return the row key
            return record;
        }

        public async Task UpdateRecord(ImageEntity imageEntity)
        {
            // initialize
            await Initialzie();
            // TODO should check the response and handle appropriately
            await CloudTable.ExecuteAsync(TableOperation.Merge(imageEntity));
        }

        public async Task UpdateRecord(string id, Action<ImageEntity> mergeRecordCallback)
        {
            await Initialzie();

            var result = await CloudTable.ExecuteAsync(TableOperation.Retrieve<ImageEntity>(ImageEntity.PARTITION_KEY, id));
            if (result.HttpStatusCode == (int)System.Net.HttpStatusCode.OK)
            {
                ImageEntity entity = result.Result as ImageEntity;
                if (entity != null)
                {
                    mergeRecordCallback(entity);
                    // TODO should check the response and handle appropriately
                    await CloudTable.ExecuteAsync(TableOperation.Merge(entity));
                }
            }
        }
    }
}
