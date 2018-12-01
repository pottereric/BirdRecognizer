using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace BirdRecognizerRest
{
    internal class Program
    {
        private static List<string> testImages = new List<string>()
        {
            @"..\..\..\TestImages\bird.jpg",
            @"..\..\..\TestImages\bird2.jpg",
            @"..\..\..\TestImages\dog.jpg",
            @"..\..\..\TestImages\cat.jpg",
            @"..\..\..\TestImages\snake.jpg",
        };

        private static async Task Main(string[] args)
        {
            foreach (var testImage in testImages)
            {
                bool result = await ImageIsBird(testImage, 0.7);
                Console.WriteLine($"{testImage} : {result}");
            }

            Console.WriteLine("Press Any Key To Continue");
            Console.ReadKey();
        }

        private static async Task<bool> ImageIsBird(string imagePath, double confidenceThreshold)
        {
            var tags = await MakeAnalysisRequest(imagePath);
            return tags.Any(t => t.name.ToLower() == "bird");
        }

        #region subsription Information
        private const string subscriptionKey = "-your-subscription-key-";
        private const string uriBase = "-your-cognitive-services-url-";
        #endregion subsription Information

        /// <summary>
        /// Gets the analysis of the specified image file by using the Computer Vision REST API.
        /// </summary>
        /// <param name="imageFilePath">The image file.</param>
        private static async Task<List<Tag>> MakeAnalysisRequest(string imageFilePath)
        {
            HttpClient client = new HttpClient();

            // Request headers.
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionKey);

            // Request parameters. A third optional parameter is "details".
            string requestParameters = "visualFeatures=Tags&language=en";

            // Assemble the URI for the REST API Call.
            string uri = uriBase + "?" + requestParameters;

            HttpResponseMessage response;

            // Request body. Posts a locally stored JPEG image.
            byte[] byteData = GetImageAsByteArray(imageFilePath);

            using (ByteArrayContent content = new ByteArrayContent(byteData))
            {
                // This example uses content type "application/octet-stream".
                // The other content types you can use are "application/json" and "multipart/form-data".
                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

                // Execute the REST API call.
                response = await client.PostAsync(uri, content);

                // Get the JSON response.
                string contentString = await response.Content.ReadAsStringAsync();

                var tagData = JsonConvert.DeserializeObject<TagData>(contentString);

                return tagData.tags;
            }
        }

        /// <summary>
        /// Returns the contents of the specified file as a byte array.
        /// </summary>
        /// <param name="imageFilePath">The image file to read.</param>
        /// <returns>The byte array of the image data.</returns>
        private static byte[] GetImageAsByteArray(string imageFilePath)
        {
            FileStream fileStream = new FileStream(imageFilePath, FileMode.Open, FileAccess.Read);
            BinaryReader binaryReader = new BinaryReader(fileStream);
            return binaryReader.ReadBytes((int)fileStream.Length);
        }
    }
}