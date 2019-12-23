import { Pipe, PipeTransform } from '@angular/core';

@Pipe({ name: 'asPercentage' })
export class PercentagePipe implements PipeTransform {

  transform(percentage: number) {
    return Math.round(percentage * 100);
  }

}
