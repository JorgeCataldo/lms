import { NgModule } from '@angular/core';
import { SafeUrlPipe } from './safe-url.pipe';
import { LoopPipe } from './loop.pipe';
import { KeysLoopPipe } from './keys-loop.pipe';
import { PercentagePipe } from './percentage.pipe';

@NgModule({
  declarations: [
    SafeUrlPipe,
    LoopPipe,
    KeysLoopPipe,
    PercentagePipe
  ],
  exports: [
    SafeUrlPipe,
    LoopPipe,
    KeysLoopPipe,
    PercentagePipe
  ]
})
export class PipesModule { }
