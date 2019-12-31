using Domain.SeedWork;
using MongoDB.Bson;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.UserFiles
{
    public class UserFile : Entity
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string DownloadLink { get; set; }
        public string Resource { get; set; }
        public ObjectId ResourceId { get; set; }

        public static Result<UserFile> Create(
            ObjectId userId, string title, string description, string downloadLink, string resource = null, string resourceId = null
        ) {
            return Result.Ok(
                new UserFile() {
                    Id = ObjectId.GenerateNewId(),
                    CreatedBy = userId,
                    Title = title,
                    Description = description,
                    DownloadLink = downloadLink,
                    Resource = resource,
                    ResourceId = ObjectId.Parse(resourceId)
                }
            );
        }
    }
}