import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { RouterModule } from '@angular/router';
import { PaginationModule } from '../../shared/components/pagination/pagination.module';
import { MaterialComponentsModule } from '../../shared/material.module';
import { SettingsUsersService } from '../_services/users.service';
import { ListSearchModule } from '../../shared/components/list-search/list-search.module';
import { SettingsManageTeamComponent } from 'src/app/settings/manage-team/manage-team.component';
import { PipesModule } from '../../shared/pipes/pipes.module';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { SettingsRecommendTrackComponent } from './recommend-track/recommend-track.component';
import { SettingsTrackCardSelectComponent } from './recommend-track/track-card-select/track-card-select.component';
import { ClickOutsideModule } from 'ng-click-outside';
import { SuccessDialogModule } from 'src/app/shared/dialogs/success/success.module';
import { SettingsModuleCardSelectComponent } from './recommend-track/module-card-select/module-card-select.component';
import { SettingsEventCardSelectComponent } from './recommend-track/event-card-select/event-card-select.component';
import { MarkdownToHtmlModule, MarkdownToHtmlPipe } from 'markdown-to-html-pipe';
import { SettingsSuggestTestComponent } from './suggest-test/suggest-test.component';
import { SettingsTestCardSelectComponent } from './suggest-test/test-card/test-card.component';
import { ManageUserRegisterFiltersModule } from './filters/filters.module';
import { SettingsManageUserRegisterComponent } from './manage-user-register.component';

@NgModule({
  declarations: [
    SettingsManageUserRegisterComponent,
    SettingsRecommendTrackComponent,
    SettingsTrackCardSelectComponent,
    SettingsModuleCardSelectComponent,
    SettingsEventCardSelectComponent,
    SettingsSuggestTestComponent,
    SettingsTestCardSelectComponent,
  ],
  imports: [
    BrowserModule,
    MaterialComponentsModule,
    RouterModule,
    PaginationModule,
    ListSearchModule,
    PipesModule,
    FormsModule,
    ClickOutsideModule,
    SuccessDialogModule,
    ReactiveFormsModule,
    MarkdownToHtmlModule,
    ManageUserRegisterFiltersModule
  ],
  providers: [
    SettingsUsersService,
    MarkdownToHtmlPipe
  ]
})
export class SettingsManageUserRegisterModule { }
