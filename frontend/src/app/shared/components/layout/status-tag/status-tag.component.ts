import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-status-tag',
  template: `
    <p class="status-tag" [style.backgroundColor]="backgroundColor" >
      {{ status }}
    </p>`,
  styleUrls: ['./status-tag.component.scss']
})
export class StatusTagComponent {

  @Input() status: string;
  @Input() backgroundColor: string = '#06e295';

}
