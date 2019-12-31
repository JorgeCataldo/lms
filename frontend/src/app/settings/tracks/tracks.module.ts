import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { NgUtilModule } from '@tg4/ng-util';
import { RouterModule } from '@angular/router';
import { NewModuleModule } from 'src/app/settings/modules/new-module/new-module.module';
import { PaginationModule } from '../../shared/components/pagination/pagination.module';
import { SettingsTracksService } from '../_services/tracks.service';
import { SettingsTracksComponent } from './tracks.component';
import { MaterialComponentsModule } from '../../shared/material.module';
import { SettingsTrackCardComponent } from './track-card/track-card.component';
import { NewTrackModule } from './new-track/new-track.module';
import { DeleteTrackDialogComponent } from './delete-track/delete-track.dialog';
import { SettingsTrackOverviewComponent } from './track-overview/track-overview.component';
import { TrackPathModule } from 'src/app/pages/track/track-overview/track-path/track-path.module';
import { ProgressBarModule } from 'src/app/shared/components/layout/progress-bar/progress-bar.module';
import { TrackStudentOverviewComponent } from './track-overview/student-overview/student-overview.component';
import { TrackOverviewStudentsComponent } from './track-overview/components/track-students/track-students.component';
import { TrackOverviewWrongConceptsComponent } from './track-overview/components/wrong-concepts/wrong-concepts.component';
import { TrackOverviewPerformanceRadarComponent } from './track-overview/components/performance-radar/performance-radar.component';
import { TrackOverviewEventsParticipationComponent } from './track-overview/components/events-participation/events-participation.component';
import { TrackOverviewWrongConceptsDialogComponent } from './track-overview/components/wrong-concepts-dialog/wrong-concepts.dialog';
import { TrackModuleOverviewComponent } from './track-overview/module-overview/module-overview.component';
import { TrackOverviewContentViewsComponent } from './track-overview/components/content-views/content-views.component';
import { TrackOverviewBadgesComponent } from './track-overview/components/badges/badges.component';
import { TrackOverviewStudentsProgressComponent } from './track-overview/components/students-progress/students-progress.component';
import { PipesModule } from 'src/app/shared/pipes/pipes.module';
import { TrackOverviewTopBottomPerformesComponent } from './track-overview/components/top-bottom-performes/top-bottom-performes.component';
// tslint:disable-next-line:max-line-length
import { TrackOverviewTopBottomPerformesDialogComponent } from './track-overview/components/top-bottom-performes-dialog/top-bottom-performes.dialog';
import { TrackOverviewModule } from 'src/app/pages/track/track-overview/track-overview.module';
import { TrackOverviewBadgesDialogComponent } from './track-overview/components/badges-dialog/badges.dialog';
import { TrackOverviewTrackParticipationComponent } from './track-overview/components/track-participation/track-participation.component';

@NgModule({
  declarations: [
    SettingsTracksComponent,
    SettingsTrackCardComponent,
    DeleteTrackDialogComponent,
    SettingsTrackOverviewComponent,
    TrackOverviewStudentsComponent,
    TrackStudentOverviewComponent,
    TrackOverviewPerformanceRadarComponent,
    TrackOverviewWrongConceptsComponent,
    TrackOverviewEventsParticipationComponent,
    TrackOverviewWrongConceptsDialogComponent,
    TrackOverviewTopBottomPerformesDialogComponent,
    TrackOverviewBadgesDialogComponent,
    TrackOverviewContentViewsComponent,
    TrackModuleOverviewComponent,
    TrackOverviewBadgesComponent,
    TrackOverviewStudentsProgressComponent,
    TrackOverviewTopBottomPerformesComponent,
    TrackOverviewTrackParticipationComponent
  ],
  imports: [
    BrowserModule,
    MaterialComponentsModule,
    FormsModule,
    ReactiveFormsModule,
    NgUtilModule,
    NewModuleModule,
    RouterModule,
    PaginationModule,
    NewTrackModule,
    TrackPathModule,
    ProgressBarModule,
    PipesModule,
    TrackOverviewModule
  ],
  entryComponents: [
    DeleteTrackDialogComponent,
    TrackOverviewWrongConceptsDialogComponent,
    TrackOverviewTopBottomPerformesDialogComponent,
    TrackOverviewBadgesDialogComponent
  ],
  providers: [
    SettingsTracksService
  ],
  exports:
  [
    TrackOverviewBadgesComponent,
    TrackOverviewPerformanceRadarComponent,
    TrackOverviewContentViewsComponent,
    TrackOverviewWrongConceptsComponent
  ]
})
export class SettingsTracksModule { }
