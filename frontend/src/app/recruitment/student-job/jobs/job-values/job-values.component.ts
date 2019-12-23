import { Component, OnInit, Input } from '@angular/core';
import { NotificationClass } from '../../../../shared/classes/notification';
import { UserJobPositionById } from 'src/app/models/previews/user-job-application.interface';

@Component({
  selector: 'app-job-values',
  templateUrl: './job-values.component.html',
  styleUrls: ['./job-values.component.scss']
})
export class JobValuesComponent {
  @Input() readonly job: UserJobPositionById;
}
