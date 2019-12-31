import { NgModule } from '@angular/core';
import { RouterModule, Routes, UrlSegment } from '@angular/router';
import { HomeComponent } from './pages/home/home.component';
import { ModuleComponent } from './pages/module/module.component';
import { ClassroomLessonComponent } from './pages/classroom-lesson/classroom-lesson.component';
import { ContentComponent } from './pages/content/content.component';
import { SearchResultsComponent } from './pages/search-results/search-results.component';
import { ExamComponent } from './pages/exam/exam.component';
import { TrackComponent } from './pages/track/track.component';
import { SettingsModulesComponent } from './settings/modules/modules.component';
import { SettingsNewModuleComponent } from './settings/modules/new-module/new-module.component';
import { SettingsEventsComponent } from './settings/events/events.component';
import { SettingsNewEventComponent } from './settings/events/new-event/new-event.component';
import { SettingsTracksComponent } from './settings/tracks/tracks.component';
import { SettingsNewTrackComponent } from './settings/tracks/new-track/new-track.component';
import { LoginComponent } from './pages/login/login.component';
import { AuthGuard } from './shared/guards/auth.guard';
import { RoleGuard } from './shared/guards/role.guard';
import { SettingsUsersComponent } from './settings/users/users.component';
import { SettingsManageUserComponent } from './settings/users/manage-user/manage-user.component';
import { SettingsManageUserPasswordComponent } from './settings/users/manage-user-password/manage-user-password.component';
import { SettingsUserDetailsComponent } from './settings/users/user-details/user-details.component';
import { SettingsUsersSyncComponent } from './settings/users-sync/users-sync.component';
import { SettingsUsersSyncDetailsComponent } from './settings/users-sync/sync-details/sync-details.component';
import { SettingsEventsApplicationsComponent } from './settings/events-applications/events-applications.component';
import { SettingsManageTeamComponent } from './settings/manage-team/manage-team.component';
import { SettingsRecommendTrackComponent } from './settings/manage-team/recommend-track/recommend-track.component';
import { SettingsEventDetailsComponent } from './settings/events/event-details/event-details.component';
import { SettingsEventsApplicationsGradesComponent } from './settings/events-applications-grades/events-applications-grades.component';
import { ForumComponent } from './pages/forum/forum.component';
import { ForumQuestionComponent } from './pages/forum/forum-question/forum-question.component';
import { EventsValuationComponent } from './settings/events-valuation/events-valuation.component';
import { SettingsEventResultsComponent } from './settings/events/event-results/event-results.component';
import { TrackOverviewComponent } from './pages/track/track-overview/track-overview.component';
import { SettingsTrackOverviewComponent } from './settings/tracks/track-overview/track-overview.component';
import { TrackStudentOverviewComponent } from './settings/tracks/track-overview/student-overview/student-overview.component';
import { TrackModuleOverviewComponent } from './settings/tracks/track-overview/module-overview/module-overview.component';
import { MessagesComponent } from './pages/messages/messages.component';
import { FileUploadComponent } from './settings/users/user-archive-upload/file-upload.component';
import { SettingsCustomEmailComponent } from './settings/custom-email/custom-email.component';
import { SettingsCustomEmailSentComponent } from './settings/custom-email-sent/custom-email-sent.component';
import { ProfileTestsComponent } from './settings/profile-tests/profile-tests.component';
import { SettingsManageTestComponent } from './settings/profile-tests/manage-test/manage-test.component';
import { ProductSuggestionComponent } from './settings/product-suggestion/product-suggestion.component';
import { ManageSuggestionComponent } from './settings/product-suggestion/manage-suggestion/manage-suggestion.component';
import { SettingsSuggestTestComponent } from './settings/manage-team/suggest-test/suggest-test.component';
import { ProfileTestComponent } from './pages/profile-test/profile-test.component';
import { EmailConfirmationComponent } from './pages/login/email-validation/email-confirmation.component';
import { RegisterComponent } from './pages/register/register.component';
import { SettingsManageUserCareerComponent } from './settings/users/manage-user-career/manage-user-career.component';
import { ErrorComponent } from './pages/error/error.component';
import { EventForumComponent } from './pages/event-forum/event-forum.component';
import { EventForumQuestionComponent } from './pages/event-forum/event-forum-question/event-forum-question.component';
import { SettingsUserRecommendationComponent } from './settings/users/user-recommendation/user-recommendation.component';
import { FormulasComponent } from './settings/formulas/formulas.component';
import { ManageFormulaComponent } from './settings/formulas/manage-formula/manage-formula.component';
import { RecruitingCompanyFormComponent } from './recruitment/recruiting-company/company-form/company-form.component';
import { RecruitmentManageTalentsComponent } from './recruitment/manage-talents/manage-talents.component';
import { LogsComponent } from './settings/logs/logs.component';
import { ManageJobsCompanyFormComponent } from './recruitment/manage-jobs/manage-jobs.component';
import { ManageJobApplicationsComponent } from './recruitment/job-applications/job-applications.component';
import { EmploymentComponent } from './recruitment/manage-jobs/employment/employment.component';
import { SettingsUserPerformaceComponent } from './settings/users/user-details/user-performace/user-performace.component';
import { SettingsUserCalendarComponent } from './settings/users/user-details/user-calendar/user-calendar.component';
import { ModuleInfoComponent } from './pages/module/module-info/module-info.component';
import { ColorEditComponent } from './settings/color-edit/color-edit.component';
import { RecJobsComponent } from './recruitment/student-job/rec-jobs/rec-jobs.component';
import { StudentJobsComponent } from './recruitment/student-job/jobs/student-jobs.component';
import { StudentSearchJobComponent } from './recruitment/student-job/student-search-job/student-search-job.component';
import { CandidacyStudentsComponent } from './recruitment/student-job/candidacy/candidacy-students.component';
import { ValuationTestsComponent } from './settings/valuation-tests/valuation-tests.component';
import { SettingsManageValuationTestComponent } from './settings/valuation-tests/manage-valuation-test/manage-valuation-test.component';
import { ValuationTestComponent } from './pages/valuation-test/valuation-test.component';
import { SettingsValuationTestComponent } from './settings/valuation-test/valuation-test.component';
import { SettingsValuationTestGradeComponent } from './settings/valuation-test/valuation-test-grade/valuation-test-grade.component';
import { ReportComponent } from './pages/report/report.component';
// tslint:disable-next-line: max-line-length
import { SettingsManageValuationTestReleaseComponent } from './settings/valuation-tests/manage-valuation-test-release/manage-valuation-test-release.component';
import { NpsComponent } from './settings/nps/nps.component';
import { MyCoursesComponent } from './pages/my-courses/my-courses.component';
import { EcommerceComponent } from './pages/e-commerce/e-commerce.component';
import { CompetenceMapComponent } from './pages/competence-map/competence-map.component';
import { TrainningPerformanceComponent } from './pages/trainning-performance/trainning-performance.component';
// tslint:disable-next-line: max-line-length
import { ProfessionalPerformanceHistoryComponent } from './pages/professional-performance-history/professional-performance-history.component';
import { EffortPerformanceComponent } from './pages/effort-performance/effort-performance.component';
import { DataQuestionsTagsComponent } from './pages/data-questions-tags/data-questions-tags.component';
import { SettingsManageUserRegisterComponent } from './settings/manage-user-register/manage-user-register.component';
import { ValuationTestsResultsComponent } from './pages/valuation-tests-results/valuation-tests-results.component';
import { ProfileTestsResultsComponent } from './pages/profile-test-results/profile-tests-results.component';

export function reviewMatcherFunction(url: UrlSegment[]) {
  if (url && url.length > 4 && url[0].path.match('configuracoes') && url[1].path.match('avaliar-evento')) {
    return ({consumed: url,
      posParams: {
        eventId: url[2],
        scheduleId: url[3]
      }});
  } else {
    return null;
  }
}

const appRoutes: Routes = [
  {
    path: '', component: LoginComponent,
    data: { page: 'login' }
  }, {
    path: 'register', component: RegisterComponent,
    data: { page: 'register' }
  }, {
    path: 'confirmacao-email', component: EmailConfirmationComponent,
    data: { page: 'email-confirmation' }
  }, {
    path: 'home', component: HomeComponent,
    data: { page: 'home' }, canActivate: [ AuthGuard ]
  }, {
    path: 'erro', component: ErrorComponent,
    data: { page: 'error' }
  }, {
    path: 'busca/:searchValue', component: SearchResultsComponent,
    data: { page: 'search-results' }, canActivate: [ AuthGuard ]
  }, {
    path: 'trilha/:trackId', component: TrackComponent,
    data: { page: 'track' }, canActivate: [ AuthGuard ]
  }, {
    path: 'trilha-de-curso/:trackId', component: TrackOverviewComponent,
    data: { page: 'track' }, canActivate: [ AuthGuard ]
  }, {
    path: 'modulo/:moduleId', component: ModuleComponent,
    data: { page: 'module' }, canActivate: [ AuthGuard ]
  }, {
    path: 'modulo/:moduleId/:subjectId/:contentIndex', component: ContentComponent,
    data: { page: 'content' }, canActivate: [ AuthGuard ]
  }, {
    path: 'evento/:eventId/:scheduleId', component: ClassroomLessonComponent,
    data: { page: 'event' }, canActivate: [ AuthGuard ]
  }, {
    path: 'avaliacao/:moduleId/:subjectId', component: ExamComponent,
    data: { page: 'exam' }, canActivate: [ AuthGuard ]
  }, {
    path: 'forum/:moduleId', component: ForumComponent,
    data: { page: 'forum' }, canActivate: [ AuthGuard ]
  }, {
    path: 'forum/:moduleName/:moduleId/:questionId', component: ForumQuestionComponent,
    data: { page: 'forum' }, canActivate: [ AuthGuard ]
  }, {
    path: 'forum-evento/:eventId/:eventScheduleId', component: EventForumComponent,
    data: { page: 'forum-evento' }, canActivate: [ AuthGuard ]
  }, {
    path: 'forum-evento/:eventId/:eventScheduleId/:questionId', component: EventForumQuestionComponent,
    data: { page: 'forum-evento' }, canActivate: [ AuthGuard ]
  }, {
    path: 'atendimento', component: MessagesComponent,
    data: { page: 'atendimento' }, canActivate: [ AuthGuard ]
  }, {
    path: 'teste-de-perfil/:testId', component: ProfileTestComponent,
    data: { page: 'profile-test' }, canActivate: [ AuthGuard ]
  },
  /* Settings */ {
    path: 'configuracoes/modulos', component: SettingsModulesComponent,
    data: { page: 'settings-modules' }, canActivate: [ AuthGuard ]
  }, {
    path: 'configuracoes/modulo', component: SettingsNewModuleComponent,
    data: { page: 'settings-new-module' }, canActivate: [ AuthGuard ]
  }, {
    path: 'configuracoes/eventos', component: SettingsEventsComponent,
    data: { page: 'settings-events' }, canActivate: [ AuthGuard ]
  }, {
    path: 'configuracoes/evento', component: SettingsNewEventComponent,
    data: { page: 'settings-new-event' }, canActivate: [ AuthGuard ]
  }, {
    path: 'configuracoes/avaliar-evento/:eventId/:scheduleId', component: EventsValuationComponent,
    data: { page: 'event' }, canActivate: [ AuthGuard ]
  }, {
    matcher: reviewMatcherFunction, component: EventsValuationComponent,
    data: { page: 'event' }, canActivate: [ AuthGuard ]
  }, {
    path: 'configuracoes/trilhas', component: SettingsTracksComponent,
    data: { page: 'settings-tracks' }, canActivate: [ AuthGuard, RoleGuard ]
  }, {
    path: 'configuracoes/trilha', component: SettingsNewTrackComponent,
    data: { page: 'settings-new-track' }, canActivate: [ AuthGuard, RoleGuard ]
  }, {
    path: 'configuracoes/trilha-de-curso/:trackId', component: SettingsTrackOverviewComponent,
    data: { page: 'settings-track-overview' }, canActivate: [ AuthGuard, RoleGuard ]
  }, {
    path: 'configuracoes/trilha-de-curso/:trackId/:studentId', component: TrackStudentOverviewComponent,
    data: { page: 'settings-track-student-overview' }, canActivate: [ AuthGuard ]
  }, {
    path: 'configuracoes/trilha-de-curso/:trackId/modulo/:moduleId', component: TrackModuleOverviewComponent,
    data: { page: 'settings-track-module-overview' }, canActivate: [ AuthGuard, RoleGuard ]
  }, {
    path: 'configuracoes/usuarios', component: SettingsUsersComponent,
    data: { page: 'settings-users' }, canActivate: [ AuthGuard ]
  }, {
    path: 'configuracoes/usuarios/:userId', component: SettingsManageUserComponent,
    data: { page: 'settings-manage-user' }, canActivate: [ AuthGuard ]
  }, {
    path: 'configuracoes/usuarios/recomendacao/:userId', component: SettingsUserRecommendationComponent,
    data: { page: 'settings-manage-user-recommendation' }, canActivate: [ AuthGuard ]
  }, {
    path: 'configuracoes/usuarios/carreira/:userId', component: SettingsManageUserCareerComponent,
    data: { page: 'settings-manage-user-career' }, canActivate: [ AuthGuard ]
  }, {
    path: 'configuracoes/usuarios/senha/:userId', component: SettingsManageUserPasswordComponent,
    data: { page: 'settings-manage-user-password' }, canActivate: [ AuthGuard ]
  }, {
    path: 'configuracoes/detalhes-usuario/:userId', component: SettingsUserDetailsComponent,
    data: { page: 'settings-user-details' }, canActivate: [ AuthGuard ]
  }, {
    path: 'configuracoes/card-recomendacao/:userId', component: SettingsUserRecommendationComponent,
    data: { page: 'settings-user-details' }, canActivate: [ AuthGuard ]
  }, {
    path: 'configuracoes/detalhes-usuario/:userId/envio_arquivo', component: FileUploadComponent,
    data: { page: 'settings-user-file-upload' }, canActivate: [ AuthGuard]
  }, {
    path: 'configuracoes/processos-de-sincronia', component: SettingsUsersSyncComponent,
    data: { page: 'settings-users-sync' }, canActivate: [ AuthGuard, RoleGuard ]
  }, {
    path: 'configuracoes/processos-de-sincronia/:syncId', component: SettingsUsersSyncDetailsComponent,
    data: { page: 'settings-users-sync-details' }, canActivate: [ AuthGuard, RoleGuard ]
  }, {
    path: 'configuracoes/gerenciar-eventos/:eventId', component: SettingsEventDetailsComponent,
    data: { page: 'settings-manage-events' }, canActivate: [ AuthGuard ]
  }, {
    path: 'configuracoes/gerenciar-eventos/:eventId/rascunho', component: SettingsEventDetailsComponent,
    data: { page: 'settings-manage-events-draft' }, canActivate: [ AuthGuard ]
  }, {
    path: 'configuracoes/gerenciar-eventos/:eventId/resultados/:scheduleId', component: SettingsEventResultsComponent,
    data: { page: 'settings-manage-events-results' }, canActivate: [ AuthGuard, RoleGuard ]
  }, {
    path: 'configuracoes/gerenciar-inscricoes/:eventId/:scheduleId', component: SettingsEventsApplicationsComponent,
    data: { page: 'settings-events-manage-subscriptions' }, canActivate: [ AuthGuard ]
  }, {
    path: 'configuracoes/gerenciar-inscricoes-notas/:eventId/:scheduleId', component: SettingsEventsApplicationsGradesComponent,
    data: { page: 'settings-events-manage-subscriptions' }, canActivate: [ AuthGuard ]
  }, {
    path: 'configuracoes/gerenciar-equipe', component: SettingsManageUserRegisterComponent,
    data: { page: 'settings-manage-team' }, canActivate: [ AuthGuard ]
  }, {
    path: 'configuracoes/gerenciar-usuario', component: SettingsManageTeamComponent,
    data: { page: 'settings-manage-team' }, canActivate: [ AuthGuard ]
  }, {
    path: 'configuracoes/buscar-talentos', component: RecruitmentManageTalentsComponent,
    data: { page: 'settings-manage-talents' }, canActivate: [ AuthGuard ]
  }, {
    path: 'configuracoes/enturmacao-matricula', component: SettingsRecommendTrackComponent,
    data: { page: 'settings-recommend-track' }, canActivate: [ AuthGuard ]
  }, {
    path: 'configuracoes/enviar-email', component: SettingsCustomEmailComponent,
    data: { page: 'settings-custom-email' }, canActivate: [ AuthGuard ]
  }, {
    path: 'configuracoes/emails-enviados', component: SettingsCustomEmailSentComponent,
    data: { page: 'settings-custom-email-sent' }, canActivate: [ AuthGuard ]
  }, {
    path: 'configuracoes/pesquisa-na-base', component: ProfileTestsComponent,
    data: { page: 'settings-profile-tests' }, canActivate: [ AuthGuard, RoleGuard ]
  }, {
    path: 'configuracoes/pesquisa-na-base-nps', component: ProfileTestsComponent,
    data: { page: 'settings-profile-tests-nps' }, canActivate: [ AuthGuard, RoleGuard ]
  }, {
    path: 'configuracoes/pesquisa-na-base/:testId', component: SettingsManageTestComponent,
    data: { page: 'settings-profile-test' }, canActivate: [ AuthGuard, RoleGuard ]
  }, {
    path: 'configuracoes/resultado-pesquisa-na-base', component: ProfileTestsResultsComponent,
    data: { page: 'settings-profile-tests-results' }, canActivate: [ AuthGuard, RoleGuard ]
  }, {
    path: 'configuracoes/resultado-pesquisa-na-base-nps', component: ProfileTestsResultsComponent,
    data: { page: 'settings-profile-tests-nps-results' }, canActivate: [ AuthGuard, RoleGuard ]
  }, {
    path: 'configuracoes/recomendacoes-produtos', component: ProductSuggestionComponent,
    data: { page: 'settings-product-suggestion' }, canActivate: [ AuthGuard, RoleGuard ]
  }, {
    path: 'configuracoes/recomendacoes-produtos/:responseId', component: ManageSuggestionComponent,
    data: { page: 'settings-manage-suggestion' }, canActivate: [ AuthGuard, RoleGuard ]
  }, {
    path: 'configuracoes/indicar-teste', component: SettingsSuggestTestComponent,
    data: { page: 'settings-suggest-test' }, canActivate: [ AuthGuard, RoleGuard ]
  }, {
    path: 'configuracoes/formulas', component: FormulasComponent,
    data: { page: 'formulas' }, canActivate: [ AuthGuard, RoleGuard ]
  }, {
    path: 'configuracoes/formulas/:formulaId', component: ManageFormulaComponent,
    data: { page: 'manage-formula' }, canActivate: [ AuthGuard, RoleGuard ]
  }, {
    path: 'configuracoes/identificacao-empresa', component: RecruitingCompanyFormComponent,
    data: { page: 'company-form' }, canActivate: [ AuthGuard ]
  }, {
    path: 'configuracoes/logs', component: LogsComponent,
    data: { page: 'company-form' }, canActivate: [ AuthGuard ]
  }, {
    path: 'configuracoes/vagas-empresa', component: ManageJobsCompanyFormComponent,
    data: { page: 'company-jobs' }, canActivate: [ AuthGuard ]
  }, {
    path: 'configuracoes/gerenciar-inscricoes-vagas/:jobPositionId', component: ManageJobApplicationsComponent,
    data: { page: 'recruitment-job-applications' }, canActivate: [ AuthGuard ]
  }, {
    path: 'configuracoes/cadastro-vaga/:jobPositionId', component: EmploymentComponent,
    data: { page: 'recruitment-job-edit' }, canActivate: [ AuthGuard ]
  }, {
    path: 'configuracoes/gerenciar-vagas-inscricao/:jobPositionId', component: RecJobsComponent,
    data: { page: 'settings-users-info' }, canActivate: [ AuthGuard ]
  }, {
    path: 'minha-candidatura', component: CandidacyStudentsComponent,
    data: { page: 'settings-users-info' }, canActivate: [ AuthGuard ]
  }, {
    path: 'buscar-vagas-aluno', component: StudentSearchJobComponent,
    data: { page: 'settings-users-info' }, canActivate: [ AuthGuard ]
  }, {
    path: 'gerenciar-vaga-alunos/:jobPositionId', component: StudentJobsComponent,
    data: { page: 'settings-users-info' }, canActivate: [ AuthGuard ]
  }, {
    path: 'performance', component: SettingsUserPerformaceComponent,
    data: { page: 'settings-users-performace' }, canActivate: [ AuthGuard ]
  }, {
    path: 'home/calendario', component: SettingsUserCalendarComponent,
    data: { page: 'settings-users-info' }, canActivate: [ AuthGuard ]
  }, {
    path: 'home/modulo/:moduleId', component: ModuleInfoComponent,
    data: { page: 'settings-users-info' }, canActivate: [ AuthGuard ]
  }, {
    path: 'home/color', component: ColorEditComponent,
    data: { page: 'settings-users-info' }, canActivate: [ AuthGuard ]
  }, {
    path: 'configuracoes/testes-de-avaliacao', component: ValuationTestsComponent,
    data: { page: 'settings-valuation-tests' }, canActivate: [ AuthGuard, RoleGuard ]
  }, {
    path: 'configuracoes/teste-de-avaliacao', component: SettingsManageValuationTestComponent,
    data: { page: 'settings-valuation-test' }, canActivate: [ AuthGuard, RoleGuard ]
  }, {
    path: 'configuracoes/teste-de-avaliacao/liberacao', component: SettingsManageValuationTestReleaseComponent,
    data: { page: 'settings-valuation-test' }, canActivate: [ AuthGuard, RoleGuard ]
  }, {
    path: 'configuracoes/teste-de-avaliacao/repostas/:testId', component: SettingsValuationTestComponent,
    data: { page: 'settings-product-suggestion' }, canActivate: [ AuthGuard, RoleGuard ]
  }, {
    path: 'configuracoes/teste-de-avaliacao/repostas/:testId/:responseId', component: SettingsValuationTestGradeComponent,
    data: { page: 'settings-manage-suggestion' }, canActivate: [ AuthGuard, RoleGuard ]
  }, {
    path: 'configuracoes/testes-de-avaliacao/:testId', component: SettingsManageValuationTestComponent,
    data: { page: 'settings-valuation-test' }, canActivate: [ AuthGuard, RoleGuard ]
  }, {
    path: 'configuracoes/testes-de-avaliacao/repostas/:testId', component: SettingsValuationTestComponent,
    data: { page: 'settings-product-suggestion' }, canActivate: [ AuthGuard, RoleGuard ]
  }, {
    path: 'configuracoes/testes-de-avaliacao/repostas/:testId/:responseId', component: SettingsValuationTestGradeComponent,
    data: { page: 'settings-manage-suggestion' }, canActivate: [ AuthGuard, RoleGuard ]
  }, {
    path: 'teste-de-avaliacao/:testId', component: ValuationTestComponent,
    data: { page: 'profile-test' }, canActivate: [ AuthGuard ]
  }, {
    path: 'configuracoes/resultado-testes-de-avaliacao', component: ValuationTestsResultsComponent,
    data: { page: 'settings-valuation-tests-results' }, canActivate: [ AuthGuard, RoleGuard ]
  }, {
    path: 'relatorios', component: ReportComponent,
    data: { page: 'report' }, canActivate: [ AuthGuard, RoleGuard ]
  }, {
    path: 'configuracoes/nps', component: NpsComponent,
    data: { page: 'settings-nps' }, canActivate: [ AuthGuard, RoleGuard ]
  }, {
    path: 'meus-cursos', component: MyCoursesComponent,
    data: { page: 'settings-manage-suggestion' }, canActivate: [ AuthGuard ]
  }, {
    path: 'catalogo-de-cursos', component: EcommerceComponent,
    data: { page: 'settings-manage-suggestion' }, canActivate: [ AuthGuard ]
  }, {
    path: 'mapa-de-competencias', component: CompetenceMapComponent,
    data: { page: 'settings-manage-suggestion' }, canActivate: [ AuthGuard ]
  }, {
    path: 'desempenho-da-capacitacao', component: TrainningPerformanceComponent,
    data: { page: 'settings-manage-suggestion' }, canActivate: [ AuthGuard ]
  }, {
    path: 'historico-de-performance-profissional', component: ProfessionalPerformanceHistoryComponent,
    data: { page: 'settings-manage-suggestion' }, canActivate: [ AuthGuard ]
  }, {
    path: 'empenho-desempenho', component: EffortPerformanceComponent,
    data: { page: 'effort-performance' }, canActivate: [ AuthGuard ]
  }, {
    path: 'banco-questoes-tags', component: DataQuestionsTagsComponent,
    data: { page: 'effort-performance' }, canActivate: [ AuthGuard ]
  }, {
    path: '**', redirectTo: ''
  }
];

@NgModule({
  imports: [
    RouterModule.forRoot(appRoutes)
  ],
  exports: [
    RouterModule
  ],
  providers: [
    AuthGuard,
    RoleGuard
  ]
})
export class AppRoutingModule { }
