using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace ImageGrabber
{
    public class ImageEntry : TableEntity  
    {
        public string BlobId { get; set; }
        public string Caption { get; set; }
        public string Url { get; set; }
        public string MimeType { get; set; }

        public static string GetRowKey(Uri uri)
        {
            return uri.AbsolutePath.Replace("/","^");
        }

        public static string GetPartitionKey(Uri uri)
        {
            return uri.DnsSafeHost;
        }
    }

    
}
