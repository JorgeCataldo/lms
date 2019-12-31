import { Component, Input } from '@angular/core';
import { EventPreview } from '../../../models/previews/event.interface';

@Component({
  selector: 'app-event-card',
  template: `
    <div class="event-card"
      [style.backgroundImage]="'url('+ event.imageUrl +')'"
    >
      <p class="date" *ngIf="showDate" >
        Data do curso<br>
        <span>{{ event.date ? (event.date | date : 'dd/MM/yyyy') : '--' }}</span>
      </p>
      <div class="content" >
        <p class="title" >{{ event.title }}</p>
        <p class="subtitle" >{{ event.instructor }}</p>
      </div>
      <p class="requirements" >
        <span>4 de 4</span> requisitos
      </p>
    </div>
    <div class="bg-shadow" ></div>`,
  styleUrls: ['./event-card.component.scss']
})
export class EventCardComponent {

  @Input() readonly event: EventPreview;
  @Input() readonly showDate: boolean = true;

}
