using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Aggregates.Modules;
using Domain.Aggregates.Questions;
using Domain.Data;
using Domain.Base;
using Domain.ValueObjects;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Newtonsoft.Json;
using Performance.Domain.Aggregates.AuditLogs;
using Tg4.Infrastructure.Functional;
using Amazon;
using Amazon.S3;
using Amazon.S3.Transfer;
using System.IO;

namespace Domain.Aggregates.ModulesDrafts.Commands
{
    public class UploadModuleDraftContentToS3
    {
        public class Contract : CommandContract<Result<bool>>
        {
            public string ModuleId { get; set; }
            public Stream File { get; set; }
            public List<ContractSubject> Subjects { get; set; }
            public string UserId { get; set; }
            public string UserRole { get; set; }
        }

        public class ContractSubject
        {
            public string Id { get; set; }
            public string[] Concepts { get; set; }
            public string Title { get; set; }
            public string Excerpt { get; set; }
        }


        public class Handler : IRequestHandler<Contract, Result<bool>>
        {
            private readonly IDbContext _db;
            private readonly IMediator _mediator;
            private const string bucketName = "psk-html-content";
            private const string keyName = "*** provide a name for the uploaded object ***";
            private const string filePath = "*** provide the full path name of the file to upload ***";
            // Specify your bucket region (an example region is shown).
            private static readonly RegionEndpoint bucketRegion = RegionEndpoint.USWest1;
            private static IAmazonS3 s3Client;


            public Handler(IDbContext db, IMediator mediator)
            {
                _db = db;
                _mediator = mediator;
            }

            public async Task<Result<bool>> Handle(Contract request, CancellationToken cancellationToken)
            {
                if (request.UserRole == "Secretary")
                    return Result.Fail<bool>("Acesso Negado");

                s3Client = new AmazonS3Client(bucketRegion);
                UploadFileAsync(request.File).Wait();


                return Result.Ok(true);
            }

            private static async Task UploadFileAsync(Stream stream)
            {
                try
                {
                    var fileTransferUtility =
                        new TransferUtility(s3Client);

                        await fileTransferUtility.UploadAsync(stream,
                                                   bucketName, keyName);
                }
                catch (AmazonS3Exception e)
                {
                    Console.WriteLine("Error encountered on server. Message:'{0}' when writing an object", e.Message);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Unknown encountered on server. Message:'{0}' when writing an object", e.Message);
                }

            }
        }
    }
}
