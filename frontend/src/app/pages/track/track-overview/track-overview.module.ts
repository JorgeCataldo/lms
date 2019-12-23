import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { PipesModule } from 'src/app/shared/pipes/pipes.module';
import { RouterModule } from '@angular/router';
import { CardTagModule } from 'src/app/shared/components/layout/card-tag/card-tag.module';
import { ProgressBarModule } from 'src/app/shared/components/layout/progress-bar/progress-bar.module';
import { ModuleCardModule } from 'src/app/shared/components/module-card/module-card.module';
import { EventCardModule } from 'src/app/shared/components/event-card/event-card.module';
import { TrackPathModule } from './track-path/track-path.module';
import { TrackOverviewComponent } from './track-overview.component';
import { TrackOverviewWarningsComponent } from './track-warnings/track-warnings.component';
import { TrackMandatoryVideoComponent } from './track-mandatory-video/track-mandatory-video.component';
import { VideoDialogComponent } from 'src/app/shared/components/video-dialog/video.dialog';
import { TrackCalendarModule } from './track-calendar/track-calendar.module';

@NgModule({
  declarations: [
    TrackOverviewComponent,
    TrackOverviewWarningsComponent,
    TrackMandatoryVideoComponent,
    VideoDialogComponent
  ],
  imports: [
    BrowserModule,
    RouterModule,
    CardTagModule,
    ProgressBarModule,
    ModuleCardModule,
    EventCardModule,
    TrackPathModule,
    PipesModule,
    TrackCalendarModule
  ],
  exports: [
    TrackOverviewComponent
  ],
  entryComponents: [
    VideoDialogComponent
  ]
})
export class TrackOverviewModule { }
