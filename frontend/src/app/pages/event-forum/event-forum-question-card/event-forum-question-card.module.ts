import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { EventForumQuestionCardComponent } from './event-forum-question-card.component';
import { MarkdownToHtmlModule } from 'markdown-to-html-pipe';

@NgModule({
  declarations: [
    EventForumQuestionCardComponent
  ],
  imports: [
    BrowserModule,
    MarkdownToHtmlModule
  ],
  exports: [
    EventForumQuestionCardComponent
  ]
})
export class EventForumQuestionCardModule { }
