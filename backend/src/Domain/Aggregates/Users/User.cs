using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Aggregates.UserFiles;
using Domain.SeedWork;
using Domain.ValueObjects;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Tg4.Infrastructure.Functional;

namespace Domain.Aggregates.Users
{
    [BsonDiscriminator(RootClass = true)]
    [BsonKnownTypes(typeof(Student), typeof(HumanResources), typeof(Admin), typeof(Secretary), typeof(Recruiter), typeof(BusinessManager), typeof(Author))]
    public abstract class User: Entity, IAggregateRoot
    {
        public string UserName { get; set; }
        public string Name { get; set; }
        public string Email{ get; set; }
        public string Phone { get; set; }
        public string Phone2 { get; set; }
        public string NormalizedUsername { get; set; }
        public bool IsBlocked { get; set; }
        public Guid RefreshToken { get; set; }
        public string PasswordHash { get; set; }
        [BsonIgnore]
        public string Password { get; set; }
        public bool IsEmailConfirmed { get; set; }
        public string NormalizedEmail { get; set; }
        public IList<string> Roles { get; set; } = new List<string>();
        public string Cpf { get; set; }
        public Address Address { get; set; }
        public DateTimeOffset? BirthDate { get; set; }
        public string ImageUrl { get; set; }
        public ObjectId ResponsibleId { get; set; }
        public string RegistrationId { get; set; }
        public string Info { get; set; }
        public List<UserProgress> ModulesInfo { get; set; }
        public List<UserProgress> TracksInfo { get; set; }
        public List<UserProgress> EventsInfo { get; set; }
        public List<string> AcquiredKnowledge { get; set; }
        public string LineManager { get; set; }
        public string LineManagerEmail { get; set; }
        public DateTimeOffset ServiceDate { get; set; }
        public string Education { get; set; }
        public string Gender { get; set; }
        public bool? Manager { get; set; }
        public long? Cge { get; set; }
        public long? IdCr { get; set; }
        public string CoHead { get; set; }
        public RelationalItem Company { get; set; }
        public RelationalItem BusinessGroup { get; set; }
        public RelationalItem BusinessUnit { get; set; }
        public RelationalItem Country { get; set; }
        public string Language { get; set; }
        public RelationalItem FrontBackOffice { get; set; }
        public RelationalItem Job { get; set; }
        public RelationalItem Location { get; set; }
        public RelationalItem Rank { get; set; }
        public List<RelationalItem> Sectors { get; set; }
        public RelationalItem Segment { get; set; }
        public List<WrongConcept> WrongConcepts { get; set; }
        public bool FirstAccess { get; set; }
        public bool EmailVerified { get; set; }
        public long? EcommerceId { get; set; }
        public DocumentType? Document { get; set; }
        public string DocumentNumber { get; set; }
        public string DocumentEmitter { get; set; }
        public DateTimeOffset? EmitDate { get; set; }
        public DateTimeOffset? ExpirationDate { get; set; }
        public bool? SpecialNeeds { get; set; }
        public string SpecialNeedsDescription { get; set; }
        public string LinkedIn { get; set; }
        public UserProfile Profile { get; set; }
        public FinancialResponsibleInfo FinancialResponsible { get; set; }
        public bool? AllowRecommendation { get; set; }
        public bool? SecretaryAllowRecommendation { get; set; }
        public List<UserFile> Files { get; set; }
        public bool? ForumActivities { get; set; }
        public string ForumEmail { get; set; }
        public string LinkedInId { get; set; }
        public DateTimeOffset? DateBorn { get; set; }


        public class UserProgress
        {
            private UserProgress(
                ObjectId id, int level, decimal percentage, string name, int? validFor, bool videoViewed = false, bool blocked = false
            ) {
                Id = id;
                Level = level;
                Percentage = percentage;
                Name = name;
                ViewedMandatoryVideo = videoViewed;
                Blocked = blocked;
                CreatedAt = DateTimeOffset.Now;
                ValidFor = validFor;
                DueDate = validFor != null && validFor > 0 ? DateTimeOffset.Now.AddDays((double)validFor) : (DateTimeOffset?)null;
            }


            public ObjectId Id { get; set; }
            public string Name { get; set; }
            public int Level { get; set; }
            public decimal Percentage { get; set; }
            public bool? ViewedMandatoryVideo { get; set; }
            public bool? Blocked { get; set; }
            public DateTimeOffset? CreatedAt { get; set; }
            public int? ValidFor { get; set; }
            public DateTimeOffset? DueDate { get; set; }

            public static Result<UserProgress> Create(
                ObjectId moduleId, int level, decimal percentage, string name, int? validFor, bool videoViewed = false
            ) {

                if (percentage < 0 || percentage > 1)
                    return Result.Fail<UserProgress>("Percentual deve ser maior que zero e menor ou igual a 1");

                var levels = Levels.Level.GetAllLevels().Data;
                if (!levels.Any(x => x.Id == level))
                    return Result.Fail<UserProgress>("Nivel de dificuldade não existe");

                var newObject = new UserProgress(moduleId, level, percentage, name, validFor, videoViewed);
                return Result.Ok(newObject);
            }
        }

        public class RelationalItem
        {
            public ObjectId Id { get; set; }
            public string Name { get; set; }
            public RelationalItem(ObjectId id, string name)
            {
                Id = id;
                Name = name;
            }
        }

        public class WrongConcept
        {
            public string Concept { get; set; }
            public ObjectId ModuleId { get; set; }
            public string ModuleName { get; set; }
            public int Count { get; set; }
        }

        public class UserProfile
        {
            public string Title { get; set; }
            public string BiasOne { get; set; }
            public string BiasTwo { get; set; }
        }

        public class FinancialResponsibleInfo
        {
            public string Name { get; set; }
            public string Surname { get; set; }
            public string FullName { get; set; }
            public string Type { get; set; }
            public string CPF { get; set; }
            public string CNPJ { get; set; }
            public string CompanyName { get; set; }
            public Address Address { get; set; }
            public string Email { get; set; }
            public string Phone { get; set; }
            public string MobilePhone { get; set; }
        }

        public User(string userName, string name, string email, string normalizedUserName, bool isBlocked)
        {
            UserName = userName;
            Name = name;
            Email = email;
            NormalizedUsername = normalizedUserName;
            IsBlocked = isBlocked;
        }

        public static ObjectId StringToObjectId(string str)
        {
            return string.IsNullOrEmpty(str) ? ObjectId.Empty : ObjectId.Parse(str);
        }

        public static Result<User> CreateUser(string name, string email, string userName, string registrationId, string cpf, 
            string responsible, string lineManager, string info, string phone, string imageUrl, string role, RelationalItem businessGroup = null, 
            RelationalItem businessUnit = null, RelationalItem country = null, RelationalItem frontBack = null, RelationalItem job = null,
            RelationalItem location = null, RelationalItem rank = null, List<RelationalItem> sectors = null, RelationalItem segment = null)
        {
            User newObj = null;
            
            switch (role)
            {
                case "Student":
                    newObj = new Student(userName, name, email, userName.ToUpper(), false)
                    {
                        RegistrationId = registrationId,
                        Cpf = cpf,
                        ResponsibleId = string.IsNullOrEmpty(responsible) ? ObjectId.Empty : ObjectId.Parse(responsible),
                        LineManager = lineManager,
                        Info = info,
                        Phone = phone,
                        ImageUrl = imageUrl,
                        Country = country,
                        Location = location,
                        BusinessGroup = businessGroup,
                        BusinessUnit = businessUnit,
                        FrontBackOffice = frontBack,
                        Job = job,
                        Rank = rank,
                        Sectors = sectors,
                        Segment = segment,
                        FirstAccess = true,
                        EmailVerified = true
                    };
                    break;
                case "Admin":
                    newObj = new Admin(userName, name, email, userName.ToUpper(), false)
                    {
                        RegistrationId = registrationId,
                        Cpf = cpf,
                        ResponsibleId = string.IsNullOrEmpty(responsible) ? ObjectId.Empty : ObjectId.Parse(responsible),
                        LineManager = lineManager,
                        Info = info,
                        Phone = phone,
                        ImageUrl = imageUrl,
                        Country = country,
                        Location = location,
                        BusinessGroup = businessGroup,
                        BusinessUnit = businessUnit,
                        FrontBackOffice = frontBack,
                        Job = job,
                        Rank = rank,
                        Sectors = sectors,
                        Segment = segment,
                        FirstAccess = false,
                        EmailVerified = true
                    };
                    break;
                case "HumanResources":
                    newObj = new HumanResources(userName, name, email, userName.ToUpper(), false)
                    {
                        RegistrationId = registrationId,
                        Cpf = cpf,
                        ResponsibleId = string.IsNullOrEmpty(responsible) ? ObjectId.Empty : ObjectId.Parse(responsible),
                        LineManager = lineManager,
                        Info = info,
                        Phone = phone,
                        ImageUrl = imageUrl,
                        Country = country,
                        Location = location,
                        BusinessGroup = businessGroup,
                        BusinessUnit = businessUnit,
                        FrontBackOffice = frontBack,
                        Job = job,
                        Rank = rank,
                        Sectors = sectors,
                        Segment = segment,
                        FirstAccess = false,
                        EmailVerified = true
                    };
                    break;
                case "Secretary":
                    newObj = new Secretary(userName, name, email, userName.ToUpper(), false)
                    {
                        RegistrationId = registrationId,
                        Cpf = cpf,
                        ResponsibleId = string.IsNullOrEmpty(responsible) ? ObjectId.Empty : ObjectId.Parse(responsible),
                        LineManager = lineManager,
                        Info = info,
                        Phone = phone,
                        ImageUrl = imageUrl,
                        Country = country,
                        Location = location,
                        BusinessGroup = businessGroup,
                        BusinessUnit = businessUnit,
                        FrontBackOffice = frontBack,
                        Job = job,
                        Rank = rank,
                        Sectors = sectors,
                        Segment = segment,
                        FirstAccess = false,
                        EmailVerified = true
                    };
                    break;
                case "Recruiter":
                    newObj = new Recruiter(userName, name, email, userName.ToUpper(), false)
                    {
                        RegistrationId = registrationId,
                        Cpf = cpf,
                        ResponsibleId = string.IsNullOrEmpty(responsible) ? ObjectId.Empty : ObjectId.Parse(responsible),
                        LineManager = lineManager,
                        Info = info,
                        Phone = phone,
                        ImageUrl = imageUrl,
                        Country = country,
                        Location = location,
                        BusinessGroup = businessGroup,
                        BusinessUnit = businessUnit,
                        FrontBackOffice = frontBack,
                        Job = job,
                        Rank = rank,
                        Sectors = sectors,
                        Segment = segment,
                        FirstAccess = true,
                        EmailVerified = true
                    };
                    break;
                case "BusinessManager":
                    newObj = new BusinessManager(userName, name, email, userName.ToUpper(), false)
                    {
                        RegistrationId = registrationId,
                        Cpf = cpf,
                        ResponsibleId = string.IsNullOrEmpty(responsible) ? ObjectId.Empty : ObjectId.Parse(responsible),
                        LineManager = lineManager,
                        Info = info,
                        Phone = phone,
                        ImageUrl = imageUrl,
                        Country = country,
                        Location = location,
                        BusinessGroup = businessGroup,
                        BusinessUnit = businessUnit,
                        FrontBackOffice = frontBack,
                        Job = job,
                        Rank = rank,
                        Sectors = sectors,
                        Segment = segment,
                        FirstAccess = true,
                        EmailVerified = true
                    };
                    break;
                case "Author":
                    newObj = new Author(userName, name, email, userName.ToUpper(), false)
                    {
                        RegistrationId = registrationId,
                        Cpf = cpf,
                        ResponsibleId = string.IsNullOrEmpty(responsible) ? ObjectId.Empty : ObjectId.Parse(responsible),
                        LineManager = lineManager,
                        Info = info,
                        Phone = phone,
                        ImageUrl = imageUrl,
                        Country = country,
                        Location = location,
                        BusinessGroup = businessGroup,
                        BusinessUnit = businessUnit,
                        FrontBackOffice = frontBack,
                        Job = job,
                        Rank = rank,
                        Sectors = sectors,
                        Segment = segment,
                        FirstAccess = true,
                        EmailVerified = true
                    };
                    break;
                default:
                    return Result.Fail<User>("Role nao existe");
            }

            return Result.Ok(newObj);
        }

        public static Result<User> CreateUserFromFile(ObjectId id, string name, string username, string email, string lineManager, string lineManagerEmail, DateTimeOffset serviceDate,
            string education, string gender, bool? manager, long? cge, long? idCr, string coHead, RelationalItem company, RelationalItem businessGroup, RelationalItem businessUnit,
            RelationalItem country, string language, RelationalItem frontBackOffice, RelationalItem Job, RelationalItem location, RelationalItem rank, List<RelationalItem> sectors, 
            RelationalItem segment)
        {
            User newObj = new Student(username, name, email, username.ToUpper(), false)
            {
                Id = id,
                LineManager = lineManager,
                LineManagerEmail = lineManagerEmail,
                ServiceDate = serviceDate,
                Education = education,
                Gender = gender,
                Manager = manager,
                Cge = cge,
                IdCr = idCr,
                CoHead = coHead,
                Company = company,
                BusinessGroup = businessGroup,
                BusinessUnit = businessUnit,
                Country = country,
                Language = language,
                FrontBackOffice = frontBackOffice,
                Job = Job,
                Location = location,
                Rank = rank,
                Sectors = sectors,
                Segment = segment,
                FirstAccess = true,
                EmailVerified = true
            };

            return Result.Ok(newObj);
        }

        public Result Edit(string name, string email, string cpf, 
            Address address)
        {
            var emailResult = ValueObjects.Email.Create(email);

            if (emailResult.IsFailure)
                return emailResult;

            if (string.IsNullOrWhiteSpace(name))
                return Result.Fail("Nome não pode ser vazio");

            Name = name;
            Email = emailResult.Data.Value;
            Cpf = cpf;
            Address = address;

            return Result.Ok();
        }

        public abstract string GetUserRole(User user);

        public enum DocumentType
        {
            Rg = 1,
            Cnh = 2
        }
    }
}