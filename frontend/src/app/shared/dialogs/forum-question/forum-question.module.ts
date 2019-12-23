import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { MaterialComponentsModule } from '../../material.module';
import { ForumQuestionDialogComponent } from './forum-question.dialog';
import { ForumQuestionCardModule } from 'src/app/pages/forum/question-card/question-card.module';
import { FormsModule } from '@angular/forms';

@NgModule({
  declarations: [
    ForumQuestionDialogComponent
  ],
  imports: [
    BrowserModule,
    MaterialComponentsModule,
    ForumQuestionCardModule,
    FormsModule
  ],
  entryComponents: [
    ForumQuestionDialogComponent
  ]
})
export class ForumQuestionDialogModule { }
