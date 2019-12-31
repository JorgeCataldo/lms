import { Component, Input } from '@angular/core';
import { MatSnackBar } from '@angular/material';
import { FormGroup } from '@angular/forms';
import { NotificationClass } from 'src/app/shared/classes/notification';

@Component({
  selector: 'app-professional-objectives',
  templateUrl: './professional-objectives.component.html',
  styleUrls: ['../manage-user-career.component.scss']
})
export class ProfessionalObjectivesComponent extends NotificationClass {

  @Input() formGroup: FormGroup;

  constructor(
    protected _snackBar: MatSnackBar
  ) {
    super(_snackBar);
  }

}
