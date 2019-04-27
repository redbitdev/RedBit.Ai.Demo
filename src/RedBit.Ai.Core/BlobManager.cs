﻿using Microsoft.Azure.Storage;
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

                // make access level to blob so urls are public
                var perm = await BlobContainer.GetPermissionsAsync();
                if (perm.PublicAccess != BlobContainerPublicAccessType.Blob)
                {
                    perm.PublicAccess = BlobContainerPublicAccessType.Blob;
                    await BlobContainer.SetPermissionsAsync(perm);
                }
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

        /// <summary>
        /// Adds an extra small image to the container
        /// </summary>
        /// <param name="stream">the stream to add</param>
        /// <param name="url">the url of the original image</param>
        /// <returns>the url</returns>
        public Task<string> AddExtraSmallImage(Stream stream, string url)
        {
            return AddImageToContainer(stream, url, "extrasmallimagecontainer");
        }

        /// <summary>
        /// Adds an small image to the container
        /// </summary>
        /// <param name="stream">the stream to add</param>
        /// <param name="url">the url of the original image</param>
        /// <returns>the url</returns>
        public Task<string> AddSmallImage(Stream stream, string url)
        {
            return AddImageToContainer(stream, url, "smallimagecontainer");
        }

        /// <summary>
        /// Adds an medium image to the container
        /// </summary>
        /// <param name="stream">the stream to add</param>
        /// <param name="url">the url of the original image</param>
        /// <returns>the url</returns>
        public Task<string> AddMediumImage(Stream stream, string url)
        {
            return AddImageToContainer(stream, url, "mediumimagecontainer");
        }

        /// <summary>
        /// Gets the blob from the url as a stream
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public async Task<Stream> GetBlobAsStream(string url)
        {
            // initialize
            await Initialzie();

            try
            {
                // get the original file name
                var blobName = ExtractBlobNameFromUrl(url);

                // download the stream and save into meory stream
                var blob = BlobContainer.GetBlockBlobReference(blobName);
                var ms = new System.IO.MemoryStream();
                await blob.DownloadToStreamAsync(ms);

                // reset it to 0
                ms.Seek(0, SeekOrigin.Begin);

                // return it
                return ms;
            }
            catch
            {
                return null;
            }
        }

        private async Task<string> AddImageToContainer(Stream stream, string url, string containerName)
        {
            // reset it to 0
            stream.Seek(0, SeekOrigin.Begin);

            // initialize
            await Initialzie();

            // compose the name 
            var blobName = ExtractBlobNameFromUrl(url);

            // get the container
            var container = BlobClient.GetContainerReference(containerName);
            await container.CreateIfNotExistsAsync();
            // make access level to blob so urls are public
            var perm = await container.GetPermissionsAsync();
            if (perm.PublicAccess != BlobContainerPublicAccessType.Blob)
            {
                perm.PublicAccess = BlobContainerPublicAccessType.Blob;
                await container.SetPermissionsAsync(perm);
            }

            // upload the blob
            var blob = container.GetBlockBlobReference(blobName);
            await blob.UploadFromStreamAsync(stream);

            // return the filename
            return $"{container.StorageUri.PrimaryUri}/{blobName}";
        }

        private string ExtractBlobNameFromUrl(string url)
        {
            var uri = new Uri(url);
            var blobName = uri.Segments[uri.Segments.Length - 1];
            return blobName;
        }
    }
}
