import { Component, Input, EventEmitter, Output } from '@angular/core';
import { MatSnackBar } from '@angular/material';
import { FormGroup, FormArray } from '@angular/forms';
import { NotificationClass } from 'src/app/shared/classes/notification';

@Component({
  selector: 'app-complementary-info',
  templateUrl: './complementary-info.component.html',
  styleUrls: ['../manage-user-career.component.scss']
})
export class CareerComplementaryInfoComponent extends NotificationClass {

  @Input() formGroup: FormGroup;
  @Input() travelAvailability: boolean;
  @Input() movingAvailability: boolean;
  @Output() addReward = new EventEmitter();
  @Output() addCertificate = new EventEmitter();
  @Output() addLanguage = new EventEmitter();
  @Output() addSkill = new EventEmitter();
  @Output() selectTravelAvailability = new EventEmitter<boolean>();
  @Output() selectMovingAvailability = new EventEmitter<boolean>();

  constructor(
    protected _snackBar: MatSnackBar
  ) {
    super(_snackBar);
  }

  public cleanLevel(index: number): void {
    const formArray = this.formGroup.get('fixedLanguages') as FormArray;
    const formControl = formArray.controls[index];

    if (!(formControl.get('languages').value === 'outro') || (formControl.get('languages').value === 'outro'))
      formControl.get('level').setValue('');
  }

  public removeForm(key: string, index: number) {
    const formArray = this.formGroup.get(key) as FormArray;
    formArray.removeAt(index);
  }
}
