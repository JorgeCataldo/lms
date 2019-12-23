import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { ContentEventForumService } from '../_services/event-forum.service';
import { EventForumComponent } from './event-forum.component';
import { EventForumQuestionComponent } from './event-forum-question/event-forum-question.component';
import { EventForumLastQuestionCardComponent } from './event-forum-last-question-card/event-forum-last-question-card.component';
import { PaginationModule } from 'src/app/shared/components/pagination/pagination.module';
import { ListSearchModule } from 'src/app/shared/components/list-search/list-search.module';
import { EventForumQuestionCardModule } from './event-forum-question-card/event-forum-question-card.module';
import { EventForumQuestionAnswerCardComponent } from './event-forum-question/event-forum-answer-card/event-forum-answer-card.component';
import { MarkdownToHtmlModule } from 'markdown-to-html-pipe';

@NgModule({
  declarations: [
    EventForumComponent,
    EventForumLastQuestionCardComponent,
    EventForumQuestionComponent,
    EventForumQuestionAnswerCardComponent
  ],
  imports: [
    BrowserModule,
    FormsModule,
    RouterModule,
    PaginationModule,
    ListSearchModule,
    EventForumQuestionCardModule,
    MarkdownToHtmlModule
  ],
  providers: [
    ContentEventForumService
  ]
})
export class EventForumModule { }
