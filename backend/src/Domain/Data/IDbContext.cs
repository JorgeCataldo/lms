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
using MongoDB.Driver;
using Domain.Aggregates.Forums;
using Domain.Aggregates.Notifications;
using Domain.Aggregates.Locations;
using Domain.Aggregates.Countries;
using Domain.Aggregates.Messages;
using Domain.Aggregates.UserFiles;
using Domain.Aggregates.UserContentNotes;
using Domain.Aggregates.VerificationEmail;
using Domain.Aggregates.ProfileTests;
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

namespace Domain.Data
{
    public interface IDbContext
    {
        IMongoCollection<Event> EventCollection { get; }
        IMongoCollection<EventApplication> EventApplicationCollection { get; set; }
        IMongoCollection<User> UserCollection { get; }
        IMongoCollection<Module> ModuleCollection { get; }
        IMongoCollection<Question> QuestionCollection { get; }
        IMongoCollection<Track> TrackCollection { get; }
        IMongoCollection<UserSubjectProgress> UserSubjectProgressCollection { get; set; }
        IMongoCollection<UserModuleProgress> UserModuleProgressCollection { get; set; }
        IMongoCollection<UserTrackProgress> UserTrackProgressCollection { get; set; }
        IMongoCollection<Segment> SegmentCollection { get; }
        IMongoCollection<Sector> SectorCollection { get; }
        IMongoCollection<Rank> RankCollection { get; }
        IMongoCollection<Job> JobCollection { get; }
        IMongoCollection<File> FileCollection { get; }
        IMongoCollection<Company> CompanyCollection { get; }
        IMongoCollection<BusinessGroup> BusinessGroupCollection { get; }
        IMongoCollection<BusinessUnit> BusinessUnitCollection { get; }
        IMongoCollection<FrontBackOffice> FrontBackOfficeCollection { get; }
        IMongoCollection<Action> ActionCollection { get; }
        IMongoCollection<Forum> ForumCollection { get; }
        IMongoCollection<ForumQuestion> ForumQuestionCollection { get; }
        IMongoCollection<ForumAnswer> ForumAnswerCollection { get; }
        IMongoCollection<EventReaction> EventReactionCollection { get; }
        IMongoCollection<Notification> NotificationCollection { get; }
        IMongoCollection<Country> CountryCollection { get; }
        IMongoCollection<Location> LocationCollection { get; }
        IMongoCollection<Message> MessageCollection { get; }
        IMongoCollection<ContactArea> ContactAreaCollection { get; }
        IMongoCollection<CustomEmail> CustomEmailCollection { get; }
        IMongoCollection<UserFile> UserFileCollection { get; }
        IMongoCollection<UserContentNote> UserContentNoteCollection { get; }
        IMongoCollection<ProfileTest> ProfileTestCollection { get; }
        IMongoCollection<ProfileTestQuestion> ProfileTestQuestionCollection { get; }
        IMongoCollection<ProfileTestResponse> ProfileTestResponseCollection { get; }
        IMongoCollection<SuggestedProduct> SuggestedProductCollection { get; }
        IMongoCollection<UserVerificationEmail> UserVerificationEmailCollection { get; }
        IMongoCollection<ErrorLog> ErrorLogCollection { get; }
        IMongoCollection<Institute> InstituteCollection { get; }
        IMongoCollection<EventForum> EventForumCollection { get; }
        IMongoCollection<EventForumQuestion> EventForumQuestionCollection { get; }
        IMongoCollection<EventForumAnswer> EventForumAnswerCollection { get; }
        IMongoCollection<ModuleDraft> ModuleDraftCollection { get; }
        IMongoCollection<EventDraft> EventDraftCollection { get; }
        IMongoCollection<QuestionDraft> QuestionDraftCollection { get; }
        IMongoCollection<Responsible> ResponsibleCollection { get; }
        IMongoCollection<UserCareer> UserCareerCollection { get; }
        IMongoCollection<PurchaseHistory> PurchaseHistoryCollection { get; }
        IMongoCollection<ModuleGrade> ModuleGradeCollection { get; }
        IMongoCollection<Formula> FormulaCollection { get; }
        IMongoCollection<RecruitingCompany> RecruitingCompanyCollection { get; }
        IMongoCollection<RecruitmentFavorite> RecruitmentFavoriteCollection { get; }
        IMongoCollection<FormulaVariables> FormulaTypeVariablesCollection { get; }
        IMongoCollection<AuditLog> AuditLogCollection { get; }
        IMongoCollection<JobPosition> JobPositionCollection { get; }
        IMongoCollection<Candidate> CandidateCollection { get; }
        IMongoCollection<ColorPalette> ColorPaletteCollection { get; }
        IMongoCollection<ValuationTest> ValuationTestCollection { get; }
        IMongoCollection<ValuationTestQuestion> ValuationTestQuestionCollection { get; }
        IMongoCollection<ValuationTestResponse> ValuationTestResponseCollection { get; }
        IMongoCollection<NetPromoterScore> NetPromoterScoresCollection { get; }
        IMongoCollection<Activation> ActivationsCollection { get; }
        IMongoDatabase Database { get; }
    }
}