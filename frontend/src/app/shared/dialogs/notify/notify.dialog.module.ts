import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { MaterialComponentsModule } from '../../material.module';
import { NotifyDialogComponent } from './notify.dialog';

@NgModule({
  declarations: [
    NotifyDialogComponent
  ],
  imports: [
    BrowserModule,
    MaterialComponentsModule
  ],
  entryComponents: [
    NotifyDialogComponent
  ]
})
export class NotifyDialogModule { }
