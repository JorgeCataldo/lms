using Domain.Aggregates.Events;
using Domain.Aggregates.Jobs;
using Domain.Aggregates.Modules;
using Domain.Aggregates.Questions;
using Domain.Aggregates.Ranks;
using Domain.Aggregates.Sectors;
using Domain.Aggregates.Segments;
using Domain.Aggregates.Tracks;
using Domain.Aggregates.UserProgressHistory;
using Domain.Aggregates.Users;
using Domain.Aggregates.Files;
using Domain.Aggregates.Companies;
using Domain.Aggregates.BusinessGroups;
using Domain.Aggregates.BusinessUnits;
using Domain.Aggregates.FrontBackOffices;
using Domain.Data;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Domain.Aggregates.Forums;
using Domain.Aggregates.Notifications;
using Domain.Aggregates.Countries;
using Domain.Aggregates.Locations;
using Domain.Aggregates.Messages;
using Domain.Aggregates.UserFiles;
using Domain.Aggregates.UserContentNotes;
using Domain.Aggregates.ProfileTests;
using Domain.Aggregates.VerificationEmail;
using Domain.Aggregates;
using Domain.Aggregates.Institutions;
using Domain.Aggregates.EventForums;
using Domain.Aggregates.ModulesDrafts;
using Domain.Aggregates.Responsibles;
using Domain.Aggregates.UsersCareer;
using Domain.Aggregates.PurchaseHistory;
using Domain.Aggregates.RecruitingCompany;
using Domain.Aggregates.RecruitmentFavorite;
using Performance.Domain.Aggregates.AuditLogs;
using Domain.Aggregates.JobPosition;
using Domain.Aggregates.Candidate;
using Domain.Aggregates.ColorPalettes;
using Domain.Aggregates.ValuationTests;
using Domain.Aggregates.NetPromoterScores;
using Domain.Aggregates.Activations;

namespace Infrastructure.Data
{
    public class DbContext : IDbContext
    {
        public const string EventCollectionName = "Events";
        public const string EventApplicationCollectionName = "EventApplications";
        public const string UserCollectionName = "Users";
        public const string UserCareerCollectionName = "UsersCareer";
        public const string ModuleCollectionName = "Modules";
        public const string QuestionCollectionName = "Questions";
        public const string TrackCollectionName = "Tracks";
        public const string SegmentCollectionName = "Segments";
        public const string SectorCollectionName = "Sectors";
        public const string RankCollectionName = "Ranks";
        public const string JobCollectionName = "Jobs";
        public const string UserSubjectProgressCollectionName = "UserSubjectProgress";
        public const string UserModuleProgressCollectionName = "UserModuleProgress";
        public const string UserTrackProgressCollectionName = "UserTrackProgress";
        public const string FileCollectionName = "Files";
        public const string CompanyCollectionName = "Companies";
        public const string BusinessGroupCollectionName = "BusinessGroups";
        public const string BusinessUnitCollectionName = "BusinessUnits";
        public const string FrontBackOfficeCollectionName = "FrontBackOffices";
        public const string ActionCollectionName = "Actions";
        public const string ForumCollectionName = "Forums";
        public const string ForumQuestionCollectionName = "ForumQuestions";
        public const string ForumAnswerCollectionName = "ForumAnswers";
        public const string EventReactionCollectionName = "EventReactions";
        public const string NotificationCollectionName = "Notifications";
        public const string CountryCollectionName = "Countries";
        public const string LocationCollectionName = "Locations";
        public const string MessageCollectionName = "Messages";
        public const string ContactAreaCollectionName = "ContactAreas";
        public const string CustomEmailsCollectionName = "CustomEmails";
        public const string UserFilesCollectionName = "UserFiles";
        public const string UserContentNotesCollectionName = "UserContentNotes";
        public const string ProfileTestsCollectionName = "ProfileTests";
        public const string ProfileTestQuestionsCollectionName = "ProfileTestQuestions";
        public const string ProfileTestResponsesCollectionName = "ProfileTestResponses";
        public const string SuggestedProductsCollectionName = "SuggestedProducts";
        public const string UserVerificationEmailsCollectionName = "UserVerificationEmails";
        public const string ErrorLogsCollectionName = "ErrorLogs";
        public const string InstitutionsCollectionName = "Institutions";
        public const string EventForumCollectionName = "EventForums";
        public const string EventForumQuestionCollectionName = "EventForumQuestions";
        public const string EventForumAnswerCollectionName = "EventForumAnswers";
        public const string ModulesDraftsCollectionName = "ModulesDrafts";
        public const string EventsDraftsCollectionName = "EventsDrafts";
        public const string QuestionsDraftsCollectionName = "QuestionsDrafts";
        public const string ResponsiblesCollectionName = "Responsibles";
        public const string PurchaseHistoryCollectionName = "PurchaseHistory";
        public const string ModuleGradeCollectionName = "ModuleGrade";
        public const string FormulasCollectionName = "Formulas";
        public const string FormulaTypeVariablesCollectionName = "FormulaTypeVariables";
        public const string RecruitingCompanyCollectionName = "RecruitingCompany";
        public const string RecruitmentFavoriteCollectionName = "RecruitmentFavorite";
        public const string AuditLogCollectionName = "AuditLogs";
        public const string JobPositionCollectionName = "JobPosition";
        public const string CandidateCollectionName = "Candidate";
        public const string ColorPaletteCollectionName = "ColorPalettes";
        public const string ValuationTestsCollectionName = "ValuationTests";
        public const string ValuationTestQuestionsCollectionName = "ValuationTestQuestions";
        public const string ValuationTestResponsesCollectionName = "ValuationTestResponses";
        public const string NetPromoterScoresCollectionName = "NetPromoterScores";
        public const string ActivationsCollectionName = "Activations";

        public DbContext(IOptions<MongoSettings> settings)
        {            
            var client = new MongoClient(settings.Value.ConnectionString);
            
            Database = client.GetDatabase(settings.Value.Database);            
            EventCollection = Database.GetCollection<Event>(EventCollectionName);
            EventApplicationCollection = Database.GetCollection<EventApplication>(EventApplicationCollectionName);
            UserCollection = Database.GetCollection<User>(UserCollectionName);
            ModuleCollection = Database.GetCollection<Module>(ModuleCollectionName);
            QuestionCollection = Database.GetCollection<Question>(QuestionCollectionName);
            TrackCollection = Database.GetCollection<Track>(TrackCollectionName);
            SegmentCollection = Database.GetCollection<Segment>(SegmentCollectionName);
            SectorCollection = Database.GetCollection<Sector>(SectorCollectionName);
            RankCollection = Database.GetCollection<Rank>(RankCollectionName);
            JobCollection = Database.GetCollection<Job>(JobCollectionName);
            UserSubjectProgressCollection = Database.GetCollection<UserSubjectProgress>(UserSubjectProgressCollectionName);
            UserModuleProgressCollection = Database.GetCollection<UserModuleProgress>(UserModuleProgressCollectionName);
            UserTrackProgressCollection = Database.GetCollection<UserTrackProgress>(UserTrackProgressCollectionName);
            FileCollection = Database.GetCollection<File>(FileCollectionName);
            CompanyCollection = Database.GetCollection<Company>(CompanyCollectionName);
            BusinessGroupCollection = Database.GetCollection<BusinessGroup>(BusinessGroupCollectionName);
            BusinessUnitCollection = Database.GetCollection<BusinessUnit>(BusinessUnitCollectionName);
            FrontBackOfficeCollection = Database.GetCollection<FrontBackOffice>(FrontBackOfficeCollectionName);
            ActionCollection = Database.GetCollection<Action>(ActionCollectionName);
            ForumCollection = Database.GetCollection<Forum>(ForumCollectionName);
            ForumQuestionCollection = Database.GetCollection<ForumQuestion>(ForumQuestionCollectionName);
            ForumAnswerCollection = Database.GetCollection<ForumAnswer>(ForumAnswerCollectionName);
            EventReactionCollection = Database.GetCollection<EventReaction>(EventReactionCollectionName);
            NotificationCollection = Database.GetCollection<Notification>(NotificationCollectionName);
            CountryCollection = Database.GetCollection<Country>(CountryCollectionName);
            LocationCollection = Database.GetCollection<Location>(LocationCollectionName);
            MessageCollection = Database.GetCollection<Message>(MessageCollectionName);
            ContactAreaCollection = Database.GetCollection<ContactArea>(ContactAreaCollectionName);
            CustomEmailCollection = Database.GetCollection<CustomEmail>(CustomEmailsCollectionName);
            UserFileCollection = Database.GetCollection<UserFile>(UserFilesCollectionName);
            UserContentNoteCollection = Database.GetCollection<UserContentNote>(UserContentNotesCollectionName);
            ProfileTestCollection = Database.GetCollection<ProfileTest>(ProfileTestsCollectionName);
            ProfileTestQuestionCollection = Database.GetCollection<ProfileTestQuestion>(ProfileTestQuestionsCollectionName);
            ProfileTestResponseCollection = Database.GetCollection<ProfileTestResponse>(ProfileTestResponsesCollectionName);
            SuggestedProductCollection = Database.GetCollection<SuggestedProduct>(SuggestedProductsCollectionName);
            UserVerificationEmailCollection = Database.GetCollection<UserVerificationEmail>(UserVerificationEmailsCollectionName);
            ErrorLogCollection = Database.GetCollection<ErrorLog>(ErrorLogsCollectionName);
            InstituteCollection = Database.GetCollection<Institute>(InstitutionsCollectionName);
            EventForumCollection = Database.GetCollection<EventForum>(EventForumCollectionName);
            EventForumQuestionCollection = Database.GetCollection<EventForumQuestion>(EventForumQuestionCollectionName);
            EventForumAnswerCollection = Database.GetCollection<EventForumAnswer>(EventForumAnswerCollectionName);
            ModuleDraftCollection = Database.GetCollection<ModuleDraft>(ModulesDraftsCollectionName);
            EventDraftCollection = Database.GetCollection<EventDraft>(EventsDraftsCollectionName);
            QuestionDraftCollection = Database.GetCollection<QuestionDraft>(QuestionsDraftsCollectionName);
            ResponsibleCollection = Database.GetCollection<Responsible>(ResponsiblesCollectionName);
            UserCareerCollection = Database.GetCollection<UserCareer>(UserCareerCollectionName);
            PurchaseHistoryCollection = Database.GetCollection<PurchaseHistory>(PurchaseHistoryCollectionName);
            ModuleGradeCollection = Database.GetCollection<ModuleGrade>(ModuleGradeCollectionName);
            FormulaCollection = Database.GetCollection<Formula>(FormulasCollectionName);
            FormulaTypeVariablesCollection = Database.GetCollection<FormulaVariables>(FormulaTypeVariablesCollectionName);
            RecruitingCompanyCollection = Database.GetCollection<RecruitingCompany>(RecruitingCompanyCollectionName);
            RecruitmentFavoriteCollection = Database.GetCollection<RecruitmentFavorite>(RecruitmentFavoriteCollectionName);
            AuditLogCollection = Database.GetCollection<AuditLog>(AuditLogCollectionName);
            JobPositionCollection = Database.GetCollection<JobPosition>(JobPositionCollectionName);
            CandidateCollection = Database.GetCollection<Candidate>(CandidateCollectionName);
            ColorPaletteCollection = Database.GetCollection<ColorPalette>(ColorPaletteCollectionName);
            ValuationTestCollection = Database.GetCollection<ValuationTest>(ValuationTestsCollectionName);
            ValuationTestQuestionCollection = Database.GetCollection<ValuationTestQuestion>(ValuationTestQuestionsCollectionName);
            ValuationTestResponseCollection = Database.GetCollection<ValuationTestResponse>(ValuationTestResponsesCollectionName);
            NetPromoterScoresCollection = Database.GetCollection<NetPromoterScore>(NetPromoterScoresCollectionName);
            ActivationsCollection = Database.GetCollection<Activation>(ActivationsCollectionName);
        }

        public IMongoDatabase Database { get; }

        public IMongoCollection<Event> EventCollection { get; }
        public IMongoCollection<EventApplication> EventApplicationCollection { get; set; }
        public IMongoCollection<User> UserCollection { get; }
        public IMongoCollection<Module> ModuleCollection { get; }
        public IMongoCollection<Question> QuestionCollection { get; }
        public IMongoCollection<Track> TrackCollection { get; }
        public IMongoCollection<Segment> SegmentCollection { get; }
        public IMongoCollection<Sector> SectorCollection { get; }
        public IMongoCollection<Rank> RankCollection { get; }
        public IMongoCollection<Job> JobCollection { get; }
        public IMongoCollection<UserSubjectProgress> UserSubjectProgressCollection { get; set; }
        public IMongoCollection<UserModuleProgress> UserModuleProgressCollection { get; set; }
        public IMongoCollection<UserTrackProgress> UserTrackProgressCollection { get; set; }
        public IMongoCollection<File> FileCollection { get; }
        public IMongoCollection<Company> CompanyCollection { get; }
        public IMongoCollection<BusinessGroup> BusinessGroupCollection { get; }
        public IMongoCollection<BusinessUnit> BusinessUnitCollection { get; }
        public IMongoCollection<FrontBackOffice> FrontBackOfficeCollection { get; }
        public IMongoCollection<Action> ActionCollection { get; }
        public IMongoCollection<Forum> ForumCollection { get; }
        public IMongoCollection<ForumQuestion> ForumQuestionCollection { get; }
        public IMongoCollection<ForumAnswer> ForumAnswerCollection { get; }
        public IMongoCollection<EventReaction> EventReactionCollection { get; }
        public IMongoCollection<Notification> NotificationCollection { get; }
        public IMongoCollection<Country> CountryCollection { get; }
        public IMongoCollection<Location> LocationCollection { get; }
        public IMongoCollection<Message> MessageCollection { get; }
        public IMongoCollection<ContactArea> ContactAreaCollection { get; }
        public IMongoCollection<CustomEmail> CustomEmailCollection { get; }
        public IMongoCollection<UserFile> UserFileCollection { get; }
        public IMongoCollection<UserContentNote> UserContentNoteCollection { get; }
        public IMongoCollection<ProfileTest> ProfileTestCollection { get; }
        public IMongoCollection<ProfileTestQuestion> ProfileTestQuestionCollection { get; }
        public IMongoCollection<ProfileTestResponse> ProfileTestResponseCollection { get; }
        public IMongoCollection<SuggestedProduct> SuggestedProductCollection { get; }
        public IMongoCollection<UserVerificationEmail> UserVerificationEmailCollection { get; }
        public IMongoCollection<ErrorLog> ErrorLogCollection { get; }
        public IMongoCollection<Institute> InstituteCollection { get; }
        public IMongoCollection<EventForum> EventForumCollection { get; }
        public IMongoCollection<EventForumQuestion> EventForumQuestionCollection { get; }
        public IMongoCollection<EventForumAnswer> EventForumAnswerCollection { get; }
        public IMongoCollection<ModuleDraft> ModuleDraftCollection { get; }
        public IMongoCollection<EventDraft> EventDraftCollection { get; }
        public IMongoCollection<QuestionDraft> QuestionDraftCollection { get; }
        public IMongoCollection<Responsible> ResponsibleCollection { get; }
        public IMongoCollection<UserCareer> UserCareerCollection { get; }
        public IMongoCollection<PurchaseHistory> PurchaseHistoryCollection { get; }
        public IMongoCollection<ModuleGrade> ModuleGradeCollection { get; }
        public IMongoCollection<Formula> FormulaCollection { get; }
        public IMongoCollection<FormulaVariables> FormulaTypeVariablesCollection { get; }
        public IMongoCollection<RecruitingCompany> RecruitingCompanyCollection { get; }
        public IMongoCollection<RecruitmentFavorite> RecruitmentFavoriteCollection { get; }
        public IMongoCollection<AuditLog> AuditLogCollection { get; }
        public IMongoCollection<JobPosition> JobPositionCollection { get; }
        public IMongoCollection<Candidate> CandidateCollection { get; }
        public IMongoCollection<ColorPalette> ColorPaletteCollection { get; }
        public IMongoCollection<ValuationTest> ValuationTestCollection { get; }
        public IMongoCollection<ValuationTestQuestion> ValuationTestQuestionCollection { get; }
        public IMongoCollection<ValuationTestResponse> ValuationTestResponseCollection { get; }
        public IMongoCollection<NetPromoterScore> NetPromoterScoresCollection { get; }
        public IMongoCollection<Activation> ActivationsCollection { get; }
    }
}