import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';

import { HomeSearchComponent } from '../home/search/search.component';
import { TrackCardModule } from '../../shared/components/track-card/track-card.module';
import { RouterModule } from '@angular/router';
import { PaginationModule } from '../../shared/components/pagination/pagination.module';
import { ContentModulesService } from '../_services/modules.service';
import { BaseUrlService, BackendService } from '@tg4/http-infrastructure/dist/src';
import { UrlService } from '../../shared/services/url.service';
import { ModuleCardModule } from '../../shared/components/module-card/module-card.module';
import { ContentEventsService } from '../_services/events.service';
import { ContentTracksService } from '../_services/tracks.service';
import { UserService } from '../_services/user.service';
import { SettingsUserDetailsModule } from 'src/app/settings/users/user-details/user-details.module';
import { NPSDialogModule } from 'src/app/shared/dialogs/nps/nps.dialog.module';
import { MyCoursesComponent } from './my-courses.component';
import { MyCoursesSearchComponent } from './search/search.component';

@NgModule({
  declarations: [
    MyCoursesComponent,
    MyCoursesSearchComponent
  ],
  imports: [
    BrowserModule,
    FormsModule,
    ModuleCardModule,
    TrackCardModule,
    RouterModule,
    PaginationModule,
    HttpClientModule,
    SettingsUserDetailsModule,
  ],
  providers: [
    ContentModulesService,
    ContentEventsService,
    ContentTracksService,
    UserService,
    BackendService,
    { provide: BaseUrlService, useClass: UrlService }
  ]
})
export class MyCoursesModule { }
