import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { ContentForumService } from '../_services/forum.service';
import { ForumComponent } from './forum.component';
import { ForumQuestionComponent } from './forum-question/forum-question.component';
import { ForumLastQuestionCardComponent } from './last-question-card/last-question-card.component';
import { PaginationModule } from 'src/app/shared/components/pagination/pagination.module';
import { ListSearchModule } from 'src/app/shared/components/list-search/list-search.module';
import { ForumQuestionCardModule } from './question-card/question-card.module';
import { ForumQuestionAnswerCardComponent } from './forum-question/answer-card/answer-card.component';
import { MarkdownToHtmlModule } from 'markdown-to-html-pipe';

@NgModule({
  declarations: [
    ForumComponent,
    ForumLastQuestionCardComponent,
    ForumQuestionComponent,
    ForumQuestionAnswerCardComponent
  ],
  imports: [
    BrowserModule,
    FormsModule,
    RouterModule,
    PaginationModule,
    ListSearchModule,
    ForumQuestionCardModule,
    MarkdownToHtmlModule
  ],
  providers: [
    ContentForumService
  ]
})
export class ForumModule { }
