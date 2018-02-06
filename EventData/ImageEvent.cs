using System;

namespace EventData
{
    public class ImageEvent
    {
        public string Id { get; set; }
        public string EventType { get; set; }
        public string Subject { get; set; }
        public string EventTime { get; set; }

        public ImageData Data { get; set; }

        public ImageEvent()
        {
            Id = Guid.NewGuid().ToString();
            EventType = "WebImage";
            EventTime = DateTimeOffset.Now.ToString("o");
        }

        public ImageEvent(Uri uri) : this()
        {
            
            Subject = uri.ToString();
            Data = new ImageData { Url = Subject };
        }
    }
}
