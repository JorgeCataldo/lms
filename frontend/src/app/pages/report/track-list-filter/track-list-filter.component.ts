import { Component, Input, Output, EventEmitter } from '@angular/core';
import { TrackPreview } from 'src/app/models/previews/track.interface';

@Component({
  selector: 'app-track-list-filter',
  template: `
    <div class="track-card" >
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
  styleUrls: ['./track-list-filter.component.scss']
})
export class TrackListFilterComponent {

  @Input() track: TrackPreview;
  @Input() showImage: boolean;
  @Output() updateCollection = new EventEmitter();

}
