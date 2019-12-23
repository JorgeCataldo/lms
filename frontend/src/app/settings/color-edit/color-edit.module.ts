import { NgModule } from '@angular/core';
import { ColorEditComponent } from './color-edit.component';
import { BrowserModule } from '@angular/platform-browser';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MaterialComponentsModule } from 'src/app/shared/material.module';
import { RouterModule } from '@angular/router';
import { ProgressBarModule } from 'src/app/shared/components/layout/progress-bar/progress-bar.module';
import { ConceptTagModule } from 'src/app/shared/components/layout/concept-tag/concept-tag.module';
import { CardsSliderModule } from 'src/app/shared/components/cards-slider/cards-slider.module';
import { TrackCalendarModule } from 'src/app/pages/track/track-overview/track-calendar/track-calendar.module';
import { ListSearchModule } from 'src/app/shared/components/list-search/list-search.module';
import { SettingsUsersService } from '../_services/users.service';

@NgModule({
    declarations: [
        ColorEditComponent
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
        ColorEditComponent
      ]
})
export class ColorEditModule {

}
