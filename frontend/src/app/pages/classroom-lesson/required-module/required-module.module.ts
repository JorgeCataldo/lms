import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { EventRequiredModuleComponent } from './required-module.component';

@NgModule({
  declarations: [
    EventRequiredModuleComponent
  ],
  imports: [
    BrowserModule
  ],
  exports: [
    EventRequiredModuleComponent
  ]
})
export class ClassroomLessonRequiredModule { }
