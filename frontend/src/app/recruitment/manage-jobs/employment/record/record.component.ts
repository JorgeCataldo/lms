import { Component, Input, Output, EventEmitter } from '@angular/core';
import { MatSnackBar } from '@angular/material';
import { FormGroup, FormArray } from '@angular/forms';
import { NotificationClass } from 'src/app/shared/classes/notification';
import { SettingsUsersService } from 'src/app/settings/_services/users.service';
import { Institute } from 'src/app/settings/users/user-models/user-career';

@Component({
  selector: 'app-record',
  templateUrl: './record.component.html',
  styleUrls: ['../employment.component.scss']
})
export class RecordComponent extends NotificationClass {

  @Input() formGroup: FormGroup;

  constructor(
    protected _snackBar: MatSnackBar
  ) {
    super(_snackBar);
  }
}
