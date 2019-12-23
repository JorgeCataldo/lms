import { Component, Input, Output, EventEmitter } from '@angular/core';
import { EventPreview } from 'src/app/models/previews/event.interface';

@Component({
  selector: 'app-suggestion-event-select',
  template: `
    <div class="event-card" >
      <img class="main-img" [src]="event.imageUrl" />

      <div class="preview" >
        <div>
          <h3>{{ event.title }}</h3>
        </div>
      </div>

      <div class="edit" >
        <mat-checkbox
          [(ngModel)]="event.checked"
          (ngModelChange)="updateEvent()"
        ></mat-checkbox>
      </div>
    </div>
  `,
  styleUrls: ['./event-select.component.scss']
})
export class SuggestionEventSelectComponent {

  @Input() set setEvent(setEvent: EventPreview) {
    this.event = setEvent;
  }
  @Output() updateCollection = new EventEmitter();

  public event: EventPreview;

  public updateEvent(): void {
    this.updateCollection.emit();
  }

}
