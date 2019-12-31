import { NgModule } from '@angular/core';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import {
  MatCheckboxModule,
  MatDatepickerModule,
  MatDialogModule,
  MatExpansionModule,
  MatFormFieldModule,
  MatInputModule,
  MatNativeDateModule,
  MatPaginatorModule,
  MatProgressSpinnerModule,
  MatSelectModule,
  MatSnackBarModule,
  MatStepperModule,
  MatTabsModule,
  MatTableModule,
  MatProgressBarModule,
  MatRadioModule,
  MatSidenavModule,
  MatSortModule,
  MAT_DATE_LOCALE,
  MatSliderModule,
  MatSlideToggleModule
} from '@angular/material';

@NgModule({
  imports: [
    BrowserAnimationsModule,
    MatCheckboxModule,
    MatDatepickerModule,
    MatDialogModule,
    MatExpansionModule,
    MatFormFieldModule,
    MatInputModule,
    MatNativeDateModule,
    MatPaginatorModule,
    MatProgressSpinnerModule,
    MatSelectModule,
    MatSnackBarModule,
    MatStepperModule,
    MatTabsModule,
    MatTableModule,
    MatProgressBarModule,
    MatRadioModule,
    MatSidenavModule,
    MatSortModule,
    MatSliderModule,
    MatSlideToggleModule
  ],
  exports: [
    MatCheckboxModule,
    MatDatepickerModule,
    MatDialogModule,
    MatExpansionModule,
    MatFormFieldModule,
    MatInputModule,
    MatNativeDateModule,
    MatPaginatorModule,
    MatProgressSpinnerModule,
    MatSelectModule,
    MatSnackBarModule,
    MatStepperModule,
    MatTabsModule,
    MatTableModule,
    MatProgressBarModule,
    MatRadioModule,
    MatSidenavModule,
    MatSortModule,
    MatSliderModule,
    MatSlideToggleModule
  ],
  providers: [
    { provide: MAT_DATE_LOCALE, useValue: 'pt-BR' },
  ]
})
export class MaterialComponentsModule { }
