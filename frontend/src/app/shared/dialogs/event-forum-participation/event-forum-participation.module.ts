import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { MaterialComponentsModule } from '../../material.module';
import { EventForumParticipationDialogComponent } from './event-forum-participation.dialog';
import { FormsModule } from '@angular/forms';
import { EventForumQuestionCardModule } from 'src/app/pages/event-forum/event-forum-question-card/event-forum-question-card.module';

@NgModule({
  declarations: [
    EventForumParticipationDialogComponent
  ],
  imports: [
    BrowserModule,
    MaterialComponentsModule,
    EventForumQuestionCardModule,
    FormsModule
  ],
  entryComponents: [
    EventForumParticipationDialogComponent
  ]
})
export class EventForumParticipationDialogModule { }
