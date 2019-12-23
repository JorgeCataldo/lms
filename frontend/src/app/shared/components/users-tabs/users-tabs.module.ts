import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { UsersTabsComponent } from './users-tabs.component';

@NgModule({
  declarations: [
    UsersTabsComponent
  ],
  imports: [
    BrowserModule
  ],
  exports: [
    UsersTabsComponent
  ]
})
export class UsersTabsModule { }
