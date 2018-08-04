using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.WebJobs.ServiceBus;
using Newtonsoft.Json;

namespace AzureIoTFunction
{
    public static class AzureIot
    {
        [FunctionName("AzureIot")]
        public static void Run([EventHubTrigger("AzureIotEvent", Connection = "AzureIotHack_events_IOTHUB", ConsumerGroup = "$Default")]string myEventHubMessage, TraceWriter log)
        {
            log.Info($"C# Event Hub trigger function processed a message: {myEventHubMessage}");
            DetectionResults results = (DetectionResults)JsonConvert.DeserializeObject(myEventHubMessage, typeof(DetectionResults));
            SlackMessage message = GetSlackMessage(results);

            HttpClient client = new HttpClient
            {
                BaseAddress = new Uri("{Slack Webhook post endpoint}")
            };

            string serializeObject = JsonConvert.SerializeObject(message);
            log.Info($"Serialized Value: {serializeObject}");
            var httpContent = new StringContent(serializeObject);
            client.PostAsync("", httpContent);
        }

        private static SlackMessage GetSlackMessage(DetectionResults results)
        {
            SlackMessage message = new SlackMessage
            {
                Text = "Detection Notification",
                Attachments = new List<Attachment>
                {
                    new Attachment
                    {
                        Pretext = results.ToString(),
                        Text = $"{results.DeviceName} @ {results.DateTime}",
                        ImageUrl = results.Url
                    }
                }
            };
            return message;
        }
    }
}
