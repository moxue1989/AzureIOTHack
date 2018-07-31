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
        //private static ConcurrentDictionary<string, DeviceClient> _connectionStringToDeviceClients;
        public static Dictionary<string, string> DeviceToConnectionString = 
            new Dictionary<string, string>
            {
                // Add devices and their names here
                {"AlphaIotDevice", "DEVICE_CONNECTION_STRING_ONE" },
                {"BetaIotDevice", "DEVICE_CONNECTION_STRING_ONE" },
                {"CharlieIotDevice", "DEVICE_CONNECTION_STRING_ONE" }
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
