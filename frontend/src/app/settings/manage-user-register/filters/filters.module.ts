import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { RouterModule } from '@angular/router';
import { MaterialComponentsModule } from 'src/app/shared/material.module';
import { ListSearchModule } from 'src/app/shared/components/list-search/list-search.module';
import { ManageUserRegisterFiltersComponent } from './filters.component';
import { FormsModule } from '@angular/forms';

@NgModule({
  declarations: [
    ManageUserRegisterFiltersComponent
  ],
  imports: [
    BrowserModule,
    MaterialComponentsModule,
    RouterModule,
    ListSearchModule,
    FormsModule
  ],
  exports: [
    ManageUserRegisterFiltersComponent
  ]
})
export class ManageUserRegisterFiltersModule { }
