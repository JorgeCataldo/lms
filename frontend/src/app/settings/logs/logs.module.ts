import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { NgUtilModule } from '@tg4/ng-util';
import { RouterModule } from '@angular/router';
import { PaginationModule } from '../../shared/components/pagination/pagination.module';
import { MaterialComponentsModule } from '../../shared/material.module';
import { SettingsUsersService } from '../_services/users.service';
import { UsersTabsModule } from '../../shared/components/users-tabs/users-tabs.module';
import { ListSearchModule } from '../../shared/components/list-search/list-search.module';
import { DateRangePickerModule } from '../../shared/components/date-range-picker/date-range.picker.module';
import { ClickOutsideModule } from 'ng-click-outside';
import { NgxMaskModule } from 'ngx-mask';
import { RecruitingCompanyService } from 'src/app/recruitment/_services/recruiting-company.service';
import { LogsComponent } from './logs.component';
import { LogsService } from '../_services/logs.service';


@NgModule({
  declarations: [
    LogsComponent
  ],
  imports: [
    BrowserModule,
    MaterialComponentsModule,
    PaginationModule,
    DateRangePickerModule
  ],
  providers: [
    LogsService
  ]
})
export class LogsModule { }
