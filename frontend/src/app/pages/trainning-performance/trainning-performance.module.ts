import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { RouterModule } from '@angular/router';
import { MaterialComponentsModule } from 'src/app/shared/material.module';
import { FormsModule } from '@angular/forms';
import { TrainningPerformanceComponent } from './trainning-performance.component';

@NgModule({
  declarations: [
    TrainningPerformanceComponent
  ],
  imports: [
    BrowserModule,
    RouterModule,
    FormsModule,
    MaterialComponentsModule
  ]
})
export class TrainningPerformanceModule { }
