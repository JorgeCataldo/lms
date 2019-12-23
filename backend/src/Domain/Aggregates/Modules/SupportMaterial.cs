using Domain.SeedWork;
using MongoDB.Bson;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.Modules
{
    public class SupportMaterial : Entity
    {
        private SupportMaterial(
            string title, string description, string downloadLink, SupportMaterialTypeEnum? type
        ) {
            Id = ObjectId.GenerateNewId();
            Title = title;
            Description = description;
            DownloadLink = downloadLink;
            Type = type;
        }

        public string Title { get; set; }
        public string Description { get; set; }
        public string DownloadLink { get; set; }
        public SupportMaterialTypeEnum? Type { get; set; }

        public static Result<SupportMaterial> Create(
            string title, string description, string downloadLink, SupportMaterialTypeEnum? type
        ) {
            if (title.Length > 200)
                return Result.Fail<SupportMaterial>($"Tamanho máximo do título do assunto é de 200 caracteres. ({title})");

            var newObject = new SupportMaterial(title, description, downloadLink, type);

            return Result.Ok(newObject);
        }

        public enum SupportMaterialTypeEnum
        {
            Link = 1,
            File = 2
        }
    }
}