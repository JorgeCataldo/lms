import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { SearchResultsComponent } from './search-results.component';
import { TrackCardModule } from '../../shared/components/track-card/track-card.module';
import { ModuleCardModule } from '../../shared/components/module-card/module-card.module';
import { PaginationModule } from '../../shared/components/pagination/pagination.module';

@NgModule({
  declarations: [
    SearchResultsComponent
  ],
  imports: [
    BrowserModule,
    ModuleCardModule,
    TrackCardModule,
    PaginationModule
  ]
})
export class SearchResultsModule { }
