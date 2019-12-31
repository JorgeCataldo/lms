import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { RouterModule } from '@angular/router';
import { MaterialComponentsModule } from '../../../shared/material.module';
import { ProgressBarModule } from '../../../shared/components/layout/progress-bar/progress-bar.module';
import { SettingsUserRecommendationComponent } from './user-recommendation.component';
import { SettingsUserDetailsSummaryRecommendationComponent } from './user-summary-recommendation/user-summary-recommendation.component';
import { SettingsUsersService } from '../../_services/users.service';
import { ConceptTagModule } from '../../../shared/components/layout/concept-tag/concept-tag.module';
import { CardsSliderModule } from '../../../shared/components/cards-slider/cards-slider.module';
import { TrackCalendarModule } from 'src/app/pages/track/track-overview/track-calendar/track-calendar.module';
import { SettingsUserCareerRecommendationComponent } from './user-career-recommendation/user-career-recommendation.component';
import { SettingsProseekRecommendationComponent } from './proseek-recommendation/proseek-recommendation.component';
import { ProfileRadarComponent } from './proseek-recommendation/profile-radar/profile-radar.component';
import { RecommendationRadarComponent } from './proseek-recommendation/recommendation-radar/recommendation-radar.component';
import { JobRecommendationDialogComponent } from './job-recommendation-dialog/job-recommendation.dialog';
import { RecommendationBarComponent } from './proseek-recommendation/recommendation-bar/recommendation-bar.component';

@NgModule({
  declarations: [
    SettingsUserRecommendationComponent,
    SettingsUserCareerRecommendationComponent,
    SettingsUserDetailsSummaryRecommendationComponent,
    SettingsProseekRecommendationComponent,
    ProfileRadarComponent,
    RecommendationRadarComponent,
    RecommendationBarComponent,
    JobRecommendationDialogComponent
  ],
  imports: [
    BrowserModule,
    MaterialComponentsModule,
    RouterModule,
    ProgressBarModule,
    ConceptTagModule,
    CardsSliderModule,
    TrackCalendarModule
  ],
  providers: [
    SettingsUsersService
  ],
  entryComponents: [
    JobRecommendationDialogComponent
  ]
})
export class SettingsUserRecommendationModule { }
