import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { MaterialComponentsModule } from '../../material.module';
import { NpsDialogComponent } from './nps.dialog';

@NgModule({
  declarations: [
    NpsDialogComponent
  ],
  imports: [
    BrowserModule,
    MaterialComponentsModule
  ],
  entryComponents: [
    NpsDialogComponent
  ]
})
export class NPSDialogModule { }
