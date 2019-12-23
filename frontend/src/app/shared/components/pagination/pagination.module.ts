import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { PaginationComponent } from './pagination.component';
import { MaterialComponentsModule } from '../../material.module';
import { FormsModule } from '@angular/forms';
import { PipesModule } from '../../pipes/pipes.module';

@NgModule({
  declarations: [
    PaginationComponent
  ],
  imports: [
    BrowserModule,
    FormsModule,
    MaterialComponentsModule,
    PipesModule
  ],
  exports: [
    PaginationComponent
  ]
})
export class PaginationModule { }
