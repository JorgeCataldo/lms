import { Component, Input } from '@angular/core';
import { UserJobPositionById } from 'src/app/models/previews/user-job-application.interface';

@Component({
  selector: 'app-job-activities',
  templateUrl: './job-activities.component.html',
  styleUrls: ['./job-activities.component.scss']
})
export class JobActivitiesComponent {
  @Input() readonly job: UserJobPositionById;
}
