using Domain.Aggregates.Users;
using Domain.SeedWork;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.Files
{
    public class File : Entity
    {
        private File()
        {
            Id = ObjectId.GenerateNewId();
        }

        public int ImportStatus { get; set; }
        public List<ImportError> ImportErrors { get; set; }
        public int TotalUsers { get; set; }
        public ImportedUsers NewUsers { get; set; }
        public ImportedUsers UpdatedUsers { get; set; }
        public ImportedUsers BlockedUsers { get; set; }

        public class ImportError
        {
            public string ImportAction { get; set; }
            public string Name { get; set; }
            public string Username { get; set; }
            public long? Cge { get; set; }
            public string ImportErrorString { get; set; }
        }

        public class ImportUser
        {
            public string ImgUrl { get; set; }
            public string Name { get; set; }
            public string Rank { get; set; }
            public string Responsible { get; set; }
            public string Status { get; set; }

            public ImportUser (string name, string rank, string responsible, string status, string imgUrl = "")
            {
                Name = name;
                Rank = rank;
                Responsible = responsible;
                Status = status;
                ImgUrl = imgUrl;
            }
        }

        public class ImportedUsers
        {
            public List<ImportUser> Users { get; set; }
            public int Quantity { get; set; }
        }

        public static Result<File> Create(int totalUsers, ImportedUsers newUsers, ImportedUsers updatedUsers, ImportedUsers blockedUsers, List<ImportError> importErrors)
        {
            var newObject = new File()
            {
                ImportStatus = importErrors.Count > 0 ? 0 : 1,
                TotalUsers = totalUsers,
                NewUsers = newUsers,
                UpdatedUsers = updatedUsers,
                BlockedUsers = blockedUsers,
                ImportErrors = importErrors
            };
            return Result.Ok(newObject);
        }
     }
}