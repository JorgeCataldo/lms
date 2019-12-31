import { Component, Input, Output, EventEmitter } from '@angular/core';
import { TrackPreview } from 'src/app/models/previews/track.interface';

@Component({
  selector: 'app-settings-track-card-valuation-test',
  template: `
    <div class="track-card" >
      <img class="main-img" [src]="track.imageUrl" />

      <div class="preview" >
        <div>
          <h3>{{ track.title }}</h3>
        </div>
      </div>

      <div class="edit" >
        <mat-checkbox
          [(ngModel)]="track.checked"
          (ngModelChange)="updateCollection.emit()"
        ></mat-checkbox>
      </div>
    </div>
  `,
  styleUrls: ['./track-card-valuation-test.component.scss']
})
export class SettingsTrackCardValuationTestComponent {

  @Input() track: TrackPreview;
  @Output() updateCollection = new EventEmitter();

}
