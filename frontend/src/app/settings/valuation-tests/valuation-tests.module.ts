import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { MaterialComponentsModule } from '../../shared/material.module';
import { ValuationTestsComponent } from './valuation-tests.component';
import { ManageValuationQuestionDialogComponent } from './manage-valuation-test/manage-valuation-question/manage-valuation-question.dialog';
import { SettingsValuationTestsService } from '../_services/valuation-tests.service';
import { SettingsManageValuationTestComponent } from './manage-valuation-test/manage-valuation-test.component';
import { SettingsValuationTestCardComponent } from './valuation-test-card/valuation-test-card.component';
import { ListSearchModule } from 'src/app/shared/components/list-search/list-search.module';
import { SettingsModuleCardValuationTestComponent } from './manage-valuation-test/module-card-valuation-test/module-card-select.component';
import { SettingsTrackCardValuationTestComponent } from './manage-valuation-test/track-card-valuation-test/track-card-select.component';
import { SettingsManageValuationTestReleaseComponent } from './manage-valuation-test-release/manage-valuation-test-release.component';
// tslint:disable-next-line: max-line-length
import { ValuationTestReleaseToggleComponent } from './manage-valuation-test-release/valuation-test-release-toggle/valuation-test-release-toggle.component';
import { SettingsModuleCardSelectComponent } from '../manage-team/recommend-track/module-card-select/module-card-select.component';

@NgModule({
  declarations: [
    ValuationTestsComponent,
    SettingsManageValuationTestComponent,
    SettingsManageValuationTestReleaseComponent,
    ValuationTestReleaseToggleComponent,
    SettingsValuationTestCardComponent,
    ManageValuationQuestionDialogComponent,
    SettingsModuleCardValuationTestComponent,
    SettingsTrackCardValuationTestComponent,
    SettingsValuationTestCardComponent,
    ManageValuationQuestionDialogComponent,
    SettingsModuleCardValuationTestComponent,
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
    ManageValuationQuestionDialogComponent
  ],
  providers: [
    SettingsValuationTestsService
  ]
})
export class SettingsValuationTestsModule { }
