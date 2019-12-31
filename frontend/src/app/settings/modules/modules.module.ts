import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { NgUtilModule } from '@tg4/ng-util';
import { NewModuleModule } from 'src/app/settings/modules/new-module/new-module.module';
import { RouterModule } from '@angular/router';
import { SettingsModulesComponent } from './modules.component';
import { SettingsModuleCardComponent } from './module-card/module-card.component';
import { ContentModulesService } from '../../pages/_services/modules.service';
import { MaterialComponentsModule } from '../../shared/material.module';
import { PaginationModule } from '../../shared/components/pagination/pagination.module';
import { ExcelService } from 'src/app/shared/services/excel.service';
import { DeleteModuleDialogComponent } from './delete-module/delete-module.dialog';
import { CloneModuleDialogComponent } from './clone-module/clone-module.dialog';

@NgModule({
  declarations: [
    SettingsModulesComponent,
    SettingsModuleCardComponent,
    DeleteModuleDialogComponent,
    CloneModuleDialogComponent
  ],
  imports: [
    BrowserModule,
    MaterialComponentsModule,
    FormsModule,
    ReactiveFormsModule,
    NgUtilModule,
    NewModuleModule,
    RouterModule,
    PaginationModule
  ],
  entryComponents: [
    DeleteModuleDialogComponent,
    CloneModuleDialogComponent
  ],
  providers: [
    ContentModulesService,
    ExcelService
  ]
})
export class SettingsModulesModule { }
