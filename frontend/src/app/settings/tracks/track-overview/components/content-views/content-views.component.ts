import { Component, Input } from '@angular/core';
import { ViewedContent } from 'src/app/models/track-overview.interface';

@Component({
  selector: 'app-track-overview-content-views',
  template: `
    <div class="content" >
      <p class="title" >
        VISUALIZAÇÃO DE CONTEÚDOS
      </p>
      <ul>
        <li *ngFor="let item of contents; let index = index" >
          <p class="concept" >
            {{ item.contentTitle }}<br>
            <small>{{ contentDictionary[item.contentType] }}</small>
          </p>
          <p class="count" *ngIf="fromTrack">
            {{ item.count }}<span>/{{ studentsCount }}</span>
          </p>
        </li>
      </ul>
      <p class="no-content" *ngIf="contents && contents.length === 0" >
        Ainda não há conteúdos visualizados por {{fromTrack ? 'esta turma' : 'este aluno'}}.
      </p>
    </div>`,
  styleUrls: ['./content-views.component.scss']
})
export class TrackOverviewContentViewsComponent {

  @Input() readonly studentsCount: number = 0;
  @Input() readonly contents: Array<ViewedContent>;
  @Input() readonly fromTrack: boolean = true;

  public contentDictionary = {
    0: 'Vídeo',
    1: 'Texto',
    2: 'PDF'
  };

}
