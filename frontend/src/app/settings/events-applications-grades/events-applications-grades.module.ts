import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { NgUtilModule } from '@tg4/ng-util';
import { RouterModule } from '@angular/router';
import { PaginationModule } from '../../shared/components/pagination/pagination.module';
import { MaterialComponentsModule } from '../../shared/material.module';
import { UsersTabsModule } from '../../shared/components/users-tabs/users-tabs.module';
import { ListSearchModule } from '../../shared/components/list-search/list-search.module';
import { DateRangePickerModule } from '../../shared/components/date-range-picker/date-range.picker.module';
import { SettingsEventsApplicationsGradesComponent } from './events-applications-grades.component';
import { NgxMaskModule } from 'ngx-mask';
import { EventForumParticipationDialogModule } from 'src/app/shared/dialogs/event-forum-participation/event-forum-participation.module';
import { EventsChangeDialogComponent } from './events-chage-date-dialog/events-change-dialog.component';
import { EventApplicationNoteDialogComponent } from './event-application-note/event-application-note.dialog';

@NgModule({
  declarations: [
    SettingsEventsApplicationsGradesComponent,
    EventsChangeDialogComponent,
    EventApplicationNoteDialogComponent
  ],
  entryComponents: [
    EventsChangeDialogComponent,
    EventApplicationNoteDialogComponent
  ],
  imports: [
    BrowserModule,
    MaterialComponentsModule,
    FormsModule,
    ReactiveFormsModule,
    EventForumParticipationDialogModule,
    NgUtilModule,
    RouterModule,
    PaginationModule,
    UsersTabsModule,
    ListSearchModule,
    DateRangePickerModule,
    NgxMaskModule.forRoot()
  ]
})
export class SettingsEventsApplicationsGradesModule { }
