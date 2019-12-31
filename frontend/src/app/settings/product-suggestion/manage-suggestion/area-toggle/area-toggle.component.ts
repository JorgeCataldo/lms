import { Component, Output, EventEmitter, Input } from '@angular/core';

@Component({
  selector: 'app-suggestion-area-toggle',
  template: `
    <h2 class="toggle" >
    {{ title }}
    <span>
      <img src="./assets/img/arrow-back.png"
        (click)="toggle.emit()"
        [ngClass]="{ 'closed': isClosed }"
      />
    </span>
  </h2>`,
  styleUrls: ['./area-toggle.component.scss']
})
export class SuggestionAreaToggleComponent {

  @Input() readonly title: string;
  @Input() readonly isClosed: boolean;
  @Output() toggle: EventEmitter<boolean> = new EventEmitter();

}
