import { Component, Input, Output, EventEmitter } from '@angular/core';
import { TrackEvent } from '../../../../../../models/track-event.model';

@Component({
  selector: 'app-track-event-card',
  template: `
  <div class="track-event-card" >
    <div class="content" >
      <h3>{{ trackEvent.title }}</h3>
      <img src="./assets/img/trash.png"
        (click)="removeEvent.emit(index)"
      />
    </div>
  </div>`,
  styleUrls: ['./track-event-card.component.scss']
})
export class TrackEventCardComponent {

  @Input() readonly trackEvent: TrackEvent;
  @Input() readonly index: number;
  @Output() removeEvent = new EventEmitter<number>();

}
