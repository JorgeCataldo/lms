import { Component, Input, Output, EventEmitter } from '@angular/core';
import { EventSchedule } from 'src/app/models/event-schedule.model';

@Component({
  selector: 'app-settings-past-event-card',
  template: `
    <div class="past-event-card" >
      <div class="content" >
        <p class="header" >
          {{ schedule.eventDate | date : 'dd/MM/yyyy' }}
        </p>
        <p *ngIf="finished && false" class="progress" >
          TODO: Pegar da média
          Aproveitamento Médio <span>80%</span>
        </p>
        <p *ngIf="!finished" class="progress" >
          Avaliação pendente
        </p>
      </div>
      <div class="buttons" >
        <button (click)="manageApplications.emit(schedule.id)">
          VER LISTA DE ALUNOS
        </button>
        <button class="alt" (click)="viewResults.emit(schedule.id)">
          VER AVALIAÇÃO
        </button>
      </div>
    </div>`,
  styleUrls: ['./past-event-card.component.scss']
})
export class SettingsPastEventCardComponent {

  @Input() readonly schedule: EventSchedule;
  @Input() readonly finished: boolean = false;
  @Output() manageApplications = new EventEmitter<string>();
  @Output() viewResults = new EventEmitter<string>();

}
