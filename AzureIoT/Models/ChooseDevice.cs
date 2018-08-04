using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AzureIoT.Models
{
    public class ChooseDevice
    {
        [Display(Name = "Select An IOT device for this Camera:")]
        public string DeviceName { get; set; }
        [EmailAddress]
        [Display(Name = "Enter an E-mail to Receive notifications (Optional):")]
        public string Email { get; set; }
        public SelectList DeviceList { get; set; }
    }
}
