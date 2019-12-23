import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { TrackComponent } from './track.component';
import { CardTagModule } from '../../shared/components/layout/card-tag/card-tag.module';
import { ProgressBarModule } from '../../shared/components/layout/progress-bar/progress-bar.module';
import { TrackHeaderComponent } from './track-header/track-header.component';
import { RouterModule } from '@angular/router';
import { EventCardModule } from '../../shared/components/event-card/event-card.module';
import { ModuleCardModule } from '../../shared/components/module-card/module-card.module';
import { TrackPathModule } from './track-overview/track-path/track-path.module';
import { PipesModule } from 'src/app/shared/pipes/pipes.module';
import { TrackOverviewModule } from './track-overview/track-overview.module';
import { TrackCalendarModule } from './track-overview/track-calendar/track-calendar.module';

@NgModule({
  declarations: [
    TrackComponent,
    TrackHeaderComponent
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
    TrackOverviewModule,
    TrackCalendarModule
  ]
})
export class TrackModule { }
