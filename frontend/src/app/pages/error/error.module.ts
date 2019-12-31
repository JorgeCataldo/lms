import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { RouterModule } from '@angular/router';
import { ErrorComponent } from './error.component';

@NgModule({
  declarations: [
    ErrorComponent
  ],
  imports: [
    BrowserModule,
    RouterModule
  ]
})
export class ErrorModule { }
