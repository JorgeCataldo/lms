import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { NgUtilModule } from '@tg4/ng-util';
import { MaterialComponentsModule } from '../../../shared/material.module';
import { SettingsNewEventComponent } from './new-event.component';
import { NewEventEventInfoComponent } from './steps/1_event-info/event-info.component';
import { NewEventDateComponent } from './steps/2_date/date.component';
import { ExternalService } from '../../../shared/services/external.service';
import { NewEventVideoComponent } from './steps/3_video/video.component';
import { HttpClientModule } from '@angular/common/http';
import { PipesModule } from '../../../shared/pipes/pipes.module';
import { UtilService } from '../../../shared/services/util.service';
import { NgxMaskModule } from 'ngx-mask';
import { NewEventSupportMaterialsComponent } from './steps/4_support-materials/support-materials.component';
import { RequirementsConfigModule } from '../../../shared/components/requirements-config/requirements-config.module';
import { NewEventRequirementsComponent } from './steps/5_requirements/requirements.component';
import { NewEventQuestionsComponent } from './steps/6_questions/questions.component';
import { CreatedEventDialogComponent } from './steps/7_created-event/created-event.dialog';
import { ImageCropModule } from '../../../shared/dialogs/image-crop/image-crop.module';
import { ListSearchModule } from 'src/app/shared/components/list-search/list-search.module';

@NgModule({
  declarations: [
    SettingsNewEventComponent,
    NewEventEventInfoComponent,
    NewEventDateComponent,
    NewEventVideoComponent,
    NewEventSupportMaterialsComponent,
    NewEventRequirementsComponent,
    NewEventQuestionsComponent,
    CreatedEventDialogComponent
  ],
  imports: [
    BrowserModule,
    MaterialComponentsModule,
    FormsModule,
    ReactiveFormsModule,
    NgUtilModule,
    HttpClientModule,
    PipesModule,
    NgxMaskModule,
    RequirementsConfigModule,
    ImageCropModule,
    ListSearchModule
  ],
  entryComponents: [
    CreatedEventDialogComponent
  ],
  providers: [
    ExternalService,
    UtilService
  ]
})
export class SettingsNewEventModule { }
