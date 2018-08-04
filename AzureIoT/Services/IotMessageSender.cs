using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;

namespace AzureIoT.Services
{
    public class IotMessageSender
    {
        public static Dictionary<string, string> DeviceToConnectionString = 
            new Dictionary<string, string>
            {
                {"AlphaIotDevice", "{Iot Device Connection String One}" },
                {"BetaIotDevice", "{Iot Device Connection String Two}" },
                {"CharlieIotDevice", "{Iot Device Connection String Three}" }
            };
        
        public static async void SendDeviceToCloudMessagesAsync(string deviceId, string message)
        {
            DeviceClient client = ConnectToDevice(deviceId);
            await client.SendEventAsync(new Message(Encoding.ASCII.GetBytes(message)));
        }

        private static DeviceClient ConnectToDevice(string deviceId)
        {
            string connectionString = DeviceToConnectionString[deviceId];
            return DeviceClient.CreateFromConnectionString(connectionString, TransportType.Mqtt);
        }
    }
}
