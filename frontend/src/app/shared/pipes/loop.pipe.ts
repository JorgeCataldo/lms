import { Pipe, PipeTransform } from '@angular/core';

@Pipe({ name: 'doLoop' })
export class LoopPipe implements PipeTransform {

  transform(amount: number) {
    return (
      new Array(amount)
    ).fill(1);
  }

}
