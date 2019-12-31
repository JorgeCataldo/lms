import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';

import { BtgLoginComponent } from './btg-login.component';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { NgUtilModule } from '@tg4/ng-util';
import { MaterialComponentsModule } from 'src/app/shared/material.module';

@NgModule({
  declarations: [
    BtgLoginComponent,
  ],
  imports: [
    BrowserModule,
    MaterialComponentsModule,
    FormsModule,
    ReactiveFormsModule,
    NgUtilModule
  ],
  exports: [
    BtgLoginComponent
  ]
})
export class BtgLoginModule { }
