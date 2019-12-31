import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-card-tag',
  template: `<p class="recommended" >{{ text }}</p>`,
  styleUrls: ['./card-tag.component.scss']
})
export class CardTagComponent {
  @Input() text: string;
}
