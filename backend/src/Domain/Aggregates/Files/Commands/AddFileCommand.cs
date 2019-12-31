using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Domain.Aggregates.Jobs;
using Domain.Aggregates.Ranks;
using Domain.Aggregates.Sectors;
using Domain.Aggregates.Segments;
using Domain.Aggregates.Users;
using Domain.Aggregates.Companies;
using Domain.Aggregates.BusinessGroups;
using Domain.Aggregates.BusinessUnits;
using Domain.Aggregates.FrontBackOffices;
using Domain.Data;
using Domain.Base;
using MediatR;
using Microsoft.AspNetCore.Http;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Tg4.Infrastructure.Functional;
using Microsoft.AspNetCore.Identity;
using Domain.Aggregates.Countries;
using Domain.Aggregates.Locations;
using Microsoft.Extensions.Configuration;

namespace Domain.Aggregates.Files.Commands
{
    public class AddFileCommand
    {
        public class Contract : CommandContract<Result<File>>
        {
            public string SyncAppId { get; set; }
            public string FileContent { get; set; }
        }

        public class UserItem
        {
            public ObjectId Id { get; set; }
            public string Name { get; set; }
            public string UserName { get; set; }
            public string Password { get; set; }
            public string Email { get; set; }
            public string LineManager { get; set; }
            public string LineManagerEmail { get; set; }
            public ObjectId ResponsibleId { get; set; }
            public DateTimeOffset ServiceDate { get; set; }
            public string Education { get; set; }
            public string Gender { get; set; }
            public bool Manager { get; set; }
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
            public LocationRelationalItem Location { get; set; }
            public RelationalItem Rank { get; set; }
            public List<RelationalItem> Sectors { get; set; }
            public RelationalItem Segment { get; set; }
            public bool IsBlocked { get; internal set; }
        }

        public class RelationalItem
        {
            public ObjectId Id { get; set; }
            public string Name { get; set; }
        }

        public class ListRelationalItems
        {
            public List<RelationalItem> TotalItems { get; set; }
            public List<RelationalItem> NewItems { get; set; } = new List<RelationalItem>();
            public List<RelationalItem> BaseItems { get; set; }
        }

        public class LocationRelationalItem
        {
            public ObjectId Id { get; set; }
            public string Name { get; set; }
            public ObjectId CountryId { get; set; }
        }

        public class ListLocationRelationalItems
        {
            public List<LocationRelationalItem> TotalItems { get; set; }
            public List<LocationRelationalItem> NewItems { get; set; } = new List<LocationRelationalItem>();
            public List<LocationRelationalItem> BaseItems { get; set; }
        }

        public class MatrixRelationalItems
        {
            public ListRelationalItems Companies { get; set; }
            public ListRelationalItems BusinessGroups { get; set; }
            public ListRelationalItems BusinessUnits { get; set; }
            public ListRelationalItems FrontBackOffices { get; set; }
            public ListRelationalItems Jobs { get; set; }
            public ListRelationalItems Ranks { get; set; }
            public ListRelationalItems Segments { get; set; }
            public ListRelationalItems Sectors { get; set; }
            public ListRelationalItems Countries { get; set; }
            public ListLocationRelationalItems Locations { get; set; }
            public List<UserItem> Users { get; set; }
        }

        public class FileContent
        {
            public List<UserItem> UsersToAdd { get; set; } = new List<UserItem>();
            public List<UserItem> UsersToUpdate { get; set; } = new List<UserItem>();
            public List<UserItem> UsersToBlock { get; set; } = new List<UserItem>();
            public List<UserItem> UsersToNothing { get; set; } = new List<UserItem>();
        }

        public class Handler : IRequestHandler<Contract, Result<File>>
        {
            private readonly UserManager<User> _userManager;
            private readonly IDbContext _db;
            private readonly  IConfiguration _configuration;

            public Handler(IDbContext db, UserManager<User> userManager,
                IConfiguration configuration)
            {
                _db = db;
                _userManager = userManager;
                _configuration = configuration;
            }

            public async Task<Result<File>> Handle(Contract request, CancellationToken cancellationToken)
            {
                if(!string.IsNullOrEmpty(request.SyncAppId))
                {
                    var appIds = _configuration[$"UserSyncAppIds"]?.Split(";");
                    if(appIds == null || !appIds.Contains(request.SyncAppId))
                        return Result.Fail<File>("App não autorizado");
                }
                var list = ReadAsList(request.FileContent);

                if (list == null)
                    return Result.Fail<File>("Lista nula");

                MatrixRelationalItems relationalItems = await GetCollections(cancellationToken);

                List<UserItem> usersFile = new List<UserItem>();

                for (int i = 1; i < list.Count - 1; i++)
                {
                    var splittedRow = list[i].Split(',');
                    usersFile.Add(ParseToUser(splittedRow));
                }

                List<File.ImportError> imporErrors = new List<File.ImportError>();

                var fileContent = CompareUsers(relationalItems.Users, usersFile, imporErrors);

                SetCollectionsToUpdate(relationalItems, fileContent);

                //setando os responsáveis
                SetLineManagers(usersFile, fileContent, imporErrors);

                var updateBase = await UpdateCollections(relationalItems, fileContent, cancellationToken, imporErrors);

                var file = File.Create(
                    usersFile.Count,
                    new File.ImportedUsers { Users = fileContent.UsersToAdd.Select(x => new File.ImportUser(x.Name, x.Rank?.Name, x.LineManager, "Novo Usuário")).ToList(), Quantity = fileContent.UsersToAdd.Count },
                    new File.ImportedUsers { Users = fileContent.UsersToUpdate.Select(x => new File.ImportUser(x.Name, x.Rank?.Name, x.LineManager, "Usuário Atualizado")).ToList(), Quantity = fileContent.UsersToUpdate.Count },
                    new File.ImportedUsers { Users = fileContent.UsersToBlock.Select(x => new File.ImportUser(x.Name, x.Rank?.Name, x.LineManager, "Usuário Bloqueado")).ToList(), Quantity = fileContent.UsersToBlock.Count },
                    imporErrors
                );

                if (file.IsFailure)
                {
                    return Result.Fail<File>(file.Error);
                }

                await _db.FileCollection.InsertOneAsync(file.Data, cancellationToken: cancellationToken);

                return file;
            }

            private static void SetLineManagers(List<UserItem> usersFile, FileContent fileContent, List<File.ImportError> imporErrors)
            {
                foreach (var user in usersFile)
                {
                    if (!string.IsNullOrEmpty(user.LineManagerEmail))
                    {
                        var responsible = usersFile.FirstOrDefault(x => x.Email == user.LineManagerEmail);
                        if (responsible == null)
                        {
                            imporErrors.Add(new File.ImportError
                            {
                                ImportAction = "AddUser",
                                Name = user.Name,
                                Username = user.UserName,
                                Cge = user.Cge,
                                ImportErrorString = $"LineManager {user.LineManagerEmail} não encontrado."
                            });
                            if (fileContent.UsersToAdd.Contains(user)) fileContent.UsersToAdd.Remove(user);
                            if (fileContent.UsersToUpdate.Contains(user)) fileContent.UsersToUpdate.Remove(user);
                        }
                        else
                        {
                            user.ResponsibleId = responsible.Id;
                        }
                    }
                }
            }

            private List<string> ReadAsList(string content)
            {
                return string.IsNullOrEmpty(content)
                    ? null
                    : content.Split('\n').ToList();
            }

            private UserItem ParseToUser(string[] userFile)
            {
                var userItem = new UserItem();
                userItem.Name = userFile[0];
                userItem.UserName = userFile[1];
                userItem.Password = userFile[2];
                userItem.Email = userFile[3];
                userItem.LineManager = userFile[4];
                userItem.LineManagerEmail = userFile[5];
                var dateSplit = userFile[6].Split('/');
                userItem.ServiceDate = new DateTimeOffset(int.Parse(dateSplit[2]), int.Parse(dateSplit[1]), int.Parse(dateSplit[0]), 0, 0, 0, TimeSpan.Zero);
                userItem.Education = userFile[7];
                userItem.Gender = userFile[8];
                userItem.Manager = userFile[9] == "Yes" ? true : false;
                userItem.Cge = long.Parse(userFile[10]);
                userItem.IdCr = long.Parse(userFile[11]);
                userItem.CoHead = userFile[12];
                userItem.Company = new RelationalItem() { Id = ObjectId.Empty, Name = userFile[13] };
                userItem.BusinessGroup = new RelationalItem() { Id = ObjectId.Empty, Name = userFile[14] };
                userItem.BusinessUnit = new RelationalItem() { Id = ObjectId.Empty, Name = userFile[15] };
                userItem.Country = new RelationalItem() { Id = ObjectId.Empty, Name = userFile[16] };
                userItem.Language = userFile[17];
                userItem.FrontBackOffice = new RelationalItem() { Id = ObjectId.Empty, Name = userFile[18] };
                userItem.Job = new RelationalItem() { Id = ObjectId.Empty, Name = userFile[19] };
                userItem.Location = new LocationRelationalItem() { Id = ObjectId.Empty, Name = userFile[20], CountryId = ObjectId.Empty };
                userItem.Rank = new RelationalItem() { Id = ObjectId.Empty, Name = userFile[21] };
                userItem.Sectors = new List<RelationalItem>();
                if (!string.IsNullOrEmpty(userFile[22])) userItem.Sectors.Add(new RelationalItem() { Id = ObjectId.Empty, Name = userFile[22] });
                if (!string.IsNullOrEmpty(userFile[23])) userItem.Sectors.Add(new RelationalItem() { Id = ObjectId.Empty, Name = userFile[23] });
                if (!string.IsNullOrEmpty(userFile[24])) userItem.Sectors.Add(new RelationalItem() { Id = ObjectId.Empty, Name = userFile[24] });
                if (!string.IsNullOrEmpty(userFile[25])) userItem.Sectors.Add(new RelationalItem() { Id = ObjectId.Empty, Name = userFile[25] });
                userItem.Segment = new RelationalItem() { Id = ObjectId.Empty, Name = userFile[26].Replace("\r", "") };
                return userItem;
            }

            private FileContent CompareUsers(List<UserItem> usersBase, List<UserItem> usersFile,
                List<File.ImportError> imporErrors)
            {
                FileContent fileContent = new FileContent();

                usersFile.ForEach(uf =>
                {
                    if (uf.Cge != null && !string.IsNullOrEmpty(uf.UserName) && !string.IsNullOrEmpty(uf.Email))
                    {
                        var content = usersBase.FirstOrDefault(x => x.Cge == uf.Cge);
                        if (content == null)
                        {
                            uf.Password = string.IsNullOrEmpty(uf.Password) || uf.Password.Length < 6 ? $"Academia_{DateTime.Now.Ticks}" : uf.Password;
                            uf.Id = ObjectId.GenerateNewId();
                            fileContent.UsersToAdd.Add(uf);
                        }
                        else
                        {
                            uf.Id = content.Id;
                            fileContent.UsersToUpdate.Add(uf);
                        }
                    }
                    else
                    {
                        imporErrors.Add(new File.ImportError
                        {
                            ImportAction = "AddUser",
                            Name = uf.Name,
                            Username = uf.UserName,
                            Cge = uf.Cge,
                            ImportErrorString = "Sem CGE, email ou username"
                        });
                    }
                });

                usersBase.ForEach(ub =>
                {
                    if (ub.Cge != null && !ub.IsBlocked)
                    {
                        var content = usersFile.FirstOrDefault(x => x.Cge == ub.Cge);
                        if (content == null)
                        {
                            fileContent.UsersToBlock.Add(ub);
                        }
                    }
                    else
                    {
                        fileContent.UsersToNothing.Add(ub);
                    }
                });
                return fileContent;
            }

            private async Task<MatrixRelationalItems> GetCollections(CancellationToken cancellationToken)
            {
                var jobsCollection = _db.Database.GetCollection<RelationalItem>("Jobs");
                var jobsQry = await jobsCollection.FindAsync(FilterDefinition<RelationalItem>.Empty,
                    new FindOptions<RelationalItem>(),
                    cancellationToken: cancellationToken
                );
                var jobsList = await jobsQry.ToListAsync(cancellationToken);
                jobsList.ForEach(element => { element.Name = element.Name.ToUpper().Trim(); });

                var ranksCollection = _db.Database.GetCollection<RelationalItem>("Ranks");
                var ranksQry = await ranksCollection.FindAsync(FilterDefinition<RelationalItem>.Empty,
                    new FindOptions<RelationalItem>(),
                    cancellationToken: cancellationToken
                );
                var ranksList = await ranksQry.ToListAsync(cancellationToken);
                ranksList.ForEach(element => { element.Name = element.Name.ToUpper().Trim(); });

                var segmentsCollection = _db.Database.GetCollection<RelationalItem>("Segments");
                var segmentsQry = await segmentsCollection.FindAsync(FilterDefinition<RelationalItem>.Empty,
                    new FindOptions<RelationalItem>(),
                    cancellationToken: cancellationToken
                );
                var segmentsList = await segmentsQry.ToListAsync(cancellationToken);
                segmentsList.ForEach(element => { element.Name = element.Name.ToUpper().Trim(); });

                var sectorsCollection = _db.Database.GetCollection<RelationalItem>("Sectors");
                var sectorsQry = await sectorsCollection.FindAsync(FilterDefinition<RelationalItem>.Empty,
                    new FindOptions<RelationalItem>(),
                    cancellationToken: cancellationToken
                );
                var sectorsList = await sectorsQry.ToListAsync(cancellationToken);
                sectorsList.ForEach(element => { element.Name = element.Name.ToUpper().Trim(); });

                var companiesCollection = _db.Database.GetCollection<RelationalItem>("Companies");
                var companiesQry = await companiesCollection.FindAsync(FilterDefinition<RelationalItem>.Empty,
                    new FindOptions<RelationalItem>(),
                    cancellationToken: cancellationToken
                );
                var companiesList = await companiesQry.ToListAsync(cancellationToken);
                companiesList.ForEach(element => { element.Name = element.Name.ToUpper().Trim(); });

                var businessGroupsCollection = _db.Database.GetCollection<RelationalItem>("BusinessGroups");
                var businessGroupsQry = await businessGroupsCollection.FindAsync(FilterDefinition<RelationalItem>.Empty,
                    new FindOptions<RelationalItem>(),
                    cancellationToken: cancellationToken
                );
                var businessGroupsList = await businessGroupsQry.ToListAsync(cancellationToken);
                businessGroupsList.ForEach(element => { element.Name = element.Name.ToUpper().Trim(); });

                var businessUnitsCollection = _db.Database.GetCollection<RelationalItem>("BusinessUnits");
                var businessUnitsQry = await businessUnitsCollection.FindAsync(FilterDefinition<RelationalItem>.Empty,
                    new FindOptions<RelationalItem>(),
                    cancellationToken: cancellationToken
                );
                var businessUnitsList = await businessUnitsQry.ToListAsync(cancellationToken);
                businessUnitsList.ForEach(element => { element.Name = element.Name.ToUpper().Trim(); });

                var frontBackOfficesCollection = _db.Database.GetCollection<RelationalItem>("FrontBackOffices");
                var frontBackOfficesQry = await frontBackOfficesCollection.FindAsync(FilterDefinition<RelationalItem>.Empty,
                    new FindOptions<RelationalItem>(),
                    cancellationToken: cancellationToken
                );
                var frontBackOfficesList = await frontBackOfficesQry.ToListAsync(cancellationToken);
                frontBackOfficesList.ForEach(element => { element.Name = element.Name.ToUpper().Trim(); });

                var countriesCollection = _db.Database.GetCollection<RelationalItem>("Countries");
                var countriesQry = await countriesCollection.FindAsync(FilterDefinition<RelationalItem>.Empty,
                    new FindOptions<RelationalItem>(),
                    cancellationToken: cancellationToken
                );
                var countriesList = await countriesQry.ToListAsync(cancellationToken);
                countriesList.ForEach(element => { element.Name = element.Name.ToUpper().Trim(); });

                var locationsCollection = _db.Database.GetCollection<LocationRelationalItem>("Locations");
                var locationsQry = await locationsCollection.FindAsync(FilterDefinition<LocationRelationalItem>.Empty,
                    new FindOptions<LocationRelationalItem>(),
                    cancellationToken: cancellationToken
                );
                var locationsList = await locationsQry.ToListAsync(cancellationToken);
                locationsList.ForEach(element => { element.Name = element.Name.ToUpper().Trim(); });

                var usersCollection = _db.UserCollection.AsQueryable();
                var usersList = await usersCollection
                    .Where(x => x.Cge.HasValue) //busca só quem tem matricula interna do BTG
                    .Select(x => new UserItem()
                    {
                        Cge = x.Cge,
                        Id = x.Id,
                        Email = x.Email,
                        IsBlocked = x.IsBlocked,
                        Rank = x.Rank != null ? new RelationalItem() { Id = x.Rank.Id, Name = x.Rank.Name } : null,
                        LineManager = x.LineManager,
                        Name = x.Name,
                        UserName = x.UserName
                    }).ToListAsync(
                        cancellationToken: cancellationToken
                    );

                return new MatrixRelationalItems()
                {
                    Jobs = new ListRelationalItems() { BaseItems = jobsList, TotalItems = new List<RelationalItem>(jobsList) },
                    Ranks = new ListRelationalItems() { BaseItems = ranksList, TotalItems = new List<RelationalItem>(ranksList) },
                    Segments = new ListRelationalItems() { BaseItems = segmentsList, TotalItems = new List<RelationalItem>(segmentsList) },
                    Sectors = new ListRelationalItems() { BaseItems = sectorsList, TotalItems = new List<RelationalItem>(sectorsList) },
                    Companies = new ListRelationalItems() { BaseItems = companiesList, TotalItems = new List<RelationalItem>(companiesList) },
                    BusinessGroups = new ListRelationalItems() { BaseItems = businessGroupsList, TotalItems = new List<RelationalItem>(businessGroupsList) },
                    BusinessUnits = new ListRelationalItems() { BaseItems = businessUnitsList, TotalItems = new List<RelationalItem>(businessUnitsList) },
                    FrontBackOffices = new ListRelationalItems() { BaseItems = frontBackOfficesList, TotalItems = new List<RelationalItem>(frontBackOfficesList) },
                    Countries = new ListRelationalItems() { BaseItems = countriesList, TotalItems = new List<RelationalItem>(countriesList) },
                    Locations = new ListLocationRelationalItems() { BaseItems = locationsList, TotalItems = new List<LocationRelationalItem>(locationsList) },
                    Users = usersList
                };
            }

            private void SetCollectionsToUpdate(MatrixRelationalItems matrix, FileContent file)
            {
                file.UsersToAdd.ForEach(addUser =>
                {
                    var userJob = addUser.Job.Name.ToUpper().Trim();
                    if (!string.IsNullOrEmpty(userJob))
                    {
                        var userJobItem = matrix.Jobs.TotalItems.FirstOrDefault(x => x.Name.ToUpper().Trim() == userJob);
                        if (userJobItem == null)
                        {
                            var newJob = new RelationalItem() { Id = ObjectId.GenerateNewId(), Name = addUser.Job.Name };
                            matrix.Jobs.NewItems.Add(newJob);
                            matrix.Jobs.TotalItems.Add(newJob);
                            addUser.Job.Id = newJob.Id;
                        }
                        else
                        {
                            addUser.Job.Id = userJobItem.Id;
                        }
                    }

                    var userRank = addUser.Rank.Name.ToUpper().Trim();
                    if (!string.IsNullOrEmpty(userRank))
                    {
                        var userRankItem = matrix.Ranks.TotalItems.FirstOrDefault(x => x.Name.ToUpper().Trim() == userRank);
                        if (userRankItem == null)
                        {
                            var newRank = new RelationalItem() { Id = ObjectId.GenerateNewId(), Name = addUser.Rank.Name };
                            matrix.Ranks.NewItems.Add(newRank);
                            matrix.Ranks.TotalItems.Add(newRank);
                            addUser.Rank.Id = newRank.Id;
                        }
                        else
                        {
                            addUser.Rank.Id = userRankItem.Id;
                        }
                    }

                    var userSegment = addUser.Segment.Name.ToUpper().Trim();
                    if (!string.IsNullOrEmpty(userSegment))
                    {
                        var userSegmentItem = matrix.Segments.TotalItems.FirstOrDefault(x => x.Name.ToUpper().Trim() == userSegment);
                        if (userSegmentItem == null)
                        {
                            var newSegment = new RelationalItem() { Id = ObjectId.GenerateNewId(), Name = addUser.Segment.Name };
                            matrix.Segments.NewItems.Add(newSegment);
                            matrix.Segments.TotalItems.Add(newSegment);
                            addUser.Segment.Id = newSegment.Id;
                        }
                        else
                        {
                            addUser.Segment.Id = userSegmentItem.Id;
                        }
                    }

                    if (addUser.Sectors.Count > 0)
                    {
                        addUser.Sectors.ForEach(sector =>
                        {
                            var userSector = sector.Name.ToUpper().Trim();
                            if (!string.IsNullOrEmpty(userSector))
                            {
                                var userSectorItem = matrix.Sectors.TotalItems.FirstOrDefault(x => x.Name.ToUpper().Trim() == userSector);
                                if (userSectorItem == null)
                                {
                                    var newSector = new RelationalItem() { Id = ObjectId.GenerateNewId(), Name = sector.Name };
                                    matrix.Sectors.NewItems.Add(newSector);
                                    matrix.Sectors.TotalItems.Add(newSector);
                                    sector.Id = newSector.Id;
                                }
                                else
                                {
                                    sector.Id = userSectorItem.Id;
                                }
                            }
                        });
                    }

                    var userCompany = addUser.Company.Name.ToUpper().Trim();
                    if (!string.IsNullOrEmpty(userCompany))
                    {
                        var userCompanyItem = matrix.Companies.TotalItems.FirstOrDefault(x => x.Name.ToUpper().Trim() == userCompany);
                        if (userCompanyItem == null)
                        {
                            var newCompany = new RelationalItem() { Id = ObjectId.GenerateNewId(), Name = addUser.Company.Name };
                            matrix.Companies.NewItems.Add(newCompany);
                            matrix.Companies.TotalItems.Add(newCompany);
                            addUser.Company.Id = newCompany.Id;
                        }
                        else
                        {
                            addUser.Company.Id = userCompanyItem.Id;
                        }
                    }

                    var userBusinessGroup = addUser.BusinessGroup.Name.ToUpper().Trim();
                    if (!string.IsNullOrEmpty(userBusinessGroup))
                    {
                        var userBusinessGroupItem = matrix.BusinessGroups.TotalItems.FirstOrDefault(x => x.Name.ToUpper().Trim() == userBusinessGroup);
                        if (userBusinessGroupItem == null)
                        {
                            var newBusinessGroup = new RelationalItem() { Id = ObjectId.GenerateNewId(), Name = addUser.BusinessGroup.Name };
                            matrix.BusinessGroups.NewItems.Add(newBusinessGroup);
                            matrix.BusinessGroups.TotalItems.Add(newBusinessGroup);
                            addUser.BusinessGroup.Id = newBusinessGroup.Id;
                        }
                        else
                        {
                            addUser.BusinessGroup.Id = userBusinessGroupItem.Id;
                        }
                    }

                    var userBusinessUnit = addUser.BusinessUnit.Name.ToUpper().Trim();
                    if (!string.IsNullOrEmpty(userBusinessUnit))
                    {
                        var userBusinessUnitItem = matrix.BusinessUnits.TotalItems.FirstOrDefault(x => x.Name.ToUpper().Trim() == userBusinessUnit);
                        if (userBusinessUnitItem == null)
                        {
                            var newBusinessUnit = new RelationalItem() { Id = ObjectId.GenerateNewId(), Name = addUser.BusinessUnit.Name };
                            matrix.BusinessUnits.NewItems.Add(newBusinessUnit);
                            matrix.BusinessUnits.TotalItems.Add(newBusinessUnit);
                            addUser.BusinessUnit.Id = newBusinessUnit.Id;
                        }
                        else
                        {
                            addUser.BusinessUnit.Id = userBusinessUnitItem.Id;
                        }
                    }

                    var userFrontBackOffice = addUser.FrontBackOffice.Name.ToUpper().Trim();
                    if (!string.IsNullOrEmpty(userFrontBackOffice))
                    {
                        var userFrontBackOfficeItem = matrix.FrontBackOffices.TotalItems.FirstOrDefault(x => x.Name.ToUpper().Trim() == userFrontBackOffice);
                        if (userFrontBackOfficeItem == null)
                        {
                            var newFrontBackOffice = new RelationalItem() { Id = ObjectId.GenerateNewId(), Name = addUser.FrontBackOffice.Name };
                            matrix.FrontBackOffices.NewItems.Add(newFrontBackOffice);
                            matrix.FrontBackOffices.TotalItems.Add(newFrontBackOffice);
                            addUser.FrontBackOffice.Id = newFrontBackOffice.Id;
                        }
                        else
                        {
                            addUser.FrontBackOffice.Id = userFrontBackOfficeItem.Id;
                        }
                    }

                    var userCountry = addUser.Country.Name.ToUpper().Trim();
                    if (!string.IsNullOrEmpty(userCountry))
                    {
                        var userCountryItem = matrix.Countries.TotalItems.FirstOrDefault(x => x.Name.ToUpper().Trim() == userCountry);
                        if (userCountryItem == null)
                        {
                            var newCountry = new RelationalItem() { Id = ObjectId.GenerateNewId(), Name = addUser.Country.Name };
                            matrix.Countries.NewItems.Add(newCountry);
                            matrix.Countries.TotalItems.Add(newCountry);
                            addUser.Country.Id = newCountry.Id;
                        }
                        else
                        {
                            addUser.Country.Id = userCountryItem.Id;
                        }
                    }

                    var userLocation = addUser.Location.Name.ToUpper().Trim();
                    if (!string.IsNullOrEmpty(userLocation))
                    {
                        var userLocationItem = matrix.Locations.TotalItems.FirstOrDefault(x => x.Name.ToUpper().Trim() == userLocation);
                        if (userLocationItem == null)
                        {
                            var newLocation = new LocationRelationalItem() { Id = ObjectId.GenerateNewId(), Name = addUser.Location.Name, CountryId = addUser.Country.Id };
                            matrix.Locations.NewItems.Add(newLocation);
                            matrix.Locations.TotalItems.Add(newLocation);
                            addUser.Location.Id = newLocation.Id;
                        }
                        else
                        {
                            addUser.Location.Id = userLocationItem.Id;
                            addUser.Location.CountryId = addUser.Country.Id;
                        }
                    }
                });
            }

            private async Task<Result<bool>> UpdateCollections(MatrixRelationalItems matrix, FileContent file,
                CancellationToken cancellationToken, List<File.ImportError> imporErrors)
            {
                try
                {
                    if (matrix.Jobs.NewItems.Count > 0)
                        await _db.JobCollection.InsertManyAsync(matrix.Jobs.NewItems.Select(x => new Job(x.Id, x.Name)), cancellationToken: cancellationToken);

                    if (matrix.Ranks.NewItems.Count > 0)
                        await _db.RankCollection.InsertManyAsync(matrix.Ranks.NewItems.Select(x => new Rank(x.Id, x.Name)), cancellationToken: cancellationToken);

                    if (matrix.Segments.NewItems.Count > 0)
                        await _db.SegmentCollection.InsertManyAsync(matrix.Segments.NewItems.Select(x => new Segment(x.Id, x.Name)), cancellationToken: cancellationToken);

                    if (matrix.Sectors.NewItems.Count > 0)
                        await _db.SectorCollection.InsertManyAsync(matrix.Sectors.NewItems.Select(x => new Sector(x.Id, x.Name)), cancellationToken: cancellationToken);

                    if (matrix.Companies.NewItems.Count > 0)
                        await _db.CompanyCollection.InsertManyAsync(matrix.Companies.NewItems.Select(x => new Company(x.Id, x.Name)), cancellationToken: cancellationToken);

                    if (matrix.BusinessGroups.NewItems.Count > 0)
                        await _db.BusinessGroupCollection.InsertManyAsync(matrix.BusinessGroups.NewItems.Select(x => new BusinessGroup(x.Id, x.Name)), cancellationToken: cancellationToken);

                    if (matrix.BusinessUnits.NewItems.Count > 0)
                        await _db.BusinessUnitCollection.InsertManyAsync(matrix.BusinessUnits.NewItems.Select(x => new BusinessUnit(x.Id, x.Name)), cancellationToken: cancellationToken);

                    if (matrix.FrontBackOffices.NewItems.Count > 0)
                        await _db.FrontBackOfficeCollection.InsertManyAsync(matrix.FrontBackOffices.NewItems.Select(x => new FrontBackOffice(x.Id, x.Name)), cancellationToken: cancellationToken);

                    if (matrix.Countries.NewItems.Count > 0)
                        await _db.CountryCollection.InsertManyAsync(matrix.Countries.NewItems.Select(x => new Country(x.Id, x.Name)), cancellationToken: cancellationToken);

                    if (matrix.Locations.NewItems.Count > 0)
                        await _db.LocationCollection.InsertManyAsync(matrix.Locations.NewItems.Select(x => new Location(x.Id, x.Name, x.CountryId)), cancellationToken: cancellationToken);

                    var idx = 0;
                    Console.WriteLine($"{file.UsersToAdd.Count} usuários a serem inseridos;");
                    foreach (UserItem userAdd in file.UsersToAdd.Reverse<UserItem>())
                    {
                        var newUser = User.CreateUserFromFile(
                            userAdd.Id,
                            userAdd.Name,
                            userAdd.UserName,
                            userAdd.Email,
                            userAdd.LineManager,
                            userAdd.LineManagerEmail,
                            userAdd.ServiceDate,
                            userAdd.Education,
                            userAdd.Gender,
                            userAdd.Manager,
                            userAdd.Cge,
                            userAdd.IdCr,
                            userAdd.CoHead,
                            new User.RelationalItem(userAdd.Company.Id, userAdd.Company.Name),
                            new User.RelationalItem(userAdd.BusinessGroup.Id, userAdd.BusinessGroup.Name),
                            new User.RelationalItem(userAdd.BusinessUnit.Id, userAdd.BusinessUnit.Name),
                            new User.RelationalItem(userAdd.Country.Id, userAdd.Country.Name),
                            userAdd.Language,
                            new User.RelationalItem(userAdd.FrontBackOffice.Id, userAdd.FrontBackOffice.Name),
                            new User.RelationalItem(userAdd.Job.Id, userAdd.Job.Name),
                            new User.RelationalItem(userAdd.Location.Id, userAdd.Location.Name),
                            new User.RelationalItem(userAdd.Rank.Id, userAdd.Rank.Name),
                            userAdd.Sectors.Select(x => new User.RelationalItem(x.Id, x.Name)).ToList(),
                            new User.RelationalItem(userAdd.Segment.Id, userAdd.Segment.Name)
                        );

                        if (newUser.IsFailure)
                        {
                            imporErrors.Add(new File.ImportError
                            {
                                ImportAction = "AddUser",
                                Name = userAdd.Name,
                                Username = userAdd.UserName,
                                Cge = userAdd.Cge,
                                ImportErrorString = newUser.Error
                            });
                            file.UsersToAdd.Remove(userAdd);
                            continue;
                        }
                        var result = await _userManager.CreateAsync(newUser.Data, userAdd.Password);

                        //console status
                        idx++;
                        if (idx % 100 == 0 || idx == file.UsersToAdd.Count) Console.WriteLine($"{idx}/{file.UsersToAdd.Count} usuários inseridos;");

                        if (!result.Succeeded)
                        {
                            var errors = result.Errors.Select(error => new File.ImportError
                            {
                                ImportAction = "AddUser",
                                Name = userAdd.Name,
                                Username = userAdd.UserName,
                                Cge = userAdd.Cge,
                                ImportErrorString = error != null ? error.Description : "Erro interno ao criar usuário"
                            }).ToList();
                            imporErrors.AddRange(errors);
                            file.UsersToAdd.Remove(userAdd);
                            continue;
                        }
                    }
                    Console.WriteLine("Importação de novos usuários terminada.");

                    idx = 0;
                    Console.WriteLine($"{file.UsersToUpdate.Count} usuários a serem atualizados;");
                    var models = new List<WriteModel<User>>();
                    foreach (UserItem userUpd in file.UsersToUpdate.Reverse<UserItem>())
                    {
                        List<UpdateDefinition<User>> updates = new List<UpdateDefinition<User>>();


                        updates.Add(Builders<User>.Update.Set(usr => usr.Name, userUpd.Name));
                        updates.Add(Builders<User>.Update.Set(usr => usr.UserName, userUpd.UserName));
                        updates.Add(Builders<User>.Update.Set(usr => usr.NormalizedUsername, userUpd.UserName.ToUpper()));
                        updates.Add(Builders<User>.Update.Set(usr => usr.Email, userUpd.Email));
                        updates.Add(Builders<User>.Update.Set(usr => usr.NormalizedEmail, userUpd.Email.ToUpper()));
                        updates.Add(Builders<User>.Update.Set(usr => usr.LineManager, userUpd.LineManager));
                        updates.Add(Builders<User>.Update.Set(usr => usr.LineManagerEmail, userUpd.LineManagerEmail));
                        updates.Add(Builders<User>.Update.Set(usr => usr.ResponsibleId, userUpd.ResponsibleId));
                        updates.Add(Builders<User>.Update.Set(usr => usr.ServiceDate, userUpd.ServiceDate));
                        updates.Add(Builders<User>.Update.Set(usr => usr.Education, userUpd.Education));
                        updates.Add(Builders<User>.Update.Set(usr => usr.Gender, userUpd.Gender));
                        updates.Add(Builders<User>.Update.Set(usr => usr.Manager, userUpd.Manager));
                        updates.Add(Builders<User>.Update.Set(usr => usr.Cge, userUpd.Cge));
                        updates.Add(Builders<User>.Update.Set(usr => usr.IdCr, userUpd.IdCr));
                        updates.Add(Builders<User>.Update.Set(usr => usr.CoHead, userUpd.CoHead));
                        updates.Add(Builders<User>.Update.Set(usr => usr.Company, new User.RelationalItem(userUpd.Company.Id, userUpd.Company.Name)));
                        updates.Add(Builders<User>.Update.Set(usr => usr.BusinessGroup, new User.RelationalItem(userUpd.BusinessGroup.Id, userUpd.BusinessGroup.Name)));
                        updates.Add(Builders<User>.Update.Set(usr => usr.BusinessUnit, new User.RelationalItem(userUpd.BusinessUnit.Id, userUpd.BusinessUnit.Name)));
                        updates.Add(Builders<User>.Update.Set(usr => usr.Country, new User.RelationalItem(userUpd.Country.Id, userUpd.Country.Name)));
                        updates.Add(Builders<User>.Update.Set(usr => usr.Language, userUpd.Language));
                        updates.Add(Builders<User>.Update.Set(usr => usr.FrontBackOffice, new User.RelationalItem(userUpd.FrontBackOffice.Id, userUpd.FrontBackOffice.Name)));
                        updates.Add(Builders<User>.Update.Set(usr => usr.Job, new User.RelationalItem(userUpd.Job.Id, userUpd.Job.Name)));
                        updates.Add(Builders<User>.Update.Set(usr => usr.Location, new User.RelationalItem(userUpd.Location.Id, userUpd.Location.Name)));
                        updates.Add(Builders<User>.Update.Set(usr => usr.Rank, new User.RelationalItem(userUpd.Rank.Id, userUpd.Rank.Name)));
                        updates.Add(Builders<User>.Update.Set(usr => usr.Sectors, userUpd.Sectors.Select(x => new User.RelationalItem(x.Id, x.Name)).ToList()));
                        updates.Add(Builders<User>.Update.Set(usr => usr.Segment, new User.RelationalItem(userUpd.Segment.Id, userUpd.Segment.Name)));
                        updates.Add(Builders<User>.Update.Set(usr => usr.IsBlocked, false));
                        var combined = Builders<User>.Update.Combine(updates);

                        models.Add(new UpdateOneModel<User>(new BsonDocument("_id", userUpd.Id), combined));

                        //await _db.UserCollection.UpdateOneAsync(x => x.Id == userUpd.Id, combined, cancellationToken: cancellationToken);

                        //console status
                        idx++;
                        if (idx % 100 == 0 || idx == file.UsersToUpdate.Count) Console.WriteLine($"{idx}/{file.UsersToUpdate.Count} usuários preparados;");
                    }
                    Console.WriteLine("Iniciando atualizações de usuários.");
                    await _db.UserCollection.BulkWriteAsync(models);
                    Console.WriteLine("Terminei atualizações de usuários.");

                    idx = 0;
                    Console.WriteLine($"{file.UsersToBlock.Count} usuários a serem bloqueados;");
                    if (file.UsersToBlock.Any())
                    {
                        var update = Builders<User>.Update.Set(usr => usr.IsBlocked, true);
                        var ids = file.UsersToBlock.Select(x => x.Id).ToArray();
                        await _db.UserCollection.UpdateManyAsync(x => ids.Contains(x.Id), update, cancellationToken: cancellationToken);
                    }
                    // foreach (var userBlock in file.UsersToBlock)
                    // {
                    //     if (userBlock == null)
                    //         continue;

                    //     var update = Builders<User>.Update.Set(usr => usr.IsBlocked, true);
                    //     await _db.UserCollection.UpdateOneAsync(x => x.Id == userBlock.Id, update, cancellationToken: cancellationToken);

                    //     //console status
                    //     idx++;
                    //     if (idx % 100 == 0 || idx == file.UsersToBlock.Count) Console.WriteLine($"{idx}/{file.UsersToBlock.Count} usuários bloqueados;");
                    // }
                    Console.WriteLine("Terminei bloqueio de usuários.");

                    return Result.Ok(true);
                }
                catch (Exception ex)
                {
                    imporErrors.Add(new File.ImportError
                    {
                        ImportAction = "ImportMethod",
                        ImportErrorString = ex.Message
                    });
                    Console.WriteLine("Erro na sincronia de usuários.");
                    Console.WriteLine(ex.Message);
                    return Result.Ok(false);
                }

            }
        }
    }
}
