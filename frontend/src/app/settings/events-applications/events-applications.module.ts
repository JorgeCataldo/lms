import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { RouterModule } from '@angular/router';
import { MaterialComponentsModule } from '../../shared/material.module';
import { SettingsEventsApplicationsComponent } from './events-applications.component';
import { SettingsApplicationsTableComponent } from './applications-table/applications-table.component';
import { FormsModule } from '@angular/forms';
import { ProgressBarModule } from '../../shared/components/layout/progress-bar/progress-bar.module';
import { SettingsApplicationsHeaderComponent } from './applications-header/applications-header.component';
import { AnswersDialogComponent } from './applications-table/answers-dialog/answers.dialog';

@NgModule({
  declarations: [
    SettingsEventsApplicationsComponent,
    SettingsApplicationsHeaderComponent,
    SettingsApplicationsTableComponent,
    AnswersDialogComponent
  ],
  imports: [
    BrowserModule,
    FormsModule,
    RouterModule,
    MaterialComponentsModule,
    ProgressBarModule
  ],
  entryComponents: [
    AnswersDialogComponent
  ]
})
export class SettingsEventsApplicationsModule { }
