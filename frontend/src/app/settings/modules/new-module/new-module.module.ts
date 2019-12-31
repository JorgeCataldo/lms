import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { NgUtilModule } from '@tg4/ng-util';
import { NewModuleModuleInfoComponent } from './steps/1_module-info/module-info.component';
import { SettingsNewModuleComponent } from './new-module.component';
import { MaterialComponentsModule } from '../../../shared/material.module';
import { NewModuleVideoComponent } from './steps/2_video/video.component';
import { NewModuleSupportMaterialsComponent } from './steps/3_support-materials/support-materials.component';
import { NewModuleRequirementsComponent } from './steps/4_requirements/requirements.component';
import { NewModuleSubjectsComponent } from './steps/5_subjects/subjects.component';
import { NewModuleContentsComponent } from './steps/6_contents/contents.component';
import { VideoConfigDialogComponent } from './steps/6_contents/content-config/video-config/video-config.dialog';
import { TextConfigDialogComponent } from './steps/6_contents/content-config/text-config/text-config.dialog';
import { PdfConfigDialogComponent } from './steps/6_contents/content-config/pdf-config/pdf-config.dialog';
import { NewModuleQuestionsComponent } from './steps/7_questions/questions.component';
import { NewQuestionDialogComponent } from './steps/7_questions/new-question/new-question.dialog';
import { ConceptsRegisterModule } from '../../../shared/components/concepts-register/concepts-register.module';
import { CreatedModuleDialogComponent } from './steps/8_created/created-module.dialog';
import { RequirementsConfigModule } from '../../../shared/components/requirements-config/requirements-config.module';
import { ImageCropModule } from '../../../shared/dialogs/image-crop/image-crop.module';
import { SettingsModulesService } from '../../_services/modules.service';
import { SharedService } from '../../../shared/services/shared.service';
import { UploadService } from '../../../shared/services/upload.service';
import { UtilService } from '../../../shared/services/util.service';
import { NgxMaskModule } from 'ngx-mask';
import { ConfirmDialogModule } from '../../../shared/dialogs/confirm/confirm.dialog.module';
import { PaginationModule } from '../../../shared/components/pagination/pagination.module';
import { PipesModule } from '../../../shared/pipes/pipes.module';
import { ConceptsSelectModule } from '../../../shared/components/conceps-select/concepts-select.module';
import { UploadQuestionDatabaseDialogComponent } from './steps/7_questions/upload-qdb/upload-qdb.dialog';
import { ListSearchModule } from 'src/app/shared/components/list-search/list-search.module';
import { MissingLevelsDialogComponent } from './steps/7_questions/missing-levels/missing-levels.dialog';
import { SettingsModulesDraftsService } from '../../_services/modules-drafts.service';
import { ZipConfigDialogComponent } from './steps/6_contents/content-config/zip-config/zip-config.dialog';
import { ModuleEcommerceComponent } from './steps/9_ecommerce/ecommerce.component';
import { CurrencyMaskModule } from 'ng2-currency-mask';
import { NewModulesWeightComponent } from './steps/4.5_weight/modules-weight.component';

@NgModule({
  declarations: [
    SettingsNewModuleComponent,
    NewModuleModuleInfoComponent,
    NewModuleVideoComponent,
    NewModuleSupportMaterialsComponent,
    NewModuleRequirementsComponent,
    NewModuleSubjectsComponent,
    NewModuleContentsComponent,
    VideoConfigDialogComponent,
    ZipConfigDialogComponent,
    PdfConfigDialogComponent,
    TextConfigDialogComponent,
    ZipConfigDialogComponent,
    NewModuleQuestionsComponent,
    NewQuestionDialogComponent,
    CreatedModuleDialogComponent,
    UploadQuestionDatabaseDialogComponent,
    MissingLevelsDialogComponent,
    ModuleEcommerceComponent,
    NewModulesWeightComponent
  ],
  imports: [
    BrowserModule,
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
    ListSearchModule,
    CurrencyMaskModule
  ],
  entryComponents: [
    VideoConfigDialogComponent,
    PdfConfigDialogComponent,
    TextConfigDialogComponent,
    ZipConfigDialogComponent,
    NewQuestionDialogComponent,
    CreatedModuleDialogComponent,
    UploadQuestionDatabaseDialogComponent,
    MissingLevelsDialogComponent
  ],
  providers: [
    SettingsModulesService,
    SettingsModulesDraftsService,
    SharedService,
    UtilService,
    UploadService
  ]
})
export class NewModuleModule { }
