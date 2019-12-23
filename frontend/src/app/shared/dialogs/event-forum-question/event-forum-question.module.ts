import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { MaterialComponentsModule } from '../../material.module';
import { EventForumQuestionDialogComponent } from './event-forum-question.dialog';
import { FormsModule } from '@angular/forms';
import { EventForumQuestionCardModule } from 'src/app/pages/event-forum/event-forum-question-card/event-forum-question-card.module';

@NgModule({
  declarations: [
    EventForumQuestionDialogComponent
  ],
  imports: [
    BrowserModule,
    MaterialComponentsModule,
    EventForumQuestionCardModule,
    FormsModule
  ],
  entryComponents: [
    EventForumQuestionDialogComponent
  ]
})
export class EventForumQuestionDialogModule { }
