import { Component, Input, Output, EventEmitter } from '@angular/core';
import { TrackPreview } from '../../../../models/previews/track.interface';

@Component({
  selector: 'app-settings-track-card-select',
  template: `
    <div class="track-card" >
      <img class="main-img" [src]="track.imageUrl" />

      <div class="preview" >
        <div>
          <h3>{{ track.title }}</h3>
        </div>
        <p>{{ track.moduleCount }} MÓDULOS / {{ track.eventCount }} EVENTOS</p>
      </div>

      <div class="edit" >
        <mat-checkbox
          [(ngModel)]="track.checked"
          (ngModelChange)="updateCollection.emit()"
        ></mat-checkbox>
      </div>
    </div>
  `,
  styleUrls: ['./track-card-select.component.scss']
})
export class SettingsTrackCardSelectComponent {

  @Input() track: TrackPreview;
  @Output() updateCollection = new EventEmitter();

}
