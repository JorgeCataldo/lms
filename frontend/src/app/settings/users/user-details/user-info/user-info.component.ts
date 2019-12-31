import { Component, Input } from '@angular/core';
import { User } from 'src/app/models/user.model';

@Component({
  selector: 'app-user-info',
  templateUrl: './user-info.component.html',
  styleUrls: ['./user-info.component.scss']
})
export class SettingsUserInfoComponent {

  @Input() readonly user: User;
  @Input() userCalendar: boolean = false;
  @Input() readonly showBasicData: boolean;

  public documentType = {
    '1': 'RG',
    '2': 'CNH'
  };

}
