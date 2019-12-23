import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { RouterModule } from '@angular/router';
import { MaterialComponentsModule } from 'src/app/shared/material.module';
import { FormsModule } from '@angular/forms';
import { DataQuestionsTagsComponent } from './data-questions-tags.component';

@NgModule({
  declarations: [
    DataQuestionsTagsComponent
  ],
  imports: [
    BrowserModule,
    RouterModule,
    FormsModule,
    MaterialComponentsModule
  ]
})
export class DataQuestionsTagsModule { }
