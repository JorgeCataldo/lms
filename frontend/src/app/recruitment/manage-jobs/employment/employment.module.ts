import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { NgUtilModule } from '@tg4/ng-util';
import { RouterModule } from '@angular/router';
import { PaginationModule } from 'src/app/shared/components/pagination/pagination.module';
import { SettingsUsersComponent } from 'src/app/settings/users/users.component';
import { MaterialComponentsModule } from 'src/app/shared/material.module';
import { SettingsUsersService } from 'src/app/settings/_services/users.service';
import { UsersTabsModule } from 'src/app/shared/components/users-tabs/users-tabs.module';
import { ListSearchModule } from 'src/app/shared/components/list-search/list-search.module';
import { DateRangePickerModule } from 'src/app/shared/components/date-range-picker/date-range.picker.module';
import { SettingsUserDetailsModule } from 'src/app/settings/users/user-details/user-details.module';
import { ClickOutsideModule } from 'ng-click-outside';
import { SettingsUserRecommendationModule } from 'src/app/settings/users/user-recommendation/user-recommendation.module';
import { NgxMaskModule } from 'ngx-mask';
import { RecruitingCompanyService } from 'src/app/recruitment/_services/recruiting-company.service';


import { ActivitiesComponent } from './activities/activities.component';
import { PreRequisitesComponent } from './pre-requisites/pre-requisites.component';
import { ProfissionalValuesComponent } from './profissional-values/profissional-values.component';
import { RecordComponent } from './record/record.component';
import { RemunerationBenefitsComponent } from './remuneration-benefits/remuneration-benefits.component';
import { EmploymentComponent } from './employment.component';

@NgModule({
  declarations: [
    ActivitiesComponent,
    PreRequisitesComponent,
    ProfissionalValuesComponent,
    RecordComponent,
    RemunerationBenefitsComponent,
    EmploymentComponent
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
export class EmploymentModule { }
