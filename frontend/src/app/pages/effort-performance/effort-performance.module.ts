import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { RouterModule } from '@angular/router';
import { MaterialComponentsModule } from '../../shared/material.module';
import { PaginationModule } from 'src/app/shared/components/pagination/pagination.module';
import { EffortPerformanceComponent } from './effort-performance.component';

@NgModule({
  declarations: [
    EffortPerformanceComponent
  ],
  imports: [
    BrowserModule,
    MaterialComponentsModule,
    RouterModule,
    PaginationModule
  ]
})
export class EffortPerformanceModule { }
