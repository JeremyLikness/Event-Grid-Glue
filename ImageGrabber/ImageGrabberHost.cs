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
using System.Net;
using Microsoft.Azure.WebJobs.Extensions.Http;
using System.Net.Http.Headers;
using System.Linq;
using System.Collections.Generic;

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

        [FunctionName("Caption")]
        public static async Task<HttpResponseMessage> Caption(
            [HttpTrigger("Post")]HttpRequestMessage req,
            [Table("images")]CloudTable imageTable,
            TraceWriter log)
        {
            dynamic body = await req.Content.ReadAsAsync<object>();
            log.Info($"Received: {JsonConvert.SerializeObject(body)}");
            string href = body?.href;
            string caption = body?.caption;
            if (!string.IsNullOrWhiteSpace(href) && !string.IsNullOrWhiteSpace(caption))
            {
                log.Info($"Caption request: {href} is {caption}.");
                if (Uri.TryCreate(href, UriKind.Absolute, out Uri uri))
                {
                    var partitionKey = ImageEntry.GetPartitionKey(uri);
                    var rowKey = ImageEntry.GetRowKey(uri);
                    var operation = TableOperation.Retrieve<ImageEntry>(partitionKey, rowKey);
                    var result = await imageTable.ExecuteAsync(operation);
                    if (result != null && result.Result is ImageEntry entry)
                    {
                        entry.Caption = caption;
                        operation = TableOperation.Replace(entry);
                        await imageTable.ExecuteAsync(operation);
                        return req.CreateResponse(HttpStatusCode.NoContent);
                    }
                    else
                    {
                        log.Warning("Not found.");
                        return req.CreateResponse(HttpStatusCode.NotFound);
                    }
                }
                else
                {
                    log.Warning("Bad URL.");
                }
            }
            else
            {
                log.Warning("Bad data.");
            }
            return req.CreateResponse(HttpStatusCode.BadRequest);
        }

        [FunctionName("ShowImage")]
        public static async Task<HttpResponseMessage> ShowImage(
            [HttpTrigger(AuthorizationLevel.Anonymous, "Get", Route="ShowImage")]HttpRequestMessage req,
            [Table("images")]CloudTable imageTable,
            TraceWriter log)
        {
            string href = req.GetQueryNameValuePairs().FirstOrDefault(q => string.Compare(q.Key, "href", true) == 0).Value;
            if (!string.IsNullOrWhiteSpace(href))
            {
                log.Info($"Image request: {href}.");
                if (Uri.TryCreate(href, UriKind.Absolute, out Uri uri))
                {
                    var partitionKey = ImageEntry.GetPartitionKey(uri);
                    var rowKey = ImageEntry.GetRowKey(uri);
                    var operation = TableOperation.Retrieve<ImageEntry>(partitionKey, rowKey);
                    var result = await imageTable.ExecuteAsync(operation);
                    if (result != null && result.Result is ImageEntry entry)
                    {
                        log.Info("Get storage account.");
                        var storageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["AzureWebJobsStorage"].ToString());

                        log.Info("Create client.");
                        var blobClient = storageAccount.CreateCloudBlobClient();

                        log.Info("Get container reference.");
                        var container = blobClient.GetContainerReference("images");

                        log.Info("Get block blog reference.");
                        var blob = container.GetBlockBlobReference(entry.BlobId);

                        if (blob.Exists())
                        {
                            var response = req.CreateResponse(HttpStatusCode.OK);
                            response.Content = new StreamContent(await blob.OpenReadAsync());
                            response.Content.Headers.ContentType = new MediaTypeHeaderValue(entry.MimeType);
                            return response;
                        }
                        else
                        {
                            return req.CreateResponse(HttpStatusCode.NotFound);
                        }
                    }
                    else
                    {
                        log.Warning("Not found.");                        
                    }
                }
                else
                {
                    log.Warning("Bad URL.");                    
                }
            }
            return req.CreateResponse(HttpStatusCode.BadRequest);
        }

        [FunctionName("ListImages")]
        public static async Task<HttpResponseMessage> ListImages(
            [HttpTrigger(AuthorizationLevel.Anonymous, "Get", Route ="ListImages")]HttpRequestMessage req,
            [Table("images")]CloudTable table,
            TraceWriter log)
        {
            log.Info("Request to list images.");
            var result = new List<ImageEntry>();
            TableContinuationToken token = null;
            do
            {
                var queryResult = await table.ExecuteQuerySegmentedAsync(new TableQuery<ImageEntry>(), token);
                result.AddRange(queryResult.Results);
                token = queryResult.ContinuationToken;
            } while (token != null);
            return req.CreateResponse(HttpStatusCode.OK, result.Select(r => new { r.Url, r.Caption }));
        }

    }
}
