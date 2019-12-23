import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { MaterialComponentsModule } from '../../shared/material.module';
import { FormulasComponent } from './formulas.component';
import { FormulaCardComponent } from './formula-card/formula-card.component';
import { ManageFormulaComponent } from './manage-formula/manage-formula.component';
import { SettingsFormulasService } from '../_services/formulas.service';
import { PaginationModule } from 'src/app/shared/components/pagination/pagination.module';

@NgModule({
  declarations: [
    FormulasComponent,
    FormulaCardComponent,
    ManageFormulaComponent
  ],
  imports: [
    BrowserModule,
    MaterialComponentsModule,
    FormsModule,
    ReactiveFormsModule,
    RouterModule,
    PaginationModule
  ],
  providers: [
    SettingsFormulasService
  ]
})
export class SettingsFormulasModule { }
