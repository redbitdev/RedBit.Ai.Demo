using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using System;
using System.IO;
using System.Threading.Tasks;

// TODO : this is hardcoded to work for demo, might want to refactor if you want to make generic or something else

namespace RedBit.Ai.Core
{
    /// <summary>
    /// Manages access to blob
    /// </summary>
    public class BlobManager
    {
        private readonly string _connectionString;
        private readonly string _originalImageBlobContainerName = "originalimagecontainer";

        public  BlobManager(string connectionString)
        {
            _connectionString = connectionString;
        }

        public CloudStorageAccount StorageAccount { get; private set; }

        public CloudBlobClient BlobClient { get; set; }

        public CloudBlobContainer BlobContainer { get; set; }

        private async Task Initialzie()
        {
            if (StorageAccount == null)
            {
                // Retrieve storage account from connection string.
                StorageAccount = CloudStorageAccount.Parse(_connectionString);

                // Create the blob client
                BlobClient = StorageAccount.CreateCloudBlobClient();

                // Retrieve reference to a previously created container.
                BlobContainer = BlobClient.GetContainerReference(_originalImageBlobContainerName);

                // Create the container if it doesn't already exist.
                await BlobContainer.CreateIfNotExistsAsync();
            }
        }

        /// <summary>
        /// Adds original image to blob
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public async Task<string> AddOriginalImage(byte[] buffer)
        {
            // initialize
            await Initialzie();

            // compose the name and upload
            var blobName = $"{Guid.NewGuid().ToString("N")}.png";
            var blob = BlobContainer.GetBlockBlobReference(blobName);
            await blob.UploadFromByteArrayAsync(buffer, 0, buffer.Length);

            // return the filename
            return $"{BlobContainer.StorageUri.PrimaryUri}/{blobName}";
        }
    }
}
