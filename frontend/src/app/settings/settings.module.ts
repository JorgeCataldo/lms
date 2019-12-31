import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { MaterialComponentsModule } from '../shared/material.module';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { NgUtilModule } from '@tg4/ng-util';
import { NewModuleModule } from 'src/app/settings/modules/new-module/new-module.module';
import { RouterModule } from '@angular/router';
import { SettingsEventsModule } from './events/events.module';
import { SettingsModulesModule } from './modules/modules.module';
import { SettingsTracksModule } from './tracks/tracks.module';
import { SettingsUsersModule } from './users/users.module';
import { SettingsUsersSyncModule } from './users-sync/users-sync.module';
import { SettingsEventsApplicationsModule } from './events-applications/events-applications.module';
import { SettingsManageTeamModule } from './manage-team/manage-team.module';
import { SettingsEventsApplicationsGradesModule } from './events-applications-grades/events-applications-grades.module';
import { SettingsEventsValuationModule } from './events-valuation/events-valuation.module';
import { SettingsCustomEmailSentModule } from './custom-email-sent/custom-email-sent.module';
import { SettingsProfileTestsModule } from './profile-tests/profile-tests.module';
import { SettingsProductSuggestionModule } from './product-suggestion/product-suggestion.module';
import { SettingsFormulasModule } from './formulas/formulas.module';
import { LogsModule } from './logs/logs.module';
import { ColorEditModule } from './color-edit/color-edit.module';
import { SettingsValuationTestsModule } from './valuation-tests/valuation-tests.module';
import { ValuationTestModule } from './valuation-test/valuation-test.module';
import { SettingsNpsModule } from './nps/nps.module';
import { EffortPerformanceModule } from '../pages/effort-performance/effort-performance.module';
import { SettingsManageUserRegisterModule } from './manage-user-register/manage-user-register.module';

@NgModule({
  imports: [
    BrowserModule,
    MaterialComponentsModule,
    FormsModule,
    ReactiveFormsModule,
    NgUtilModule,
    NewModuleModule,
    RouterModule,
    SettingsEventsModule,
    SettingsModulesModule,
    SettingsTracksModule,
    SettingsUsersModule,
    SettingsUsersSyncModule,
    SettingsEventsApplicationsModule,
    SettingsEventsApplicationsGradesModule,
    SettingsEventsValuationModule,
    SettingsManageTeamModule,
    SettingsCustomEmailSentModule,
    SettingsProfileTestsModule,
    SettingsProductSuggestionModule,
    SettingsFormulasModule,
    LogsModule,
    ColorEditModule,
    SettingsValuationTestsModule,
    ValuationTestModule,
    SettingsNpsModule,
    EffortPerformanceModule,
    SettingsManageUserRegisterModule
  ]
})
export class SettingsModule { }
