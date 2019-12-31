import { Component, Input, Output, EventEmitter } from '@angular/core';
import { TrackPreview } from '../../../models/previews/track.interface';
import { AuthService } from 'src/app/shared/services/auth.service';
import { UtilService } from 'src/app/shared/services/util.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-settings-track-my-card',
  template: `
    <div class="track-card"  >
      <img class="main-img" [src]="track.imageUrl" (click)="goTrack.emit(track)"/>

      <div class="preview" (click)="goTrack.emit(track)" >
        <div>
          <h3>
            {{ track.title }}<br>
              <div class="hours">
                <p>{{ getDuration() }}</p>
                <span>hrs</span>
              </div>
          </h3>
        </div>
        <div class="modules">
          <div class="all-track">
            <p>{{ track.moduleCount }}</p>
            <span> módulos&nbsp;</span>
            /<p>&nbsp; {{ track.eventCount }} </p>
            <span> eventos </span>
          </div>
        </div>
      </div>

      <div class="edit" *ngIf='isAdmin === true' >
        <img src="./assets/img/view.png" (click)="sumarryTrack.emit(track)" />
        <img src="./assets/img/edit.png" (click)="editTrack.emit(track)" />
      </div>
    </div>
  `,
  styleUrls: ['./my-track-card.component.scss']
})
export class SettingsMyTrackCardComponent {

  @Input() track: TrackPreview;
  @Output() goTrack = new EventEmitter<TrackPreview>();
  @Output() sumarryTrack = new EventEmitter<TrackPreview>();
  @Output() editTrack = new EventEmitter<TrackPreview>();
  @Output() deleteTrack = new EventEmitter<TrackPreview>();

  public isAdmin: boolean = false;

  constructor(
    private _authService: AuthService,
    private _utilService: UtilService,
    private _router: Router,
  ) {
    const user = this._authService.getLoggedUser();
    this.isAdmin = user && user.role && user.role === 'Admin';
  }

  public getDuration(): string {
    return this._utilService.formatSecondsToHour( this.track.duration );
  }
}
