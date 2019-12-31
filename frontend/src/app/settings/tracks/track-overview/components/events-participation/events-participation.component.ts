import { Component, Input } from '@angular/core';
import { TrackOverview, TrackStudentOverview, AttendedEvent } from 'src/app/models/track-overview.interface';

@Component({
  selector: 'app-track-overview-events-participation',
  template: `
    <div class="events" >
      <p class="title" >
        PARTICIPAÇÃO EVENTOS
      </p>
      <p class="no-event" *ngIf="events && events.length === 0" >
        Este aluno não está inscrito em nenhum evento.
      </p>
      <table *ngIf="events && events.length > 0" >
        <thead>
          <tr>
            <th>Nome do Evento</th>
            <th>Data</th>
            <th>Status</th>
            <th>Aproveitamento</th>
          </tr>
        </thead>
        <tbody>
          <tr *ngFor="let event of events" >
            <td width="40%" >
              {{ event.title }}
            </td>
            <td width="20%" >
              {{ event.eventDate | date : 'dd/MM/yyyy' }}
            </td>
            <td width="20%" >
              <p [ngClass]="{
                'finished': event.presence === true,
                'no-show': event.presence === false
              }" >
                {{ getEventStatus(event) }}
              </p>
            </td>
            <td width="20%" >
              {{ event.finalGrade || '--' }}
            </td>
          </tr>
        </tbody>
      </table>
    </div>`,
  styleUrls: ['./events-participation.component.scss']
})
export class TrackOverviewEventsParticipationComponent {

  @Input() readonly events: Array<AttendedEvent> = [];

  public getEventStatus(event: AttendedEvent): string {
    if (event.presence === null) return '--';
    return event.presence ? 'Finalizado' : 'Não Compareceu';
  }

}
