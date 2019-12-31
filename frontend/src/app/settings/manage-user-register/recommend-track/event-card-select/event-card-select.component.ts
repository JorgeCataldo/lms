import { Component, Input, Output, EventEmitter } from '@angular/core';
import { EventPreview } from 'src/app/models/previews/event.interface';
import { EventSchedule } from 'src/app/models/event-schedule.model';

@Component({
  selector: 'app-settings-event-card-select',
  template: `
    <div class="event-card" >
      <img class="main-img" [src]="event.imageUrl" />

      <div class="preview" >
        <div>
          <h3>{{ event.title }}</h3>
          <p *ngIf="event.requirements && event.requirements.length > 0" >
            {{ event.requirements.length + ' pré-requisito(s) será(ão) associado(s)' }}
          </p>
        </div>
        <div>
          <p class="label" >Data do Evento</p>
          <mat-select [(ngModel)]="selectedSchedule" >
            <mat-option *ngFor="let schedule of event.schedules"
              [value]="schedule" >
              {{ schedule.eventDate | date : 'dd/MM/yyyy' }}
            </mat-option>
          </mat-select>
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
  styleUrls: ['./event-card-select.component.scss']
})
export class SettingsEventCardSelectComponent {

  @Input() set setEvent(setEvent: EventPreview) {
    this.event = setEvent;
    this.selectedSchedule = this.event.schedules[0];
  }
  @Output() updateCollection: EventEmitter<EventSchedule> = new EventEmitter();

  public event: EventPreview;
  public selectedSchedule: EventSchedule;

  public updateEvent(): void {
    this.selectedSchedule.eventId = this.event.id;
    this.selectedSchedule.eventTitle = this.event.title;
    this.updateCollection.emit( this.selectedSchedule );
  }

}
