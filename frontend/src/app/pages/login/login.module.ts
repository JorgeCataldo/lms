import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';

import { LoginComponent } from './login.component';
import { MaterialComponentsModule } from '../../shared/material.module';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { NgUtilModule } from '@tg4/ng-util';
import { BtgLoginModule } from './btg-login/btg-login.module';
import { BtgLoginComponent } from './btg-login/btg-login.component';
import { NgxMaskModule } from 'ngx-mask';
import { EmailConfirmationComponent } from './email-validation/email-confirmation.component';

@NgModule({
  declarations: [
    LoginComponent,
    EmailConfirmationComponent
  ],
  imports: [
    BrowserModule,
    MaterialComponentsModule,
    FormsModule,
    ReactiveFormsModule,
    NgUtilModule,
    NgxMaskModule,
    BtgLoginModule
  ],
  exports: [
    LoginComponent,
    EmailConfirmationComponent
  ],
  entryComponents: [
    BtgLoginComponent
  ]
})
export class LoginModule { }
