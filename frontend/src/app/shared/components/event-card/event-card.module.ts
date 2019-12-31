import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { EventCardComponent } from './event-card.component';

@NgModule({
  declarations: [
    EventCardComponent
  ],
  imports: [
    BrowserModule,
    RouterModule
  ],
  exports: [
    EventCardComponent
  ]
})
export class EventCardModule { }
