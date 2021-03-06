import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { PaginationModule } from '../../shared/components/pagination/pagination.module';
import { MaterialComponentsModule } from '../../shared/material.module';
import { ListSearchModule } from '../../shared/components/list-search/list-search.module';
import { ManageJobApplicationsComponent } from './job-applications.component';
import { NgUtilModule } from '@tg4/ng-util';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { ConceptsRegisterModule } from 'src/app/shared/components/concepts-register/concepts-register.module';
import { RequirementsConfigModule } from 'src/app/shared/components/requirements-config/requirements-config.module';
import { ImageCropModule } from 'src/app/shared/dialogs/image-crop/image-crop.module';
import { NgxMaskModule } from 'ngx-mask';
import { ConfirmDialogModule } from 'src/app/shared/dialogs/confirm/confirm.dialog.module';
import { PipesModule } from 'src/app/shared/pipes/pipes.module';
import { ConceptsSelectModule } from 'src/app/shared/components/conceps-select/concepts-select.module';
import { JobConfirmedComponent } from './job-confirmed/job-confirmed.component';
import { JobPendentsComponent } from './job-pendents/job-pendents.component';
import { MatBadgeModule } from '@angular/material';

@NgModule({
  declarations: [
    ManageJobApplicationsComponent,
    JobConfirmedComponent,
    JobPendentsComponent
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
export class ManageJobApplicationsModule { }
