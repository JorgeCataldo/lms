import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { ForumQuestionCardComponent } from './question-card.component';
import { MarkdownToHtmlModule } from 'markdown-to-html-pipe';

@NgModule({
  declarations: [
    ForumQuestionCardComponent
  ],
  imports: [
    BrowserModule,
    MarkdownToHtmlModule
  ],
  exports: [
    ForumQuestionCardComponent
  ]
})
export class ForumQuestionCardModule { }
