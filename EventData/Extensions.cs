using System;
using System.Collections.Generic;
using System.Text;

namespace EventData
{
    public static class Extensions
    {
        public static List<ImageEvent> ToImageEvent(this Uri uri)
        {
            var result = new List<ImageEvent>
            {
                new ImageEvent(uri)
            };
            return result;
        }
    }
}
