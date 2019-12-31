import { Component, Input, Output, EventEmitter } from '@angular/core';
import { TrackPreview } from '../../../models/previews/track.interface';
import { AuthService } from 'src/app/shared/services/auth.service';

@Component({
  selector: 'app-settings-track-card',
  template: `
    <div class="track-card" >
      <img class="main-img" [src]="track.imageUrl" />

      <div class="preview" >
        <div>
          <h3>
            {{ track.title }}<br>
            <small *ngIf="isAdmin" >
              Id: {{ track.id }}
            </small>
          </h3>
        </div>
        <p>{{ track.moduleCount }} MÓDULOS / {{ track.eventCount }} EVENTOS</p>
      </div>

      <div class="edit" >
        <img src="./assets/img/edit.png" (click)="editTrack.emit(track)" />
        <img style="margin-top: 24px;" src="./assets/img/trash.png" (click)="deleteTrack.emit(track)" />
      </div>
    </div>
  `,
  styleUrls: ['./track-card.component.scss']
})
export class SettingsTrackCardComponent {

  @Input() track: TrackPreview;
  @Output() editTrack = new EventEmitter<TrackPreview>();
  @Output() deleteTrack = new EventEmitter<TrackPreview>();

  public isAdmin: boolean = false;

  constructor(
    private _authService: AuthService
  ) {
    const user = this._authService.getLoggedUser();
    this.isAdmin = user && user.role && user.role === 'Admin';
  }

}
