import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { MaterialComponentsModule } from '../../shared/material.module';
import { ProductSuggestionComponent } from './product-suggestion.component';
import { PaginationModule } from 'src/app/shared/components/pagination/pagination.module';
import { ManageSuggestionComponent } from './manage-suggestion/manage-suggestion.component';
import { ListSearchModule } from 'src/app/shared/components/list-search/list-search.module';
import { SuggestionModuleSelectComponent } from './manage-suggestion/module-select/module-select.component';
import { SuggestionEventSelectComponent } from './manage-suggestion/event-select/event-select.component';
import { SuggestionTrackSelectComponent } from './manage-suggestion/track-select/track-select.component';
import { NotifyDialogModule } from 'src/app/shared/dialogs/notify/notify.dialog.module';
import { SuggestionAreaToggleComponent } from './manage-suggestion/area-toggle/area-toggle.component';

@NgModule({
  declarations: [
    ProductSuggestionComponent,
    ManageSuggestionComponent,
    SuggestionModuleSelectComponent,
    SuggestionEventSelectComponent,
    SuggestionTrackSelectComponent,
    SuggestionAreaToggleComponent
  ],
  imports: [
    BrowserModule,
    MaterialComponentsModule,
    FormsModule,
    ReactiveFormsModule,
    RouterModule,
    PaginationModule,
    ListSearchModule,
    NotifyDialogModule
  ]
})
export class SettingsProductSuggestionModule { }
