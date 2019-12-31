import { Component, Input } from '@angular/core';
import { TrackOverview, TrackStudentOverview, AttendedEvent } from 'src/app/models/track-overview.interface';

@Component({
  selector: 'app-track-overview-track-participation',
  template: `
    <div class="events" >
      <p class="title" >
        PARTICIPAÇÃO DA TRILHA
      </p>
      <p class="no-event" *ngIf="trackParticipation && trackParticipation.length === 0" >
        Não existe aluno inscrito em nenhum módulo ou evento.
      </p>
      <table >
        <thead>
          <tr *ngIf="!(trackParticipation && trackParticipation.length === 0)">
            <th></th>
            <th>Nome do Aluno</th>
            <th>Nota</th>
          </tr>
        </thead>
        <tbody>
          <tr *ngFor="let track of trackParticipation" >
            <td width="10%" >
              <img class="photo" [src]="track.imageUrl" />
            </td>
            <td width="70%" >
              {{ track.name }}
            </td>
            <td width="20%" >
              {{ track.grade | number: '1.2-2'}}
            </td>
          </tr>
        </tbody>
      </table>
    </div>`,
  styleUrls: ['./track-participation.component.scss']
})
export class TrackOverviewTrackParticipationComponent {

  @Input() readonly trackParticipation: any[] = [];

}
