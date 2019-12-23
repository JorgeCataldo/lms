import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { ExamComponent } from './exam.component';
import { ExamStartComponent } from './exam-start/exam-start.component';
import { ExamQuestionComponent } from './exam-question/exam-question.component';
import { FormsModule } from '@angular/forms';
import { ExamFooterComponent } from './exam-footer/exam-footer.component';
import { ProgressBarModule } from '../../shared/components/layout/progress-bar/progress-bar.module';
import { ExamFinishComponent } from './exam-finish/exam-finish.component';
import { ExamReviewComponent } from './exam-review/exam-review.component';
import { ContentModule } from '../content/content.module';
import { BadgesProgressModule } from '../../shared/components/layout/badges-progress/badges.progress.module';
import { ExamAnswerComponent } from './exam-question/answer/answer.component';
import { ExamReviewContentListComponent } from './exam-review/content-list/content-list.component';
import { ContentExamService } from '../_services/exam.service';
import { RouterModule } from '@angular/router';
import { AnalyticsService } from 'src/app/shared/services/analytics.service';
import { MarkdownToHtmlModule } from 'markdown-to-html-pipe';

@NgModule({
  declarations: [
    ExamComponent,
    ExamStartComponent,
    ExamQuestionComponent,
    ExamAnswerComponent,
    ExamFooterComponent,
    ExamFinishComponent,
    ExamReviewComponent,
    ExamReviewContentListComponent
  ],
  imports: [
    BrowserModule,
    FormsModule,
    ProgressBarModule,
    ContentModule,
    BadgesProgressModule,
    RouterModule,
    MarkdownToHtmlModule
  ],
  providers: [
    ContentExamService,
    AnalyticsService
  ]
})
export class ExamModule { }
