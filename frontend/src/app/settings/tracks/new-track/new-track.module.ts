import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { NgUtilModule } from '@tg4/ng-util';
import { MaterialComponentsModule } from '../../../shared/material.module';
import { ImageCropModule } from '../../../shared/dialogs/image-crop/image-crop.module';
import { SharedService } from '../../../shared/services/shared.service';
import { UtilService } from '../../../shared/services/util.service';
import { NgxMaskModule } from 'ngx-mask';
import { ConfirmDialogModule } from '../../../shared/dialogs/confirm/confirm.dialog.module';
import { PaginationModule } from '../../../shared/components/pagination/pagination.module';
import { SettingsTracksService } from '../../_services/tracks.service';
import { SettingsNewTrackComponent } from './new-track.component';
import { NewTrackTrackInfoComponent } from './steps/1_track-info/track-info.component';
import { NewTrackModulesEventsComponent } from './steps/3_modules-events/modules-events.component';
import { TrackModuleCardComponent } from './steps/3_modules-events/track-module-card/track-module-card.component';
import { SettingsEventsService } from '../../_services/events.service';
import { TrackEventCardComponent } from './steps/3_modules-events/track-event-card/track-event-card.component';
import { CreatedTrackDialogComponent } from './steps/6_created-track/created-track.dialog';
import { PipesModule } from '../../../shared/pipes/pipes.module';
import { NewTrackVideoComponent } from './steps/2_video/video.component';
import { NewTrackRelevantDatesComponent } from './steps/4_relevant-dates/relevant-dates.component';
import { ListSearchModule } from 'src/app/shared/components/list-search/list-search.module';
import { ClickOutsideModule } from 'ng-click-outside';
import { NewTrackModulesEventsWeightComponent } from './steps/3.6_modules-weight/modules-weight.component';
import { NewTrackModulesGradesComponent } from './steps/7_modules-grades/modules-grades.component';
import { NewTrackEcommerceComponent } from './steps/5_ecommerce/ecommerce.component';
import { NewTrackModulesEventsDatesComponent } from './steps/3.5_modules-dates/modules-dates.component';
import { CurrencyMaskModule } from 'ng2-currency-mask';

@NgModule({
  declarations: [
    SettingsNewTrackComponent,
    NewTrackTrackInfoComponent,
    NewTrackVideoComponent,
    NewTrackModulesEventsComponent,
    TrackModuleCardComponent,
    TrackEventCardComponent,
    CreatedTrackDialogComponent,
    NewTrackRelevantDatesComponent,
    NewTrackModulesEventsWeightComponent,
    NewTrackModulesGradesComponent,
    NewTrackEcommerceComponent,
    NewTrackModulesEventsDatesComponent
  ],
  imports: [
    BrowserModule,
    MaterialComponentsModule,
    FormsModule,
    ReactiveFormsModule,
    NgUtilModule,
    ImageCropModule,
    NgxMaskModule,
    ConfirmDialogModule,
    PaginationModule,
    PipesModule,
    ListSearchModule,
    ClickOutsideModule,
    CurrencyMaskModule
  ],
  entryComponents: [
    CreatedTrackDialogComponent
  ],
  providers: [
    SettingsTracksService,
    SettingsEventsService,
    SharedService,
    UtilService
  ]
})
export class NewTrackModule { }
