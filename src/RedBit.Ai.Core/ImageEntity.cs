using Microsoft.Azure.Cosmos.Table;
using System;

// TODO : this is hardcoded to work for demo, might want to refactor if you want to make generic or something else

namespace RedBit.Ai.Core
{
    /// <summary>
    /// Entity for table storage
    /// </summary>
    public class ImageEntity : TableEntity
    {
        public const string PARTITION_KEY = "images";

        public ImageEntity()
        {
            RowKey = Guid.NewGuid().ToString("N");
            PartitionKey = PARTITION_KEY;
        }

        public string OriginalImageUrl { get; set; }
        public string ExtraSmallImageUrl { get; set; }
        public string SmallImageUrl { get; set; }
        public string MediumImageUrl { get; set; }
        public string ImageAnalyzerResults { get; set; }
    }
}
