using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AzureIoT.Models;
using AzureIoT.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AzureIoT.Controllers
{
    [Produces("application/json")]
    [Route("api/Image")]
    public class ImageController : Controller
    {
        private readonly IImageManager _imageManager;

        public ImageController(IImageManager imageManager)
        {
            _imageManager = imageManager;
        }

        [HttpPost]
        public async Task<string> Post([FromBody] ImagePost image)
        {
            byte[] bytes = Convert.FromBase64String(image.ImageData);
            using (MemoryStream stream = new MemoryStream(bytes))
            {
                return await _imageManager.UploadImageAsync(stream, "Test.png");
            }
        }
    }
}