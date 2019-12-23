import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { RouterModule } from '@angular/router';
import { ProfileTestComponent } from './profile-test.component';
import { MaterialComponentsModule } from 'src/app/shared/material.module';
import { FormsModule } from '@angular/forms';

@NgModule({
  declarations: [
    ProfileTestComponent
  ],
  imports: [
    BrowserModule,
    RouterModule,
    FormsModule,
    MaterialComponentsModule
  ]
})
export class ProfileTestModule { }
