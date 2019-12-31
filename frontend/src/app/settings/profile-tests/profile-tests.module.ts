import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { MaterialComponentsModule } from '../../shared/material.module';
import { ProfileTestsComponent } from './profile-tests.component';
import { SettingsTestCardComponent } from './test-card/test-card.component';
import { SettingsManageTestComponent } from './manage-test/manage-test.component';
import { SettingsProfileTestsService } from '../_services/profile-tests.service';
import { ManageProfileQuestionDialogComponent } from './manage-test/manage-question/manage-question.dialog';
import { SettingsResearchCardComponent } from './research-card/research-card.component';
import { ActivationsService } from '../_services/activations.service';
import { ActivationEditDialogComponent } from './activation-edit-dialog/activation-edit.dialog';

@NgModule({
  declarations: [
    ProfileTestsComponent,
    SettingsManageTestComponent,
    SettingsTestCardComponent,
    SettingsResearchCardComponent,
    ManageProfileQuestionDialogComponent,
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
    ManageProfileQuestionDialogComponent,
    ActivationEditDialogComponent
  ],
  providers: [
    SettingsProfileTestsService,
    ActivationsService
  ]
})
export class SettingsProfileTestsModule { }
