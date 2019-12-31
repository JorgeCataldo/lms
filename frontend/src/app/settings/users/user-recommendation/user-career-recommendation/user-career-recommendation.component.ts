import { Component, Input } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { UserCareer } from '../../user-models/user-career';

@Component({
  selector: 'app-user-career-recommendation',
  templateUrl: './user-career-recommendation.component.html',
  styleUrls: ['./user-career-recommendation.component.scss']
})
export class SettingsUserCareerRecommendationComponent {
  @Input() career: UserCareer;
}
