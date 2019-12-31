import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { HeaderComponent } from './header.component';
import { RouterModule } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { MenuComponent } from './menu/menu.component';
import { SharedService } from '../../services/shared.service';
import { MaterialComponentsModule } from '../../material.module';

@NgModule({
  declarations: [
    HeaderComponent,
    MenuComponent
  ],
  imports: [
    BrowserModule,
    RouterModule,
    MaterialComponentsModule
  ],
  exports: [
    HeaderComponent,
    MenuComponent
  ],
  providers: [
    AuthService,
    SharedService
  ]
})
export class HeaderModule { }
