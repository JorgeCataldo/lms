import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { NgUtilModule } from '@tg4/ng-util';
import { RouterModule } from '@angular/router';
import { PaginationModule } from '../../shared/components/pagination/pagination.module';
import { SettingsUsersComponent } from './users.component';
import { MaterialComponentsModule } from '../../shared/material.module';
import { SettingsUsersService } from '../_services/users.service';
import { UsersTabsModule } from '../../shared/components/users-tabs/users-tabs.module';
import { ListSearchModule } from '../../shared/components/list-search/list-search.module';
import { DateRangePickerModule } from '../../shared/components/date-range-picker/date-range.picker.module';
import { SettingsManageUserComponent } from './manage-user/manage-user.component';
import { SettingsManageUserPasswordComponent } from './manage-user-password/manage-user-password.component';
import { SettingsUserDetailsModule } from './user-details/user-details.module';
import { FileUploadComponent } from './user-archive-upload/file-upload.component';
import { SettingsManageUserCareerComponent } from './manage-user-career/manage-user-career.component';
import { ProfessionalExperienceComponent } from './manage-user-career/professional-experience/professional-experience.component';
import { AcademicEducationComponent } from './manage-user-career/academic-education/academic-education.component';
import { CareerComplementaryInfoComponent } from './manage-user-career/complementary-info/complementary-info.component';
import { CareerComplementaryExperienceComponent } from './manage-user-career/complementary-experience/complementary-experience.component';
import { ClickOutsideModule } from 'ng-click-outside';
import { ProfessionalObjectivesComponent } from './manage-user-career/professional-objectives/professional-objectives.component';
import { SettingsUserRecommendationModule } from './user-recommendation/user-recommendation.module';
import { NgxMaskModule } from 'ngx-mask';
import { RecruitingCompanyService } from 'src/app/recruitment/_services/recruiting-company.service';


@NgModule({
  declarations: [
    SettingsUsersComponent,
    SettingsManageUserPasswordComponent,
    SettingsManageUserCareerComponent,
    ProfessionalExperienceComponent,
    ProfessionalObjectivesComponent,
    AcademicEducationComponent,
    CareerComplementaryExperienceComponent,
    CareerComplementaryInfoComponent,
    FileUploadComponent
  ],
  imports: [
    BrowserModule,
    MaterialComponentsModule,
    FormsModule,
    ReactiveFormsModule,
    NgUtilModule,
    RouterModule,
    PaginationModule,
    UsersTabsModule,
    ListSearchModule,
    DateRangePickerModule,
    SettingsUserDetailsModule,
    SettingsUserRecommendationModule,
    ClickOutsideModule,
    NgxMaskModule
  ],
  providers: [
    SettingsUsersService,
    RecruitingCompanyService
  ]
})
export class SettingsUsersModule { }
