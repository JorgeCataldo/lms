import { Component, Input } from '@angular/core';
import { Requirement } from '../../../settings/modules/new-module/models/new-requirement.model';
import { Level } from '../../../models/shared/level.interface';
import { UserService } from '../../_services/user.service';

@Component({
  selector: 'app-event-required-module',
  templateUrl: './required-module.component.html',
  styleUrls: ['./required-module.component.scss']
})
export class EventRequiredModuleComponent {

  @Input() requirement: Requirement;
  @Input() levels: any = {};
  @Input() last: boolean = false;
  @Input() isManagement?: boolean = false;
  @Input() progress: any = {};

  constructor(public userService: UserService) {}
}
