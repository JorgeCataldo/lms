import { Injectable } from '@angular/core';
import { FormGroup } from '@angular/forms';

@Injectable()
export class UtilService {

  public formatDurationToHour(duration: number): string {
    let formattedHour = '';
    let remainingDuration = duration;

    const hours = Math.floor(duration / 3600);
    formattedHour = this._padStart(hours.toString(), 2, '0') + ':';
    remainingDuration = remainingDuration - (hours * 3600);

    const minutes = Math.floor(remainingDuration / 60);
    formattedHour = formattedHour + this._padStart(minutes.toString(), 2, '0') + ':';
    remainingDuration = remainingDuration - (minutes * 60);
    return formattedHour + this._padStart(remainingDuration.toString(), 2, '0');
  }

  public formatMinutesToHour(minutes: number = 0): string {
    let formattedHour = '';
    let remaining = minutes;

    const hours = Math.floor(minutes / 60);
    formattedHour = this._padStart(hours.toString(), 2, '0') + ':';
    remaining = remaining - (hours * 60);

    return formattedHour + this._padStart(remaining.toString(), 2, '0');
  }

  public formatSecondsToMinutes(seconds: number = 0): string {
    let formatted = '';
    let remaining = seconds;

    const minutes = Math.floor(seconds / 60);
    formatted = this._padStart(minutes.toString(), 2, '0') + ':';
    remaining = remaining - (minutes * 60);

    return formatted + this._padStart(remaining.toString(), 2, '0');
  }

  public formatSecondsToVideoPosition(seconds: number = 0): string {
    let formatted = '';
    let remaining = seconds;

    const minutes = Math.floor(seconds / 60);
    formatted = this._padStart(minutes.toString(), 2, '0') + 'm';
    remaining = remaining - (minutes * 60);

    return formatted + this._padStart(remaining.toString(), 2, '0') + 's';
  }

  public formatSecondsToHourMinute(seconds: number = 0): string {
    let formatted = '';
    let remaining = seconds;

    const hours = Math.floor(seconds / 3600);
    formatted = this._padStart(hours.toString(), 2, '0') + ':';
    remaining = remaining - (hours * 3600);

    const minutes = Math.floor(remaining / 60);
    formatted = formatted + this._padStart(minutes.toString(), 2, '0');
    remaining = remaining - (minutes * 60);

    return formatted;
  }

  public formatSecondsToHour(seconds: number = 0): string {
    let formatted = '';
    let remaining = seconds;

    const hours = Math.floor(seconds / 3600);
    formatted = this._padStart(hours.toString(), 2, '0');
    remaining = remaining - (hours * 3600);

    return formatted;
  }

  public getDurationFromFormattedHour(formatted: string): number {
    formatted = formatted.split(':').join('');
    const hours = parseInt(formatted.substring(0, 2), 10);
    const minutes = parseInt(formatted.substring(2, 4), 10);
    const seconds = parseInt(formatted.substring(4, 6), 10);
    return (hours * 3600) + (minutes * 60) + seconds;
  }

  public markFormControlsAsTouch(formGroup: FormGroup): FormGroup {
    Object.keys(formGroup.controls).forEach((key: string) => {
      formGroup.controls[key].markAsTouched();
    });
    return formGroup;
  }

  public removeDuplicates(array: Array<any>, prop: string): Array<any> {
    return array.filter((item, pos, arr) => {
      return !pos || item[prop] !== arr[pos - 1][prop];
    });
  }

  public formatDateToDDMMYYYY(dateTime: Date): string {
    const mm = dateTime.getMonth() + 1;
    const dd = dateTime.getDate();

    return [(dd > 9 ? '' : '0') + dd,
      (mm > 9 ? '' : '0') + mm,
      dateTime.getFullYear()
    ].join('/');
  }

  public formatDateToName(dateTime: Date): string {
    const months: Array<string> = ['Janeiro', 'Fevereiro', 'Março', 'Abril' , 'Maio' , 'Junho', 'Julho',
    'Agosto', 'Setembro', 'Outubro', 'Novembro', 'Dezembro'];
    const mm = dateTime.getMonth();
    const dd = dateTime.getDate();

    return (dd > 9 ? '' : '0') + dd.toString() + ' de ' + months[mm] + ' de ' + dateTime.getFullYear().toString();
  }

  private _padStart(str: string, maxLength: number, fillString: string = ' '): string {
    if (str.length >= maxLength)
      return str;

    const fillLen = maxLength - str.length;
    const timesToRepeat = Math.ceil(fillLen / fillString.length);
    const truncatedStringFiller = fillString
        .repeat(timesToRepeat)
        .slice(0, fillLen);

    return truncatedStringFiller + str;
  }
}
