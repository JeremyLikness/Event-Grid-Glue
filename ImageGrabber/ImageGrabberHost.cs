using EventData;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage;
using Newtonsoft.Json;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Configuration;
using System.Net.Http;
using System.Threading.Tasks;

namespace ImageGrabber
{
    public static class ImageGrabberHost
    {
        [FunctionName("ImageGrabber")]
        public static async Task Run(
            [EventGridTrigger()]EventGridEvent eventPayload, 
            [Table("images")]CloudTable imageTable,
            TraceWriter log)
        {
            log.Info($"Received event: {eventPayload}");
            var imageData = eventPayload.Data.ToObject<ImageData>();
            if (imageData != null)
            {
                log.Info($"Request to process URL: {imageData.Url}.");
                if (Uri.TryCreate(imageData.Url, UriKind.Absolute, out Uri uri))
                {
                    try
                    {
                        var client = new HttpClient();
                        var result = await client.GetAsync(imageData.Url);

                        var mime = result.Content.Headers.ContentType.MediaType;

                        if (!mime.StartsWith("image/"))
                        {
                            log.Warning($"Not an image: {mime}");
                            return;
                        }

                        var id = Guid.NewGuid().ToString();

                        log.Info("Get storage account.");
                        var storageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["AzureWebJobsStorage"].ToString());

                        log.Info("Create client.");
                        var blobClient = storageAccount.CreateCloudBlobClient();

                        log.Info("Get container reference.");
                        var container = blobClient.GetContainerReference("images");

                        log.Info("Get block blog reference.");
                        var blob = container.GetBlockBlobReference(id);

                        log.Info("Upload image stream.");
                        await blob.UploadFromStreamAsync(await result.Content.ReadAsStreamAsync());
                        log.Info($"Added blob with id {id}");

                        var newEntry = new ImageEntry
                        {
                            PartitionKey = ImageEntry.GetPartitionKey(uri),
                            RowKey = ImageEntry.GetRowKey(uri),
                            BlobId = id,
                            Url = uri.ToString(),
                            MimeType = mime,
                            Caption = string.Empty
                        };
                        log.Info($"Adding table: {JsonConvert.SerializeObject(newEntry)}.");
                        var operation = TableOperation.Insert(newEntry);
                        log.Info("Insert table.");
                        await imageTable.ExecuteAsync(operation);
                        log.Info($"Added table mapping.");
                    }
                    catch(Exception ex)
                    {
                        log.Error("Unexpected exception", ex);
                        throw;
                    }
                }
                else
                {
                    log.Warning($"Bad url: {imageData.Url}");
                }
            }
        }

    }
}
