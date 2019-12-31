import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { BrowserModule } from '@angular/platform-browser';
import { MaterialComponentsModule } from 'src/app/shared/material.module';
import { ProgressBarModule } from 'src/app/shared/components/layout/progress-bar/progress-bar.module';
import { ClassroomLessonHeaderComponent } from './lesson-header.component';
import { SubscriptionDialogComponent } from '../subscription-dialog/subscription-dialog.component';

@NgModule({
  declarations: [
    ClassroomLessonHeaderComponent,
    SubscriptionDialogComponent
  ],
  imports: [
    BrowserModule,
    MaterialComponentsModule,
    ProgressBarModule,
    RouterModule
  ],
  entryComponents: [
    SubscriptionDialogComponent
  ],
  exports: [
    ClassroomLessonHeaderComponent
  ]
})
export class ClassroomLessonHeaderModule { }
