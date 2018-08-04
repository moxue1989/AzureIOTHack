using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace AzureIoT.Models
{
    public class TrainModel
    {
        [Required]
        public string Name { get; set; }
        public List<string> Base64Images { get; set; }
        public List<IFormFile> UploadedImages { get; set; }
    }
}
