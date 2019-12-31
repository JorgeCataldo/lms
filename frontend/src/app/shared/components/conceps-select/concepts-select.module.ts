import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { ConceptsSelectComponent } from './concepts-select.component';

@NgModule({
  declarations: [
    ConceptsSelectComponent
  ],
  imports: [
    BrowserModule
  ],
  exports: [
    ConceptsSelectComponent
  ]
})
export class ConceptsSelectModule { }
