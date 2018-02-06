using EventData;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Azure.WebJobs.Host;

namespace ImageGrabber
{
    public static class ImageGrabberHost
    {
        [FunctionName("ImageGrabber")]
        public static void Run([EventGridTrigger()]EventGridEvent eventPayload, TraceWriter log)
        {
            log.Info($"Received event: {eventPayload}");
            var imageData = eventPayload.Data.ToObject<ImageData>();
            if (imageData != null)
            {
                log.Info($"Request to process URL: {imageData.Url}.");
            }
        }
    }
}
