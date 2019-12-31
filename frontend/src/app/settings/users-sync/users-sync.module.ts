import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { RouterModule } from '@angular/router';
import { PaginationModule } from '../../shared/components/pagination/pagination.module';
import { MaterialComponentsModule } from '../../shared/material.module';
import { SettingsUsersService } from '../_services/users.service';
import { DateRangePickerModule } from '../../shared/components/date-range-picker/date-range.picker.module';
import { SettingsUsersSyncComponent } from './users-sync.component';
import { SettingsUsersSyncDetailsComponent } from './sync-details/sync-details.component';
import { StatusTagModule } from '../../shared/components/layout/status-tag/status-tag.component.module';
import { UsersTabsModule } from '../../shared/components/users-tabs/users-tabs.module';
import { ListSearchModule } from '../../shared/components/list-search/list-search.module';
import { UsersSyncErrorDialogComponent } from './users-sync-error-dialog/users-sync-error.dialog';

@NgModule({
  declarations: [
    SettingsUsersSyncComponent,
    SettingsUsersSyncDetailsComponent,
    UsersSyncErrorDialogComponent
  ],
  imports: [
    BrowserModule,
    MaterialComponentsModule,
    RouterModule,
    PaginationModule,
    DateRangePickerModule,
    ListSearchModule,
    UsersTabsModule,
    StatusTagModule
  ],
  providers: [
    SettingsUsersService
  ],
  entryComponents: [
    UsersSyncErrorDialogComponent
  ]
})
export class SettingsUsersSyncModule { }
