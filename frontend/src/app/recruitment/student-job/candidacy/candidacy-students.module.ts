import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { RouterModule } from '@angular/router';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';

import { RecruitingCompanyService } from '../../_services/recruiting-company.service';
import { MaterialComponentsModule } from 'src/app/shared/material.module';
import { NgxMaskModule } from 'ngx-mask';
import { ValidationsService } from 'src/app/shared/services/validation.service';
import { CandidacyStudentsComponent } from './candidacy-students.component';
import { NotificationsComponent } from './notifications/notifications.component';
import { NotificationsDialogComponent } from './notifications-dialog/notifications.dialog';
import { UsersTabsModule } from './user-tabs/users-tabs.module';




@NgModule({
  declarations: [
    CandidacyStudentsComponent,
    NotificationsComponent,
    NotificationsDialogComponent
  ],
  imports: [
    BrowserModule,
    MaterialComponentsModule,
    RouterModule,
    FormsModule,
    ReactiveFormsModule,
    NgxMaskModule,
    UsersTabsModule
  ],
  entryComponents: [
    NotificationsDialogComponent
  ],
  providers: [
    RecruitingCompanyService,
    ValidationsService
  ]
})
export class CandidacyStudentsModule { }
