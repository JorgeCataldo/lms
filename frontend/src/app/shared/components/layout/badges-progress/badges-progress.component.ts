import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-badges-progress',
  template: `
    <div class="unfilled"
      [style.height]="height + 'px'"
      [style.width]="width + 'px'"
    ></div>
    <div class="filled"
      [style.height]="height + 'px'"
      [style.width]="getFilledWidth() + 'px'"
      [style.bottom]="(height + 10) + 'px'"
    ></div>
    <div class="steps"
      [style.width]="(width - offset) + 'px'"
      [style.left]="offset + 'px'"
      [style.bottom]="(height + 41) + 'px'"
    >
      <div class="step"
        *ngFor="let step of stepsArray; let index = index"
        [ngClass]="{ 'filled': index < this.concludedSteps }"
        [style.backgroundImage]="getStepBackgroundIconSrc(index)"
      ></div>
    </div>`,
  styleUrls: ['./badges-progress.component.scss']
})
export class BadgesProgressComponent {

  @Input() readonly height?: number = 24;
  @Input() readonly width?: number = 380;
  @Input() set steps(steps: number) {
    this.stepsArray = new Array(steps).fill(1);
    this.offset = (this.width / (steps + 1) - 20);
  }
  @Input() readonly concludedSteps: number;

  public stepsArray: Array<number>;
  public offset: number;

  public getFilledWidth(): number {
    const correctedOffset = this.offset + 20;
    if (this.stepsArray.length === this.concludedSteps)
      return this.width;
    else
      return (0.5 * correctedOffset) + (correctedOffset * this.concludedSteps);
  }

  public getStepBackgroundIconSrc(index: number): string {
    if (index >= this.concludedSteps)
      return '';

    switch (index) {
      case 0:
        return 'url(/assets/img/pencil-icon-shadow.png)';
      case 1:
        return 'url(/assets/img/glasses-icon-shadow.png)';
      case 2:
        return 'url(/assets/img/brain-icon-shadow.png)';
      case 3:
        return 'url(/assets/img/brain-dark-icon-shadow.png)';
      default:
        return '';
    }
  }

}
