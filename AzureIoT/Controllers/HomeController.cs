﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AzureIoT.Models;
using AzureIoT.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using Microsoft.Rest;
using Newtonsoft.Json;

namespace AzureIoT.Controllers
{
    public class HomeController : Controller
    {
        public static string FaceApiKey = "{KEY_1}";
        public static string FaceApiKeyAlt = "{ALTERNATE_KEY}";
        public static string BaseUri = "https://canadacentral.api.cognitive.microsoft.com/face/v1.0";
        private readonly IImageManager _imageManager;

        readonly FaceClient _client = new FaceClient(new ApiKeyServiceClientCredentials(FaceApiKey))
        {
            BaseUri = new Uri(BaseUri)
        };

        readonly FaceClient _altClient = new FaceClient(new ApiKeyServiceClientCredentials(FaceApiKeyAlt))
        {
            BaseUri = new Uri(BaseUri)
        };

        private IList<Person> _persons;

        public HomeController(IImageManager imageManager)
        {
            _imageManager = imageManager;
        }

        public IActionResult Index()
        {
            ChooseDevice chooseDevice = new ChooseDevice()
            {
                DeviceList = new SelectList(IotMessageSender.DeviceToConnectionString.Keys)
            };
            return View(chooseDevice);
        }

        [HttpPost]
        public IActionResult Index(ChooseDevice device)
        {
            if (!IotMessageSender.DeviceToConnectionString.Keys.Contains(device.DeviceName))
            {
                return Index();
            }
            return RedirectToAction("Detect", "home", new { deviceName = device.DeviceName });
        }

        public IActionResult Detect(string deviceName)
        {
            if (deviceName == null)
            {
                return RedirectToAction("Index");
            }
            ImagePost imagePost = new ImagePost()
            {
                DeviceName = deviceName
            };
            return View(imagePost);
        }

        [HttpPost]
        public async Task<IActionResult> Detect([FromBody] ImagePost image)
        {
            byte[] bytes = Convert.FromBase64String(image.ImageData);
            IList<DetectedFace> detectedFaces;
            using (MemoryStream stream = new MemoryStream(bytes))
            {
                HttpOperationResponse<IList<DetectedFace>> response =
                    await _altClient.Face.DetectWithStreamWithHttpMessagesAsync(stream);
                detectedFaces = response.Body;
            }
            if (detectedFaces != null && detectedFaces.Count > 0)
            {
                using (MemoryStream stream = new MemoryStream(bytes))
                {
                    HttpOperationResponse<IList<DetectedFace>> response =
                        await _client.Face.DetectWithStreamWithHttpMessagesAsync(stream);
                    detectedFaces = response.Body;
                }
                List<Guid> guids = detectedFaces
                    .Where(d => d.FaceId.HasValue)
                    .Select(d => d.FaceId.Value)
                    .ToList();
                HttpOperationResponse<IList<IdentifyResult>> identifyResult = await _client.Face.IdentifyWithHttpMessagesAsync("1", guids);

                List<DetectedPerson> foundPersons = identifyResult.Body
                    .Select(r => GetPersonFromResult(r, GetPersons().ToList()))
                    .Where(dp => dp != null)
                    .ToList();
                string url;
                DateTime dateTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId
                    (DateTime.UtcNow, "Mountain Standard Time");

                using (MemoryStream stream = new MemoryStream(bytes))
                {
                    url = await _imageManager.UploadImageAsync(stream, image.DeviceName + dateTime.ToString("yyMMddHHmmss") + ".png");
                }

                DetectionResults results = new DetectionResults
                {
                    Persons = foundPersons,
                    Url = url,
                    DeviceName = image.DeviceName,
                    DateTime = dateTime.ToString("G")
                };
                
                string serializeObject = JsonConvert.SerializeObject(results);
                IotMessageSender.SendDeviceToCloudMessagesAsync(image.DeviceName, serializeObject);
                return Ok(results.ToString());
            }
            return NotFound();

        }

        private IList<Person> GetPersons()
        {
            if (_persons == null)
            {
                UpdatePersons();
            }
            return _persons;
        }

        private DetectedPerson GetPersonFromResult(IdentifyResult result, List<Person> persons)
        {
            if (result.Candidates.Count == 0)
            {
                return null;
            }
            IdentifyCandidate candidate = result.Candidates
                .OrderByDescending(c => c.Confidence)
                .First();

            Person person = persons.FirstOrDefault(p => p.PersonId == candidate.PersonId);

            return person == null ? null : new DetectedPerson()
            {
                Name = person.Name,
                Confidence = candidate.Confidence,
            };
        }

        public IActionResult Persons()
        {
            if (_persons == null)
            {
                UpdatePersons();
            }
            return View(_persons);
        }

        private void UpdatePersons()
        {
            HttpOperationResponse<IList<Person>> response = _client
                .PersonGroupPerson
                .ListWithHttpMessagesAsync("1").Result;
            _persons = response.Body;
        }

        public IActionResult Train()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Train(TrainModel model)
        {
            HttpOperationResponse<Person> response = await _client.PersonGroupPerson.CreateWithHttpMessagesAsync("1", model.Name);

            if (model.Base64Images != null)
            {
                foreach (string imageString in model.Base64Images)
                {
                    byte[] bytes = Convert.FromBase64String(imageString);
                    using (MemoryStream stream = new MemoryStream(bytes))
                    {
                        try
                        {
                            await _client.PersonGroupPerson.AddPersonFaceFromStreamWithHttpMessagesAsync("1",
                                response.Body.PersonId,
                                stream);
                        }
                        catch (Exception)
                        {
                            // ignored
                        }
                    }
                }
            }

            if (model.UploadedImages != null)
            {
                foreach (IFormFile uploadedImage in model.UploadedImages)
                {
                    BinaryReader reader = new BinaryReader(uploadedImage.OpenReadStream());
                    var readBytes = reader.ReadBytes((int)uploadedImage.Length);

                    using (MemoryStream stream = new MemoryStream(readBytes))
                    {
                        await _client.PersonGroupPerson.AddPersonFaceFromStreamWithHttpMessagesAsync("1",
                            response.Body.PersonId,
                            stream);
                    }
                }
            }
            await _client.PersonGroup.TrainWithHttpMessagesAsync("1");
            UpdatePersons();
            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
