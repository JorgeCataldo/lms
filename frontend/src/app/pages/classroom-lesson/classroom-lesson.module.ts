import { NgModule } from '@angular/core';
import { ClassroomLessonComponent } from './classroom-lesson.component';
import { BrowserModule } from '@angular/platform-browser';
import { ProgressBarModule } from '../../shared/components/layout/progress-bar/progress-bar.module';
import { ModuleSidebarModule } from '../../shared/components/module-sidebar/module-sidebar.module';
import { MaterialComponentsModule } from '../../shared/material.module';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { PipesModule } from '../../shared/pipes/pipes.module';
import { RouterModule } from '@angular/router';
import { ClassroomLessonHeaderModule } from './lesson-header/lesson-header.module';
import { ClassroomLessonRequiredModule } from './required-module/required-module.module';

@NgModule({
  declarations: [
    ClassroomLessonComponent
  ],
  imports: [
    BrowserModule,
    FormsModule,
    ReactiveFormsModule,
    MaterialComponentsModule,
    ProgressBarModule,
    ModuleSidebarModule,
    PipesModule,
    RouterModule,
    ClassroomLessonHeaderModule,
    ClassroomLessonRequiredModule
  ]
})
export class ClassroomLessonModule { }
