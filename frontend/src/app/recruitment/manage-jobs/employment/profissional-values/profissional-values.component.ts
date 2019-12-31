import { Component, Input, EventEmitter, Output } from '@angular/core';
import { MatSnackBar } from '@angular/material';
import { FormGroup, FormArray } from '@angular/forms';
import { NotificationClass } from 'src/app/shared/classes/notification';

@Component({
  selector: 'app-profissional-values',
  templateUrl: './profissional-values.component.html',
  styleUrls: ['../employment.component.scss']
})
export class ProfissionalValuesComponent extends NotificationClass {

  @Input() formGroup: FormGroup;
  @Output() addReward = new EventEmitter();

  constructor(
    protected _snackBar: MatSnackBar
  ) {
    super(_snackBar);
  }

  public removeForm(key: string, index: number) {
    const formArray = this.formGroup.get(key) as FormArray;
    formArray.removeAt(index);
  }
}
