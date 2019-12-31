import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { NgUtilModule } from '@tg4/ng-util';
import { MaterialComponentsModule } from '../../shared/material.module';
import { ContentEventsService } from 'src/app/pages/_services/events.service';
import { EventsValuationComponent } from './events-valuation.component';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';

@NgModule({
  declarations: [
    EventsValuationComponent,
  ],
  imports: [
    BrowserModule,
    BrowserAnimationsModule,
    FormsModule,
    ReactiveFormsModule,
    NgUtilModule,
    MaterialComponentsModule
  ],
  providers: [
    ContentEventsService
  ]
})
export class SettingsEventsValuationModule { }
