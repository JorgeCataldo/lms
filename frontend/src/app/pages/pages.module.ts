import { NgModule } from '@angular/core';
import { HomeModule } from './home/home.module';
import { ModuleModule } from './module/module.module';
import { ClassroomLessonModule } from './classroom-lesson/classroom-lesson.module';
import { ContentModule } from './content/content.module';
import { SearchResultsModule } from './search-results/search-results.module';
import { ExamModule } from './exam/exam.module';
import { TrackModule } from './track/track.module';
import { LoginModule } from './login/login.module';
import { ForumModule } from './forum/forum.module';
import { MessagesModule } from './messages/messages.module';
import { ProfileTestModule } from './profile-test/profile-test.module';
import { RegisterModule } from './register/register.module';
import { ErrorModule } from './error/error.module';
import { EventForumModule } from './event-forum/event-forum.module';
import { ValuationTestModule } from './valuation-test/valuation-test.module';
import { ReportModule } from './report/report.module';
import { MyCoursesModule } from './my-courses/my-courses.module';
import { EcommerceModule } from './e-commerce/e-commerce.module';
import { TrainningPerformanceComponent } from './trainning-performance/trainning-performance.component';
import { ProfessionalPerformanceHistoryComponent } from './professional-performance-history/professional-performance-history.component';
import { CompetenceMapModule } from './competence-map/competence-map.module';
import { TrainningPerformanceModule } from './trainning-performance/trainning-performance.module';
import { ProfessionalPerformanceHistoryModule } from './professional-performance-history/professional-performance-history.module';
import { DataQuestionsTagsModule } from './data-questions-tags/data-questions-tags.module';
import { SettingsValuationTestsResultsModule } from './valuation-tests-results/valuation-tests-results.module';
import { SettingsProfileTestsResultsModule } from './profile-test-results/profile-tests-results.module';

@NgModule({
  imports: [
    HomeModule,
    ErrorModule,
    ModuleModule,
    ContentModule,
    ClassroomLessonModule,
    SearchResultsModule,
    ExamModule,
    TrackModule,
    RegisterModule,
    LoginModule,
    ForumModule,
    EventForumModule,
    MessagesModule,
    ProfileTestModule,
    ValuationTestModule,
    ReportModule,
    MyCoursesModule,
    EcommerceModule,
    CompetenceMapModule,
    ProfessionalPerformanceHistoryModule,
    TrainningPerformanceModule,
    DataQuestionsTagsModule,
    SettingsValuationTestsResultsModule,
    SettingsProfileTestsResultsModule
  ],
  declarations: []
})
export class PagesModule { }
