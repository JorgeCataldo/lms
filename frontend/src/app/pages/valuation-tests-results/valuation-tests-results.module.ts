import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { MaterialComponentsModule } from '../../shared/material.module';
import { ValuationTestsResultsComponent } from './valuation-tests-results.component';
import { SettingsValuationTestsService } from '../../settings/_services/valuation-tests.service';
import { ListSearchModule } from 'src/app/shared/components/list-search/list-search.module';
import { SettingsValuationTestResultsCardComponent } from './valuation-test-results-card/valuation-test-results-card.component';
@NgModule({
  declarations: [
    ValuationTestsResultsComponent,
    SettingsValuationTestResultsCardComponent
  ],
  imports: [
    BrowserModule,
    MaterialComponentsModule,
    FormsModule,
    ReactiveFormsModule,
    RouterModule,
    ListSearchModule
  ],
  entryComponents: [
    // ManageValuationQuestionDialogComponent
  ],
  providers: [
    SettingsValuationTestsService
  ]
})
export class SettingsValuationTestsResultsModule { }
