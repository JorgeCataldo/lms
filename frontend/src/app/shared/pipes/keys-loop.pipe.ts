import { Pipe, PipeTransform } from '@angular/core';

@Pipe({ name: 'keysLoop' })
export class KeysLoopPipe implements PipeTransform {

  transform(obj) {
    return Object.keys( obj );
  }

}
