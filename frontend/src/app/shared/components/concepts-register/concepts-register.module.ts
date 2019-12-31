import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { ConceptsRegisterComponent } from './concepts-register.component';

@NgModule({
  declarations: [
    ConceptsRegisterComponent
  ],
  imports: [
    BrowserModule
  ],
  exports: [
    ConceptsRegisterComponent
  ]
})
export class ConceptsRegisterModule { }
