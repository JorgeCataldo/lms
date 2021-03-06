
import { BaseUrlService } from '@tg4/http-infrastructure/dist/src';
import { environment } from '../../../environments/environment';

export class UrlService extends BaseUrlService {

  constructor() {
    super();
    this.addUrls(environment.apiUrl, new Map<string, string>([
      /* Login and Authentication*/
      ['login', 'api/account/login'],
      ['loginLinkedIn', 'api/account/linkedIn'],
      ['bindLinkedIn', 'api/account/bindLinkedIn'],
      ['register', 'api/account/register'],
      ['sendVerificationEmail', 'api/account/sendVerificationEmail'],
      ['verifyEmailCode', 'api/account/verifyEmailCode'],
      ['firstAccess', 'api/account/firstAccess'],
      ['forgotPassword', 'api/account/forgotPassword'],
      /* Pages > Queries */
      ['getPagedModulesList', 'api/module'],
      ['getAllContent', 'api/module/getAllContent'],
      ['getModuleById', 'api/module/byId'],
      ['deleteModuleById', 'api/module/deleteModule'],
      ['getModuleOverview', 'api/module/overview'],
      ['getForumByModule', 'api/forum/forumByModule'],
      ['getForumQuestionById', 'api/forum/question'],
      ['getForumPreviewByModule', 'api/forum/previewForumQuestions'],
      ['getNotifications', 'api/user/getNotifications'],
      ['getTrackCurrentStudentOverview', 'api/track/overview/student/current'],
      ['getContactAreas', 'api/user/getContactAreas'],
      ['getEventForumByEventSchedule', 'api/eventForum/eventForumByEventSchedule'],
      ['getEventForumQuestionById', 'api/eventForum/eventForumQuestion'],
      ['getEventForumPreviewByEventSchedule', 'api/eventForum/previewEventForumQuestions'],
      ['getUserEventForumPreviewByEventSchedule', 'api/eventForum/userPreviewEventForumQuestions'],
      /* Pages > Commands  */
      ['saveForumQuestion', 'api/forum/saveQuestion'],
      ['manageQuestionLike', 'api/forum/manageQuestionLike'],
      ['manageAnswerLike', 'api/forum/question/answer/manageLike'],
      ['saveForumQuestionAnswer', 'api/forum/question/answer'],
      ['removeForumQuestion', 'api/forum/question/remove'],
      ['removeForumAnswer', 'api/forum/question/remove/answer'],
      ['manageNotification', 'api/user/manageNotification'],
      ['sendMessage', 'api/user/sendMessage'],
      ['sendCustomEmail', 'api/user/sendCustomEmail'],
      ['pagedCustomEmails', 'api/user/pagedCustomEmails'],
      ['markMandatoryVideoViewed', 'api/track/overview/video/markViewed'],
      ['saveEventForumQuestion', 'api/eventForum/saveEventForumQuestion'],
      ['manageEventForumQuestionLike', 'api/eventForum/manageEventForumQuestionLike'],
      ['manageEventForumAnswerLike', 'api/eventForum/eventForumQuestion/answer/manageLike'],
      ['saveEventForumQuestionAnswer', 'api/eventForum/eventForumQuestion/answer'],
      ['removeEventForumQuestion', 'api/eventForum/eventForumQuestion/remove'],
      ['removeEventForumAnswer', 'api/eventForum/eventForumQuestion/remove/answer'],
      /* Settings > Queries */
      ['getPagedFilteredModulesList', 'api/module/filtered'],
      ['getPagedManagerFilteredModulesList', 'api/module/filtered/manager'],
      ['getPagedMyCoursesFilteredModulesList', 'api/module/filtered/mycourses'],
      ['getPagedEffortPerformancesFilteredModulesList', 'api/module/filtered/effortPerformance'],
      ['getPagedHomeModulesList', 'api/module/home'],
      ['getPagedQuestionsList', 'api/question'],
      ['getPagedFilteredQuestionsList', 'api/question/filtered'],
      ['getQuestionsList', 'api/question/getAllQuestions'],
      ['validateModuleQuestions', 'api/question/validateModuleQuestions'],
      ['getPagedFilteredTracksList', 'api/track/filtered'],
      ['getNotRecommendedTracks', 'api/track/getNotRecommendedTracks'],
      ['getPagedFilteredMyCoursesTracksList', 'api/track/filtered/mycourses'],
      ['getPagedFilteredEffortPerformancesTracksList', 'api/track/filtered/effortPerformance'],
      ['getTrackById', 'api/track'],
      ['getTrackOverview', 'api/track/overview'],
      ['getTrackOverviewGrades', 'api/track/overview/grades'],
      ['getTrackOverviewStudents', 'api/track/overview/students'],
      ['getTrackOverviewEventInfo', 'api/track/overview/eventInfo'],
      ['getTrackStudentOverview', 'api/track/overview/student'],
      ['getTrackModuleOverview', 'api/track/overview/module'],
      ['getAllTrackContent', 'api/track/getAllContent'],
      ['getAllTracks', 'api/track/getAllTracks'],
      ['getAllFilteredTracksList', 'api/track/getAllFilteredTracksList'],
      ['getPagedFilteredEventsList', 'api/event/filtered'],
      ['addCalendarEventsFromFile', 'api/track/addCalendarEventsFromFile'],
      ['getHomeEventsList', 'api/event/homeEvents'],
      ['getEventById', 'api/event'],
      ['getEventReactions', 'api/event/getEventReactions'],
      ['getPastEvents', 'api/event/pastEvents'],
      ['addEventUsersGradeBaseValues', 'api/event/addEventUsersGradeBaseValues'],
      ['getEventStudentList', 'api/event/getEventStudentList'],
      ['getUserTypes', 'api/user/userTypes'],
      ['getUserFiles', 'api/user/getUserFiles'],
      ['getProfileTests', 'api/profileTest'],
      ['deleteProfileTest', 'api/profileTest'],
      ['getProfileTestById', 'api/profileTest/byId'],
      ['getProfileTestResponses', 'api/profileTest/responses'],
      ['getProfileTestResponseById', 'api/profileTest/responses/byId'],
      ['getSuggestedProducts', 'api/profileTest/suggested'],
      ['getAllProfileTestResponses', 'api/profileTest/responses/all'],
      /* Settings > Commands  */
      ['addNewModule', 'api/module'],
      ['updateModuleInfo', 'api/module/updateModule'],
      ['manageSubjects', 'api/module/manageSubjects'],
      ['manageContents', 'api/module/manageContents'],
      ['manageSupportMaterials', 'api/module/manageSupportMaterials'],
      ['manageRequirements', 'api/module/manageRequirements'],
      ['manageQuestion', 'api/question'],
      ['removeQuestion', 'api/question'],
      ['manageTrackInfo', 'api/track'],
      ['deleteTrackById', 'api/track/deleteTrack'],
      ['removeTrack', 'api/track'],
      ['manageEvent', 'api/event'],
      ['manageEventSupportMaterials', 'api/event/manageSupportMaterials'],
      ['manageEventRequirements', 'api/event/manageRequirements'],
      ['manageEventSchedule', 'api/event/manageSchedule'],
      ['getEventApplicationByUserQuery', 'api/event/getEventApplicationByUserQuery'],
      ['getEventApplicationByEventId', 'api/event/getEventApplicationByEventId'],
      ['getEventSchedulesByEventId', 'api/event/getEventSchedulesByEventId'],
      ['changeEventPublishedStatus', 'api/event/changeEventPublishedStatus'],
      ['changeEventUserApplicationStatus', 'api/event/changeEventUserApplicationStatus'],
      ['changeEventUserGrade', 'api/event/changeEventUserGrade'],
      ['changeEventUserForumGrade', 'api/event/changeEventUserForumGrade'],
      ['applyToEvent', 'api/event/applyToEvent'],
      ['deleteEventById', 'api/event/deleteEvent'],
      ['addEventReaction', 'api/event/addEventReaction'],
      ['manageEventReaction', 'api/event/manageEventReaction'],
      ['manageUserPresence', 'api/event/manageUserPresence'],
      ['finishEvent', 'api/event/finishEvent'],
      ['sendEventEvaluationEmail', 'api/event/sendEventReactionEvaluation'],
      ['changeUserEventApplicationSchedule', 'api/event/changeUserEventApplicationSchedule'],
      ['importQdb', 'api/question/importQdb'],
      ['manageCalendarEvents', 'api/track/manageCalendarEvents'],
      ['manageEcommerceProducts', 'api/track/manageEcommerceProducts'],
      ['manageUserFiles', 'api/user/manageUserFiles'],
      ['addUserFiles', 'api/user/addUserFiles'],
      ['addAssesmentUserFiles', 'api/user/addAssesmentUserFiles'],
      ['setQuestionsLimit', 'api/module/setQuestionsLimit'],
      ['manageEcommerceModuleDraft', 'api/moduleDraft/manageEcommerceModuleDraft'],
      // Profile Tests
      ['manageProfileTest', 'api/profileTest/manage'],
      ['suggestProducts', 'api/profileTest/suggest'],
      ['suggestProfileTest', 'api/profileTest/suggest/test'],
      ['saveProfileTestResponse', 'api/profileTest/response/save'],
      ['gradeProfileTestAnswers', 'api/profileTest/responses/grade'],
      // Formulas
      ['getFormulas', 'api/formula'],
      ['getFormulaById', 'api/formula/byId'],
      ['addFormula', 'api/formula'],
      ['manageFormula', 'api/formula/manage'],
      ['deleteFormula', 'api/formula/delete'],
      ['getFormulaVariables', 'api/formula/variables/byType'],
      // Modules Drafts
      ['getPagedModulesAndDrafts', 'api/moduleDraft/paged'],
      ['addNewModuleDraft', 'api/moduleDraft'],
      ['updateModuleDraft', 'api/moduleDraft/updateModule'],
      ['cloneModuleDraft', 'api/moduleDraft/clone'],
      ['manageDraftSubjects', 'api/moduleDraft/manageSubjects'],
      ['manageModuleWeight', 'api/moduleDraft/manageModuleWeight'],
      ['manageDraftContents', 'api/moduleDraft/manageContents'],
      ['manageDraftSupportMaterials', 'api/moduleDraft/manageSupportMaterials'],
      ['manageDraftRequirements', 'api/moduleDraft/manageRequirements'],
      ['publishDraft', 'api/moduleDraft/publishDraft'],
      ['rejectDraft', 'api/moduleDraft/deleteModule'],
      ['getDraftById', 'api/moduleDraft/byId'],
      ['setDraftQuestionsLimit', 'api/moduleDraft/setQuestionsLimit'],
      ['sendFileToS3', 'api/moduleDraft/sendFileToS3'],
      // Events Drafts
      ['getPagedEventsAndDrafts', 'api/eventDraft/paged'],
      ['getEventDraftById', 'api/eventDraft/byId'],
      ['addNewEventDraft', 'api/eventDraft'],
      ['manageEventDraftSchedule', 'api/eventDraft/manageSchedule'],
      ['manageEventDraftSupportMaterials', 'api/eventDraft/manageSupportMaterials'],
      ['manageEventDraftRequirements', 'api/eventDraft/manageRequirements'],
      ['publishEventDraft', 'api/eventDraft/publishDraft'],
      ['rejectEventDraft', 'api/eventDraft/deleteEvent'],
      // Questions Drafts
      ['manageQuestionDraft', 'api/questionDraft'],
      ['getAllQuestionsDraft', 'api/questionDraft/all'],
      ['getPagedQuestionsDraft', 'api/questionDraft/paged'],
      ['removeQuestionDraft', 'api/questionDraft'],
      ['importDraftQdb', 'api/questionDraft/importQdb'],
      // Recruiting Company
      ['manageRecruitingCompany', 'api/recruitingCompany/manage'],
      ['getRecruitingCompany', 'api/recruitingCompany'],
      ['addRecruitmentFavorite', 'api/recruitingCompany/favorite'],
      ['removeRecruitmentFavorite', 'api/recruitingCompany/favorite'],
      ['getTalentsList', 'api/recruitingCompany/talents/list'],
      /* exam and Progress */
      ['getsubjectprogress', 'api/user/getsubjectprogress'],
      ['getmoduleprogress', 'api/user/getmoduleprogress'],
      ['getmodulesprogress', 'api/user/getmodulesprogress'],
      ['gettracksprogress', 'api/user/gettracksprogress'],
      ['getTrackProgress', 'api/user/getTrackProgress'],
      ['examstart', 'api/exam/startexam'],
      ['examanswer', 'api/exam/answer'],
      /* Shared */
      ['uploadImage', 'api/uploadImage/base64'],
      ['uploadFile', 'api/uploadfile/base64'],
      ['getLevels', 'api/level'],
      /* User > Queries */
      ['getUserById', 'api/user'],
      ['getUserProfile', 'api/user/userProfile'],
      ['getUserRecommendationBasicInfo', 'api/user/userRecommendationBasicInfo'],
      ['getPagedUser', 'api/user/pagedUser'],
      ['getFilteredPagedUser', 'api/user/filteredPagedUser'],
      ['getUserCategories', 'api/user/getUserCategories'],
      ['pagedUsersSyncProcesse', 'api/user/pagedUsersSyncProcesse'],
      ['getProfessors', 'api/user/getProfessors'],
      ['getUserContentNote', 'api/user/getUserContentNote'],
      ['getUserCareer', 'api/user/getUserCareer'],
      ['getUserInstitutions', 'api/user/getUserInstitutions'],
      ['exportUsersCareer', 'api/user/exportUsersCareer'],
      ['exportUsersGrade', 'api/user/exportUsersGrade'],
      ['getUserSkills', 'api/user/getUserSkills'],
      ['exportUsersEffectivenessIndicators', 'api/module/effectivenessIndicators'],
      ['getCEP', 'api/user/GetCEP'],
      ['exportCareerUsers', 'api/report/exportCareerUsers'],
      ['getUserColorPalette', 'api/user/getUserColorPalette'],
      ['getBasicProgressInfo', 'api/user/getBasicProgressInfo'],
      ['getUserToImpersonate', 'api/user/getUserToImpersonate'],
      /* User > Commands */
      ['changeUserBlockedStatus', 'api/user/changeUserBlockedStatus'],
      ['addOrModifyUser', 'api/user/createUpdateUser'],
      ['addUsersSyncProcess', 'api/user/AddUsersSyncProcess'],
      ['updateUserRecommendations', 'api/user/updateUserRecommendations'],
      ['changePassword', 'api/account/ChangePassword'],
      ['adminChangePassword', 'api/account/AdminChangePassword'],
      ['blockUserMaterial', 'api/user/blockUserMaterial'],
      ['updateUserContentNote', 'api/user/updateUserContentNote'],
      ['updateUserCareer', 'api/user/updateUserCareer'],
      ['allowRecommendation', 'api/user/allowRecommendation'],
      ['allowSecretaryRecommendation', 'api/user/secretary/allowRecommendation'],
      ['getAllEventsByUser', 'api/event/GetAllEventsByUser'],
      ['seeHow', 'api/user/seeHow'],
      ['updateUserColorPalette', 'api/user/updateUserColorPalette'],
      /* Analytics */
      ['saveAction', 'api/analytics'],
      ['removeAction', 'api/analytics/removeAction'],
      /* Responsible > Commands */
      ['createResponsibleTree', 'api/responsible/createResponsibleTree'],
      /* Audit Log */
      ['getAllUpdatedQuestionsDraft', 'api/auditLog/allQuestions'],
      ['getPagedAuditLogs', 'api/auditLog/paged'],
      ['getAllLogs', 'api/auditLog/allLogs'],
      /* Report Log */
      ['getTrackReportStudents', 'api/report/getTrackReportStudents'],
      ['effectivenessIndicators', 'api/report/effectivenessIndicators'],
      ['getUserProgressReport', 'api/report/userProgress'],
      ['getTracksGrades', 'api/report/trackGrades'],
      ['getFinanceReport', 'api/report/getFinanceReport'],
      ['getTrackAnswers', 'api/report/getTrackAnswers'],
      ['getAtypicalMovements', 'api/report/getAtypicalMovements'],
      ['getTrackNps', 'api/report/getTrackNps'],
      /* RecruitingCompany */
      ['getJobPositions', 'api/recruitingCompany/positions'],
      ['getJobPositionById', 'api/recruitingCompany/positionById'],
      ['addJobPosition', 'api/recruitingCompany/position'],
      ['deleteCandidateJobPosition', 'api/recruitingCompany/candidate'],
      ['updateJobPosition', 'api/recruitingCompany/position'],
      ['updateJobPositionStatus', 'api/recruitingCompany/positionStatus'],
      ['addCandidatesJobPosition', 'api/recruitingCompany/candidate'],
      ['approveCandidateToJobPosition', 'api/recruitingCompany/candidate/approve'],
      ['rejectCandidateToJobPosition', 'api/recruitingCompany/candidate/reject'],
      ['getAvailableCandidates', 'api/recruitingCompany/availableCandidates'],
      ['getUserJobPosition', 'api/recruitingCompany/user/positions'],
      ['approveUserJobPosition', 'api/recruitingCompany/acceptJob'],
      ['getJobsList', 'api/recruitingCompany/jobs/list'],
      ['applyTojob', 'api/recruitingCompany/user/applyTojob'],
      ['getUserJobPositionsById', 'api/recruitingCompany/user/positionById'],
      ['getUserJobNotifications', 'api/recruitingCompany/user/jobNotifications'],
      /* Valuatiom Test */
      ['getValuationTests', 'api/valuationTest'],
      ['deleteValuationTest', 'api/valuationTest'],
      ['getValuationTestById', 'api/valuationTest/byId'],
      ['getValuationTestResponses', 'api/valuationTest/responses'],
      ['getValuationTestResponseById', 'api/valuationTest/responses/byId'],
      ['getAllValuationTestResponses', 'api/valuationTest/responses/all'],
      ['manageValuationTest', 'api/valuationTest/manage'],
      ['gradeValuationTestAnswers', 'api/valuationTest/responses/grade'],
      ['getModuleValuationTests', 'api/valuationTest/module'],
      ['getTrackValuationTests', 'api/valuationTest/track'],
      ['saveValuationTestResponse', 'api/valuationTest/responses/save'],
      ['getAllLocations', 'api/location/getAll'],
      /* Nps */
      ['getUserNpsInfos', 'api/user/getFilteredNps'],
      ['saveNpsValuation', 'api/user/saveNps'],
      ['getAllNpsInfos', 'api/user/getAllNps'],
      ['getUserNpsAvailability', 'api/user/getUserNpsAvailability'],
      /* Activations */
      ['getActivations', 'api/activation/getActivations'],
      ['getActivationByType', 'api/activation/GetActivationByType'],
      ['updateActivationStatus', 'api/activation/updateActivationStatus'],
      ['createCustomActivation', 'api/activation/createCustomActivation'],
      ['updateCustomActivation', 'api/activation/updateCustomActivation'],
      ['deleteActivation', 'api/activation']
    ]));
  }

}
