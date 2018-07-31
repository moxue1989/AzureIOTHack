using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AzureIoT.Models
{
    public class ChooseDevice
    {
        public string DeviceName { get; set; }
        public SelectList DeviceList { get; set; }
    }
}
