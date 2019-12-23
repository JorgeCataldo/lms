import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { MessagesComponent } from './messages.component';
import { MaterialComponentsModule } from 'src/app/shared/material.module';

@NgModule({
  declarations: [
    MessagesComponent
  ],
  imports: [
    BrowserModule,
    FormsModule,
    ReactiveFormsModule,
    RouterModule,
    MaterialComponentsModule
  ]
})
export class MessagesModule { }
