using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text;
using EventData;

namespace Publisher
{
    class Program
    {
        private static readonly string Key = Environment.GetEnvironmentVariable("EVENT_GRID_KEY");
        private const string Endpoint = "https://imagegrabber.eastus-1.eventgrid.azure.net/api/events";

        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Pass the URL to grab.");
                return;
            }
            if (!Uri.TryCreate(args[0], UriKind.Absolute, out Uri url))
            {
                Console.WriteLine($"{args[0]} is not a valid URL.");
                return;
            }
            SendEvent(url).Wait();
            Console.WriteLine("Successfully published.");
        }

        private static async Task SendEvent(Uri url)
        {
            var content = JsonConvert.SerializeObject(url.ToImageEvent());
            Console.WriteLine(content);
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("aeg-sas-key", Key);
            var httpContent = new StringContent(content, Encoding.UTF8, "application/json");
            var result = await client.PostAsync(Endpoint, httpContent);
            var resultText = await result.Content.ReadAsStringAsync();
            Console.WriteLine($"Response: {resultText}.");
        }
    }
}
