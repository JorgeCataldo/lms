import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { RouterModule } from '@angular/router';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';

import { RecruitingCompanyFormComponent } from './company-form/company-form.component';
import { RecruitingCompanyService } from '../_services/recruiting-company.service';
import { MaterialComponentsModule } from 'src/app/shared/material.module';
import { NgxMaskModule } from 'ngx-mask';
import { ValidationsService } from 'src/app/shared/services/validation.service';

@NgModule({
  declarations: [
    RecruitingCompanyFormComponent
  ],
  imports: [
    BrowserModule,
    MaterialComponentsModule,
    RouterModule,
    FormsModule,
    ReactiveFormsModule,
    NgxMaskModule
  ],
  providers: [
    RecruitingCompanyService,
    ValidationsService
  ]
})
export class RecruitingCompanyModule { }
