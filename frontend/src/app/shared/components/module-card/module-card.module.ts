import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { ProgressBarModule } from '../layout/progress-bar/progress-bar.module';
import { ModuleCardComponent } from './module-card.component';
import { CardTagModule } from '../layout/card-tag/card-tag.module';
import { NgxMaskModule } from 'ngx-mask';

@NgModule({
  declarations: [
    ModuleCardComponent
  ],
  imports: [
    BrowserModule,
    ProgressBarModule,
    CardTagModule,
    NgxMaskModule
  ],
  exports: [
    ModuleCardComponent
  ]
})
export class ModuleCardModule { }
