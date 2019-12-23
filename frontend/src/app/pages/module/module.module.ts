import { NgModule } from '@angular/core';
import { ModuleComponent } from './module.component';
import { BrowserModule } from '@angular/platform-browser';
import { ProgressBarModule } from '../../shared/components/layout/progress-bar/progress-bar.module';
import { ModuleSubjectComponent } from './module-subject/module-subject.component';
import { SubjectContentComponent } from './module-subject/subject-content/subject-content.component';
import { RouterModule } from '@angular/router';
import { ModuleHeaderComponent } from './module-header/module-header.component';
import { ModuleSidebarModule } from '../../shared/components/module-sidebar/module-sidebar.module';
import { PipesModule } from '../../shared/pipes/pipes.module';
import { RequiredModuleComponent } from './required-module/required-module.component';
import { DirectivesModule } from 'src/app/shared/directives/directives.module';
import { ModuleInfoComponent } from './module-info/module-info.component';
import { SettingsTracksModule } from 'src/app/settings/tracks/tracks.module';

@NgModule({
  declarations: [
    ModuleComponent,
    ModuleHeaderComponent,
    ModuleSubjectComponent,
    SubjectContentComponent,
    RequiredModuleComponent,
    ModuleInfoComponent
  ],
  imports: [
    BrowserModule,
    ProgressBarModule,
    RouterModule,
    ModuleSidebarModule,
    PipesModule,
    DirectivesModule,
    SettingsTracksModule

  ]
})
export class ModuleModule { }
