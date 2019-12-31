import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { RouterModule } from '@angular/router';
import { PaginationModule } from '../../shared/components/pagination/pagination.module';
import { MaterialComponentsModule } from '../../shared/material.module';
import { SettingsUsersService } from '../_services/users.service';
import { ListSearchModule } from '../../shared/components/list-search/list-search.module';
import { NpsComponent } from './nps.component';
import { NpsAssociatedObjectsDialogComponent } from './nps-associated-objects-dialog/nps-associated-objects.dialog';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';

@NgModule({
  declarations: [
    NpsComponent,
    NpsAssociatedObjectsDialogComponent
  ],
  imports: [
    BrowserModule,
    MaterialComponentsModule,
    RouterModule,
    PaginationModule,
    ListSearchModule,
    FormsModule,
    ReactiveFormsModule
  ],
  providers: [
    SettingsUsersService
  ],
  entryComponents: [
    NpsAssociatedObjectsDialogComponent
  ]
})
export class SettingsNpsModule { }
