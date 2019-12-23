import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { ProgressBarModule } from 'src/app/shared/components/layout/progress-bar/progress-bar.module';
import { TrackOverviewPathComponent } from './track-path.component';

@NgModule({
  declarations: [
    TrackOverviewPathComponent
  ],
  imports: [
    BrowserModule,
    ProgressBarModule
  ],
  exports: [
    TrackOverviewPathComponent
  ]
})
export class TrackPathModule { }
