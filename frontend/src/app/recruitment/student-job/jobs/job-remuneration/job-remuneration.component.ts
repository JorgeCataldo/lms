import { Component, Input } from '@angular/core';
import { UserJobPositionById } from 'src/app/models/previews/user-job-application.interface';

@Component({
  selector: 'app-job-remuneration',
  templateUrl: './job-remuneration.component.html',
  styleUrls: ['./job-remuneration.component.scss']
})
export class JobRemunerationComponent {
  @Input() readonly job: UserJobPositionById;
}
