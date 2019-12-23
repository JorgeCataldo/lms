import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { ProgressBarModule } from '../layout/progress-bar/progress-bar.module';
import { TrackCardComponent } from './track-card.component';
import { CardTagModule } from '../layout/card-tag/card-tag.module';
import { RouterModule } from '@angular/router';
import { NgxMaskModule } from 'ngx-mask';

@NgModule({
  declarations: [
    TrackCardComponent
  ],
  imports: [
    BrowserModule,
    RouterModule,
    ProgressBarModule,
    CardTagModule,
    NgxMaskModule
  ],
  exports: [
    TrackCardComponent
  ]
})
export class TrackCardModule { }
