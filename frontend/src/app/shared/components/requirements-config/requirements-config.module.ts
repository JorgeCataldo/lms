import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { RequirementsConfigComponent } from './requirements-config.component';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MaterialComponentsModule } from '../../material.module';

@NgModule({
  declarations: [
    RequirementsConfigComponent
  ],
  imports: [
    BrowserModule,
    FormsModule,
    ReactiveFormsModule,
    MaterialComponentsModule
  ],
  exports: [
    RequirementsConfigComponent
  ]
})
export class RequirementsConfigModule { }
