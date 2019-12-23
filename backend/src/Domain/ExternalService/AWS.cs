using Amazon;
using Amazon.S3;
using Amazon.S3.Transfer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Domain.ExternalService
{
    public class AWS
    {
        public static async Task<string> UploadFileToS3(IFormFile file, IConfiguration configuration)
        {
            
            using (var client = new AmazonS3Client(configuration[$"AWS:AWS"], configuration[$"AWS:SecretAccessKey"], RegionEndpoint.USEast1))
            {

                var contentHtmlId = ObjectId.GenerateNewId().ToString();
                string fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                //var filePath = Path.GetTempFileName();

                using (var stream = file.OpenReadStream())
                using (var archive = new ZipArchive(stream))
                {
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        if (!entry.FullName.EndsWith("/") && !string.IsNullOrEmpty(entry.Name))
                        {
                            var newEntry = entry;
                            
                            using (var contentStream = entry.Open())
                            using (var ms = new MemoryStream())
                            {
                                contentStream.CopyTo(ms);
                                var contentType = string.Empty;

                                if (entry.Name.Contains(".png"))
                                    contentType = "image/png";
                                else if (entry.Name.Contains(".jpg") || entry.Name.Contains(".jpeg"))
                                    contentType = "image/jpeg";
                                else if (entry.Name.Contains(".gif"))
                                    contentType = "image/gif";
                                else if (entry.Name.Contains(".css"))
                                    contentType = "text/css";
                                else if (entry.Name.Contains(".js"))
                                    contentType = "application/javascript";
                                else if (entry.Name.Contains(".swf"))
                                    contentType = "application/x-shockwave-flash";
                                else contentType = "text/html";

                                var uploadRequest = new TransferUtilityUploadRequest
                                {
                                    InputStream = ms,
                                    Key = string.Format("{0}/{1}", contentHtmlId, entry.FullName),
                                    BucketName = configuration[$"AWS:BucketName"],
                                    CannedACL = S3CannedACL.PublicRead,
                                    ContentType = contentType                                    
                                };

                                var fileTransferUtility = new TransferUtility(client);
                                await fileTransferUtility.UploadAsync(uploadRequest);

                                //bytes = ms.ToArray();
                            }
                        }
                    }
                }
                var url = string.Format("{0}/{1}/index.html", configuration[$"AWS:HostUrl"], contentHtmlId );
                return url;
            }
        }

    }
}
