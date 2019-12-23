import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { RouterModule } from '@angular/router';
import { ReportComponent } from './report.component';
import { MaterialComponentsModule } from 'src/app/shared/material.module';
import { FormsModule } from '@angular/forms';
import { ReportAreaToggleComponent } from 'src/app/pages/report/report-area-toggle/report-area-toggle.component';
import { ListSearchModule } from 'src/app/shared/components/list-search/list-search.module';
import { ReportModuleSelectComponent } from './report-module-select/report-module-select.component';
import { ReportEventSelectComponent } from './report-event-select/report-event-select.component';
import { ReportTrackSelectComponent } from './report-track-select/report-track-select.component';
import { ReportProfessionalProfileComponent } from './report-professional-profile/report-professional-profile.component';
import { ReportInformationRegistrationComponent } from './report-information-registration/report-information-registration.component';
import {
  ReportLearningAssessmentObjectsComponent
} from './report-learning-assessment-objects/report-learning-assessment-objects.component';

import { ReportProgramExecutionComponent } from './report-program-execution/report-program-execution.component';
import { ReportResearchComponent } from './report-research/report-research.component';
import { TrackListFilterComponent } from './track-list-filter/track-list-filter.component';
import { ReportsService } from 'src/app/settings/_services/reports.service';

@NgModule({
  declarations: [
    ReportComponent,
    ReportAreaToggleComponent,
    ReportModuleSelectComponent,
    ReportEventSelectComponent,
    ReportInformationRegistrationComponent,
    ReportTrackSelectComponent,
    ReportProfessionalProfileComponent,
    ReportLearningAssessmentObjectsComponent,
    ReportProgramExecutionComponent,
    ReportResearchComponent,
    TrackListFilterComponent
  ],
  imports: [
    BrowserModule,
    RouterModule,
    FormsModule,
    MaterialComponentsModule,
    ListSearchModule
  ],
  providers: [
    ReportsService
  ]
})
export class ReportModule { }
