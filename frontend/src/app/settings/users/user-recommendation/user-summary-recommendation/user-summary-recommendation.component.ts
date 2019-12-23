import { Component, Input } from '@angular/core';
import { UserRecommendation } from 'src/app/models/user-recommendation.model';

@Component({
  selector: 'app-user-details-summary-recommendation',
  templateUrl: './user-summary-recommendation.component.html',
  styleUrls: ['./user-summary-recommendation.component.scss']
})
export class SettingsUserDetailsSummaryRecommendationComponent {
  @Input() user: UserRecommendation;
  @Input() dateBirth: number;
}
