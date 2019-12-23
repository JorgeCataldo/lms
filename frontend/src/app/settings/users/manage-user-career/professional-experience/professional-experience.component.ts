import { Component, Input, Output, EventEmitter } from '@angular/core';
import { MatSnackBar } from '@angular/material';
import { FormGroup, FormArray } from '@angular/forms';
import { NotificationClass } from 'src/app/shared/classes/notification';

@Component({
  selector: 'app-professional-experience',
  templateUrl: './professional-experience.component.html',
  styleUrls: ['../manage-user-career.component.scss']
})
export class ProfessionalExperienceComponent extends NotificationClass {

  @Input() formGroup: FormGroup;
  @Output() addExperience = new EventEmitter();

  constructor(
    protected _snackBar: MatSnackBar
  ) {
    super(_snackBar);
  }

  public selectProfessionalExperience(value: boolean) {
    this.formGroup.get('professionalExperience').setValue(value);
  }

  public removeForm(key: string, index: number) {
    const formArray = this.formGroup.get(key) as FormArray;
    formArray.removeAt(index);
  }
}
