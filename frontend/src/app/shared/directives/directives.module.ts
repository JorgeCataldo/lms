import { NgModule } from '@angular/core';
import { SaveActionDirective } from './save-action/save-action.directive';
import { AnalyticsService } from '../services/analytics.service';

@NgModule({
  declarations: [
    SaveActionDirective
  ],
  exports: [
    SaveActionDirective
  ],
  providers: [
    AnalyticsService
  ]
})
export class DirectivesModule { }
