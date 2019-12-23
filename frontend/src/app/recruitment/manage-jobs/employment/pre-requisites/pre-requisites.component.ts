import { Component, Input, EventEmitter, Output } from '@angular/core';
import { MatSnackBar } from '@angular/material';
import { FormGroup, FormArray } from '@angular/forms';
import { NotificationClass } from 'src/app/shared/classes/notification';

@Component({
  selector: 'app-pre-requisites',
  templateUrl: './pre-requisites.component.html',
  styleUrls: ['../employment.component.scss']
})
export class PreRequisitesComponent extends NotificationClass {

  @Input() formGroup: FormGroup;
  @Output() addLanguage = new EventEmitter();
  @Output() addOthers = new EventEmitter();

  public professionalExperience: boolean = false;

  constructor(
    protected _snackBar: MatSnackBar
  ) {
    super(_snackBar);
  }

  public cleanLevel(index: number): void {
    const formArray = this.formGroup.get('preRequirements').get('complementaryInfo') as FormArray;
    const formControl = formArray.controls[index];

    if (!formControl.get('done').value)
      formControl.get('level').setValue('');
  }

  public removeLanguageForm(key: string, index: number) {
    const formArray = this.formGroup.get('preRequirements').get('languageInfo') as FormArray;
    formArray.removeAt(index);
  }

  public removeOthersForm(key: string, index: number) {
    const formArray = this.formGroup.get('preRequirements').get('others') as FormArray;
    formArray.removeAt(index);
  }
}
