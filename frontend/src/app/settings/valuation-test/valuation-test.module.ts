import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { MaterialComponentsModule } from '../../shared/material.module';
import { PaginationModule } from 'src/app/shared/components/pagination/pagination.module';
import { NotifyDialogModule } from 'src/app/shared/dialogs/notify/notify.dialog.module';
import { SettingsValuationTestComponent } from './valuation-test.component';
import { SettingsValuationTestGradeComponent } from './valuation-test-grade/valuation-test-grade.component';

@NgModule({
  declarations: [
    SettingsValuationTestComponent,
    SettingsValuationTestGradeComponent
  ],
  imports: [
    BrowserModule,
    MaterialComponentsModule,
    FormsModule,
    ReactiveFormsModule,
    RouterModule,
    PaginationModule,
    NotifyDialogModule
  ]
})
export class ValuationTestModule { }
