import { Component, Input } from '@angular/core';
import { Track } from '../../../models/track.model';
import { UserProgress } from 'src/app/models/subject.model';

@Component({
  selector: 'app-track-header',
  template: `
    <div class="track-header" >
      <div class="header-container" [style.backgroundImage]="'url('+ track?.imageUrl +')'" >
        <div class="content" >
          <h3>Trilha de Conhecimento</h3>
          <h2>{{ track?.title }}</h2>

          <p class="conclusion" >
            <span>{{ progress?.progress*100 | number:'1.0-0' }}%</span>
            PROGRESSO
          </p>
        </div>
      </div>

      <div class="shadow" ></div>
    </div>

    <app-progress-bar
      [completedPercentage]="progress?.progress*100"
      [height]="22"
    ></app-progress-bar>`,
  styleUrls: ['./track-header.component.scss']
})
export class TrackHeaderComponent {

  @Input() track: Track;
  @Input() progress: UserProgress;

}
