import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { RouterModule } from '@angular/router';
import { MaterialComponentsModule } from 'src/app/shared/material.module';
import { FormsModule } from '@angular/forms';
import { ProfessionalPerformanceHistoryComponent } from './professional-performance-history.component';

@NgModule({
  declarations: [
    ProfessionalPerformanceHistoryComponent
  ],
  imports: [
    BrowserModule,
    RouterModule,
    FormsModule,
    MaterialComponentsModule
  ]
})
export class ProfessionalPerformanceHistoryModule { }
