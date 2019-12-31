import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { DateRangePickerComponent } from './date-range-picker.component';
import { MaterialComponentsModule } from '../../material.module';

@NgModule({
  declarations: [
    DateRangePickerComponent
  ],
  imports: [
    BrowserModule,
    FormsModule,
    MaterialComponentsModule
  ],
  exports: [
    DateRangePickerComponent
  ]
})
export class DateRangePickerModule { }
