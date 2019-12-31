import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { RouterModule } from '@angular/router';
import { MaterialComponentsModule } from '../../../shared/material.module';
import { ProgressBarModule } from '../../../shared/components/layout/progress-bar/progress-bar.module';
import { SettingsUserDetailsComponent } from './user-details.component';
import { SettingsUserDetailsSummaryComponent } from './user-summary/user-summary.component';
import { SettingsUsersService } from '../../_services/users.service';
import { ConceptTagModule } from '../../../shared/components/layout/concept-tag/concept-tag.module';
import { SettingsUserDetailsProgressComponent } from './user-progress/user-progress.component';
import { UserProgressCardComponent } from './user-progress/progress-card/progress-card.component';
import { CardsSliderModule } from '../../../shared/components/cards-slider/cards-slider.module';
import { TrackCalendarModule } from 'src/app/pages/track/track-overview/track-calendar/track-calendar.module';
import { SettingsUserCareerComponent } from './user-career/user-career.component';
import { SettingsUserInfoComponent } from './user-info/user-info.component';
import { SettingsManageUserComponent } from '../manage-user/manage-user.component';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { ListSearchModule } from 'src/app/shared/components/list-search/list-search.module';
import { SettingsUserPerformaceComponent } from './user-performace/user-performace.component';
import { SettingsUserCalendarComponent } from './user-calendar/user-calendar.component';
import { EventsCardComponent } from 'src/app/pages/my-courses/events/events-card.component';

@NgModule({
  declarations: [
    SettingsUserDetailsComponent,
    SettingsUserCareerComponent,
    SettingsUserDetailsSummaryComponent,
    SettingsUserDetailsProgressComponent,
    UserProgressCardComponent,
    SettingsUserInfoComponent,
    SettingsManageUserComponent,
    SettingsUserPerformaceComponent,
    SettingsUserCalendarComponent,
    EventsCardComponent
  ],
  imports: [
    BrowserModule,
    FormsModule,
    ReactiveFormsModule,
    MaterialComponentsModule,
    RouterModule,
    ProgressBarModule,
    ConceptTagModule,
    CardsSliderModule,
    TrackCalendarModule,
    ListSearchModule
  ],
  providers: [
    SettingsUsersService
  ],
  exports: [
    SettingsUserDetailsProgressComponent
  ]
})
export class SettingsUserDetailsModule { }
