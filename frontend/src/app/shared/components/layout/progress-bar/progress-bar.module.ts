import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { ProgressBarComponent } from './progress-bar.component';

@NgModule({
  declarations: [
    ProgressBarComponent
  ],
  imports: [
    BrowserModule
  ],
  exports: [
    ProgressBarComponent
  ]
})

export class ProgressBarModule { }
