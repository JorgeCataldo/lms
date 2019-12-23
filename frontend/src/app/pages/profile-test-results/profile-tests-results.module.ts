import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { MaterialComponentsModule } from '../../shared/material.module';
import { ProfileTestsResultsComponent } from './profile-tests-results.component';
import { SettingsTestResultsCardComponent } from './test-results-card/test-results-card.component';
// import { SettingsManageTestComponent } from './manage-test/manage-test.component';
import { SettingsProfileTestsService } from '../../settings/_services/profile-tests.service';
import { SettingsResearchResultsCardComponent } from './research-results-card/research-results-card.component';
import { ActivationsService } from '../../settings/_services/activations.service';
import { ActivationEditDialogComponent } from './activation-edit-dialog/activation-edit.dialog';

@NgModule({
  declarations: [
    ProfileTestsResultsComponent,
    // SettingsManageTestComponent,
    SettingsTestResultsCardComponent,
    SettingsResearchResultsCardComponent,
    // ManageProfileQuestionDialogComponent,
    ActivationEditDialogComponent
  ],
  imports: [
    BrowserModule,
    MaterialComponentsModule,
    FormsModule,
    ReactiveFormsModule,
    RouterModule
  ],
  entryComponents: [
    ActivationEditDialogComponent
  ],
  providers: [
    SettingsProfileTestsService,
    ActivationsService
  ]
})
export class SettingsProfileTestsResultsModule { }
