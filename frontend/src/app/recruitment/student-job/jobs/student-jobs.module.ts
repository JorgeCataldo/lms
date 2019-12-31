import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { PaginationModule } from '../../../shared/components/pagination/pagination.module';
import { MaterialComponentsModule } from '../../../shared/material.module';
import { ListSearchModule } from '../../../shared/components/list-search/list-search.module';
import { NgUtilModule } from '@tg4/ng-util';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { ConceptsRegisterModule } from 'src/app/shared/components/concepts-register/concepts-register.module';
import { RequirementsConfigModule } from 'src/app/shared/components/requirements-config/requirements-config.module';
import { ImageCropModule } from 'src/app/shared/dialogs/image-crop/image-crop.module';
import { NgxMaskModule } from 'ngx-mask';
import { ConfirmDialogModule } from 'src/app/shared/dialogs/confirm/confirm.dialog.module';
import { PipesModule } from 'src/app/shared/pipes/pipes.module';
import { ConceptsSelectModule } from 'src/app/shared/components/conceps-select/concepts-select.module';
import { MatBadgeModule } from '@angular/material';
import { JobActivitiesComponent } from './job-activities/job-activities.component';
import { JobPreRequisitesComponent } from './job-pre-requisites/job-pre-requisites.component';
import { JobRemunerationComponent } from './job-remuneration/job-remuneration.component';
import { JobValuesComponent } from './job-values/job-values.component';
import { StudentJobsComponent } from './student-jobs.component';

@NgModule({
  declarations: [
    JobActivitiesComponent,
    JobPreRequisitesComponent,
    JobRemunerationComponent,
    JobValuesComponent,
    StudentJobsComponent
  ],

  imports: [
    BrowserModule,
    MatBadgeModule,
    MaterialComponentsModule,
    FormsModule,
    ReactiveFormsModule,
    NgUtilModule,
    ConceptsRegisterModule,
    RequirementsConfigModule,
    ImageCropModule,
    NgxMaskModule,
    ConfirmDialogModule,
    PaginationModule,
    PipesModule,
    ConceptsSelectModule,
    ListSearchModule
  ]
})
export class StudentJobsModule { }
