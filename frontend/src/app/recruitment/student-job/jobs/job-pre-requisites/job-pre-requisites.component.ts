import { Component, Input } from '@angular/core';
import { UserJobPositionById } from 'src/app/models/previews/user-job-application.interface';

@Component({
  selector: 'app-job-pre-requisites',
  templateUrl: './job-pre-requisites.component.html',
  styleUrls: ['./job-pre-requisites.component.scss']
})
export class JobPreRequisitesComponent {
  @Input() readonly job: UserJobPositionById;
}
