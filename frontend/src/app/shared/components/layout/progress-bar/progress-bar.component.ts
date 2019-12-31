import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-progress-bar',
  template: `
    <div class="progress-bar"
      [ngClass]="{ 'rounded': roundedBorder }"
      [style.height]="height + 'px'"
    >
      <div
        class="completed"
        [style.width]="completedPercentage + '%'"
      ></div>
    </div>`,
  styleUrls: ['./progress-bar.component.scss']
})
export class ProgressBarComponent {

  @Input() completedPercentage: number;
  @Input() height?: number = 22;
  @Input() roundedBorder?: boolean = false;
  @Input() color?: string = '#80D1DC';
  @Input() fillingColor?: string = '#E8E8E8';

}
