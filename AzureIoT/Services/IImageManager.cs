using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace AzureIoT.Services
{
    public interface IImageManager
    {
        Task<string> UploadImageAsync(MemoryStream stream, string imageName);
    }
}
