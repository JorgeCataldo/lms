import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { ConfirmDialogComponent } from './confirm.dialog';
import { MaterialComponentsModule } from '../../material.module';

@NgModule({
  declarations: [
    ConfirmDialogComponent
  ],
  imports: [
    BrowserModule,
    MaterialComponentsModule
  ],
  entryComponents: [
    ConfirmDialogComponent
  ]
})
export class ConfirmDialogModule { }
