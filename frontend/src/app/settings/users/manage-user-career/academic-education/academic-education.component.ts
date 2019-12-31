import { Component, Input, Output, EventEmitter } from '@angular/core';
import { MatSnackBar } from '@angular/material';
import { FormGroup, FormArray } from '@angular/forms';
import { NotificationClass } from 'src/app/shared/classes/notification';
import { SettingsUsersService } from '../../../_services/users.service';
import { Institute } from '../../user-models/user-career';

@Component({
  selector: 'app-academic-education',
  templateUrl: './academic-education.component.html',
  styleUrls: ['../manage-user-career.component.scss']
})
export class AcademicEducationComponent extends NotificationClass {

  @Input() formGroup: FormGroup;
  @Output() addEducation = new EventEmitter();

  public institutes: Array<Array<Institute>> = [];

  constructor(
    protected _snackBar: MatSnackBar,
    private _settingsUsersService: SettingsUsersService
  ) {
    super(_snackBar);
  }

  public removeForm(key: string, index: number) {
    const formArray = this.formGroup.get(key) as FormArray;
    formArray.removeAt(index);
    if (key === 'colleges')
      this.institutes.pop();
  }

  private _loadColleges(searchValue: string = '', index: number): void {
    this._settingsUsersService.getUserInstitutions(searchValue).subscribe(response => {
      this.institutes[index] = response.data;
    });
  }

  public triggerCollegeSearch(searchValue: string, formGroup: FormGroup, index: number) {
    if (searchValue && searchValue.trim() !== '') {
      formGroup.get('title').setValue(searchValue);
      this._loadColleges(searchValue, index);
    }
  }

  public resetCollegeSearch(index: number): void {
    this.institutes[index] = [];
  }

  public addCollegeIntoForm(college: any, formGroup: FormGroup, index: number) {
    formGroup.get('instituteId').setValue(college.id);
    formGroup.get('title').setValue(college.name);
    this.institutes[index] = [];
  }

  public changePeriodDisabled(formGroup: FormGroup, value: string) {
    if (value === 'Completo') {
      formGroup.get('completePeriod').enable();
    } else {
      formGroup.get('completePeriod').setValue('');
      formGroup.get('completePeriod').disable();
    }
  }
}
