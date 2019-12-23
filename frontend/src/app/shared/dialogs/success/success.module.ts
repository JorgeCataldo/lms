import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { MaterialComponentsModule } from '../../material.module';
import { SuccessDialogComponent } from './success.dialog';

@NgModule({
  declarations: [
    SuccessDialogComponent
  ],
  imports: [
    BrowserModule,
    MaterialComponentsModule
  ],
  entryComponents: [
    SuccessDialogComponent
  ]
})
export class SuccessDialogModule { }
