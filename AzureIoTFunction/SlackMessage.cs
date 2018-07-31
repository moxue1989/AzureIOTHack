using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace AzureIoTFunction
{
    public class SlackMessage
    {
        [JsonProperty("text")]
        public string Text { get; set; }
        [JsonProperty("attachments")]
        public List<Attachment> Attachments { get; set; }
    }

    public class Attachment
    {
        [JsonProperty("pretext")]
        public string Pretext { get; set; }
        [JsonProperty("text")]
        public string Text { get; set; }
        [JsonProperty("image_url")]
        public string ImageUrl { get; set; }
    }
}
