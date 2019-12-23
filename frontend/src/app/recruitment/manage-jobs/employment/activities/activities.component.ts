import { Component, Input, Output, EventEmitter } from '@angular/core';
import { MatSnackBar } from '@angular/material';
import { FormGroup, FormArray } from '@angular/forms';
import { NotificationClass } from 'src/app/shared/classes/notification';

@Component({
  selector: 'app-activities',
  templateUrl: './activities.component.html',
  styleUrls: ['../employment.component.scss']
})
export class ActivitiesComponent extends NotificationClass {

  @Input() formGroup: FormGroup;
  @Output() addExperience = new EventEmitter();

  public professionalExperience: boolean = false;

  constructor(
    protected _snackBar: MatSnackBar
  ) {
    super(_snackBar);
  }

  public selectProfessionalExperience(value: boolean) {
    this.professionalExperience = value;
  }

  public removeForm(key: string, index: number) {
    const formArray = this.formGroup.get(key) as FormArray;
    formArray.removeAt(index);
  }
}
