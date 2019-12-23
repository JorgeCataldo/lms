import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { TrackCalendarComponent } from './track-calendar.component';
import { CalendarModule, DateAdapter } from 'angular-calendar';
import { adapterFactory } from 'angular-calendar/date-adapters/date-fns';
import localePt from '@angular/common/locales/pt';
import { registerLocaleData } from '@angular/common';

registerLocaleData(localePt);

@NgModule({
  declarations: [
    TrackCalendarComponent
  ],
  imports: [
    BrowserModule,
    CalendarModule.forRoot({
      provide: DateAdapter,
      useFactory: adapterFactory
    })
  ],
  exports: [
    TrackCalendarComponent
  ]
})
export class TrackCalendarModule { }
