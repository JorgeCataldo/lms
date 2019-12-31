import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { NgUtilModule } from '@tg4/ng-util';
import { RouterModule } from '@angular/router';
import { SettingsEventsComponent } from './events.component';
import { MaterialComponentsModule } from '../../shared/material.module';
import { SettingsEventCardComponent } from './event-card/event-card.component';
import { SettingsNewEventModule } from './new-event/new-event.module';
import { PaginationModule } from '../../shared/components/pagination/pagination.module';
import { SettingsEventDetailsComponent } from './event-details/event-details.component';
import { ModuleSidebarModule } from 'src/app/shared/components/module-sidebar/module-sidebar.module';
import { ClassroomLessonHeaderModule } from 'src/app/pages/classroom-lesson/lesson-header/lesson-header.module';
import { ClassroomLessonRequiredModule } from 'src/app/pages/classroom-lesson/required-module/required-module.module';
import { SettingsNextEventCardComponent } from './event-details/next-event-card/next-event-card.component';
import { SettingsPastEventCardComponent } from './event-details/past-event-card/past-event-card.component';
import { DeleteEventDialogComponent } from './delete-event/delete-event.dialog';
import { ContentEventsService } from 'src/app/pages/_services/events.service';
import { SettingsEventResultsComponent } from './event-results/event-results.component';
import { SettingsEventsDraftsService } from '../_services/events-drafts.service';

@NgModule({
  declarations: [
    SettingsEventsComponent,
    SettingsEventCardComponent,
    SettingsEventDetailsComponent,
    SettingsEventResultsComponent,
    SettingsNextEventCardComponent,
    SettingsPastEventCardComponent,
    DeleteEventDialogComponent
  ],
  imports: [
    BrowserModule,
    RouterModule,
    MaterialComponentsModule,
    FormsModule,
    ReactiveFormsModule,
    NgUtilModule,
    SettingsNewEventModule,
    PaginationModule,
    ModuleSidebarModule,
    ClassroomLessonHeaderModule,
    ClassroomLessonRequiredModule
  ],
  entryComponents: [
    DeleteEventDialogComponent
  ],
  providers: [
    ContentEventsService,
    SettingsEventsDraftsService
  ]
})
export class SettingsEventsModule { }
