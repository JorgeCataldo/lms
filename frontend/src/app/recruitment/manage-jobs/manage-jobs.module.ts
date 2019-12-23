import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { RouterModule } from '@angular/router';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';

import { RecruitingCompanyService } from '../_services/recruiting-company.service';
import { MaterialComponentsModule } from 'src/app/shared/material.module';
import { NgxMaskModule } from 'ngx-mask';
import { ValidationsService } from 'src/app/shared/services/validation.service';
import { ManageJobsCompanyFormComponent } from './manage-jobs.component';
import { EmploymentModule } from './employment/employment.module';
import { TrackOverviewTopBottomTalentsComponent } from './top-botton-talents/top-bottom-talents.component';
import { TrackOverviewTopBottomTalentsDialogComponent } from './top-botton-talents-dialog/top-bottom-talents.dialog';



@NgModule({
  declarations: [
    ManageJobsCompanyFormComponent,
    TrackOverviewTopBottomTalentsComponent,
    TrackOverviewTopBottomTalentsDialogComponent
  ],
  imports: [
    BrowserModule,
    MaterialComponentsModule,
    RouterModule,
    FormsModule,
    ReactiveFormsModule,
    NgxMaskModule,
    EmploymentModule,
  ],
  providers: [
    RecruitingCompanyService,
    ValidationsService
  ]
})
export class ManageJobsModule { }
