import { Component, Input, EventEmitter, Output } from '@angular/core';
import { MatSnackBar } from '@angular/material';
import { FormGroup, FormArray } from '@angular/forms';
import { NotificationClass } from 'src/app/shared/classes/notification';

@Component({
  selector: 'app-complementary-experience',
  templateUrl: './complementary-experience.component.html',
  styleUrls: ['../manage-user-career.component.scss']
})
export class CareerComplementaryExperienceComponent extends NotificationClass {

  @Input() formGroup: FormGroup;
  @Output() addAbility = new EventEmitter();

  constructor(
    protected _snackBar: MatSnackBar
  ) {
    super(_snackBar);
  }

  public cleanLevel(index: number): void {
    const formArray = this.formGroup.get('fixedAbilities') as FormArray;
    const formControl = formArray.controls[index];

    if (!formControl.get('hasLevel').value)
      formControl.get('level').setValue('');
  }

  public removeForm(key: string, index: number) {
    const formArray = this.formGroup.get(key) as FormArray;
    formArray.removeAt(index);
  }
}
