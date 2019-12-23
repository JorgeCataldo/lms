import { Component, Output, EventEmitter } from '@angular/core';
import { NotificationClass } from '../../classes/notification';
import { MatSnackBar } from '@angular/material';

@Component({
  selector: 'app-date-range-picker',
  template: `
    <div class="range-picker" >
      <mat-form-field>
        <input matInput
          [matDatepicker]="fromPicker"
          placeholder="De (dd/mm/aaaa)"
          [(ngModel)]="fromDate"
          (dateChange)="checkDates()"
        />
        <mat-datepicker-toggle
          matSuffix [for]="fromPicker"
        ></mat-datepicker-toggle>
        <mat-datepicker
          #fromPicker
        ></mat-datepicker>
      </mat-form-field>
      <mat-form-field>
        <input matInput
          [matDatepicker]="toPicker"
          placeholder="Até (dd/mm/aaaa)"
          [(ngModel)]="toDate"
          (dateChange)="checkDates()"
        />
        <mat-datepicker-toggle
          matSuffix [for]="toPicker"
        ></mat-datepicker-toggle>
        <mat-datepicker
          #toPicker
        ></mat-datepicker>
      </mat-form-field>
    </div>`,
  styleUrls: ['./date-range-picker.component.scss']
})
export class DateRangePickerComponent extends NotificationClass {

  @Output() rangeSelected: EventEmitter<Array<Date>> = new EventEmitter();

  public fromDate: Date;
  public toDate: Date;

  constructor(
    protected _snackBar: MatSnackBar
  ) {
    super(_snackBar);
  }

  public checkDates(): void {
    if (this.fromDate && this.toDate) {
      if (this.fromDate > this.toDate) {
        this.notify('A data final não pode ser maior do que a inicial');
        this.toDate = null;
      } else {
        this.rangeSelected.emit(
          [ this.fromDate, this.toDate ]
        );
      }
    }
  }

}
