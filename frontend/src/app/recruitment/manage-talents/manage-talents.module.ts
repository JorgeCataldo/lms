import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { RouterModule } from '@angular/router';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';

import { RecruitingCompanyService } from '../_services/recruiting-company.service';
import { MaterialComponentsModule } from 'src/app/shared/material.module';
import { RecruitmentManageTalentsComponent } from './manage-talents.component';
import { ListSearchModule } from 'src/app/shared/components/list-search/list-search.module';
import { PaginationModule } from 'src/app/shared/components/pagination/pagination.module';
import { ManageTeamFiltersModule } from 'src/app/settings/manage-team/filters/filters.module';

@NgModule({
  declarations: [
    RecruitmentManageTalentsComponent
  ],
  imports: [
    BrowserModule,
    MaterialComponentsModule,
    RouterModule,
    FormsModule,
    ReactiveFormsModule,
    ListSearchModule,
    PaginationModule,
    ManageTeamFiltersModule
  ],
  providers: [
    RecruitingCompanyService
  ]
})
export class ManageTalentsModule { }
