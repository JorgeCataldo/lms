import { Component, Input } from '@angular/core';
import { MatSnackBar } from '@angular/material';
import { FormGroup, FormArray } from '@angular/forms';
import { NotificationClass } from 'src/app/shared/classes/notification';

@Component({
  selector: 'app-remuneration',
  templateUrl: './remuneration-benefits.component.html',
  styleUrls: ['../employment.component.scss']
})
export class RemunerationBenefitsComponent extends NotificationClass {

  @Input() formGroup: FormGroup;

  constructor(
    protected _snackBar: MatSnackBar
  ) {
    super(_snackBar);
  }

  public cleanLevel(index: number): void {
    const formArray = this.formGroup.get('benefits').get('complementaryBenefits') as FormArray;
    const formControl = formArray.controls[index];

    if (!formControl.get('done').value) {
      formControl.get('level').setValue('');
      formControl.get('level').disable();
    } else {
      formControl.get('level').enable();
    }
  }
}
