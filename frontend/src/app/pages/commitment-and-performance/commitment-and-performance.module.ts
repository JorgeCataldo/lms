import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { CommitmentAndPerformanceComponent } from './commitment-and-performance.component';
import { MaterialComponentsModule } from 'src/app/shared/material.module';

@NgModule({
  declarations: [
    CommitmentAndPerformanceComponent
  ],
  imports: [
    BrowserModule,
    FormsModule,
    ReactiveFormsModule,
    RouterModule,
    MaterialComponentsModule
  ]
})
export class CommitmentAndPerformanceModule { }
