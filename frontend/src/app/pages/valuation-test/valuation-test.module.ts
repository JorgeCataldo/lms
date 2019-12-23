import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { RouterModule } from '@angular/router';
import { ValuationTestComponent } from './valuation-test.component';
import { MaterialComponentsModule } from 'src/app/shared/material.module';
import { FormsModule } from '@angular/forms';

@NgModule({
  declarations: [
    ValuationTestComponent
  ],
  imports: [
    BrowserModule,
    RouterModule,
    FormsModule,
    MaterialComponentsModule
  ]
})
export class ValuationTestModule { }
