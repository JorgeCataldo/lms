import { Component, Output, EventEmitter, Input } from '@angular/core';

@Component({
  selector: 'app-track-mandatory-video',
  template: `
    <div class="mandatory" >
      <div>
        <p class="mandatory-title"
          [ngStyle]="getTitleColors()" >
          VÍDEO INTRODUTÓRIO OBRIGATÓRIO
        </p>
        <p class="mandatory-subtitle" >
          {{ seen ? 'Conteúdo da trilha desbloqueado' :
          'Veja o vídeo para desbloquear o conteúdo da trilha'}}
        </p>
      </div>
      <button class="btn-test mandatory-btn"
        [ngStyle]="getBtnColors()"
        (click)="watch()" >
        Ver
      </button>
    </div>
    <div class="locked-track" *ngIf="!seen">
      <p>TRILHA</p>
      <img src="./assets/img/lock-icon.png" />
    </div>`,
  styleUrls: ['./track-mandatory-video.component.scss']
})
export class TrackMandatoryVideoComponent {

  @Input() readonly seen: boolean = false;
  @Output() watchVideo = new EventEmitter();

  constructor() { }

  public watch() {
    this.watchVideo.emit(true);
  }

  public getTitleColors() {
    if (this.seen) {
      return {
        'color': '#06e295'
      };
    } else {
      return {
        'color': '#ff4376'
      };
    }
  }

  public getBtnColors() {
    if (this.seen) {
      return {
        'background-color': '#06e295'
      };
    } else {
      return {
        'background-color': '#ff4376'
      };
    }
  }
}
