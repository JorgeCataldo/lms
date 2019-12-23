import { Component, OnInit } from '@angular/core';
import { Track } from '../../models/track.model';
import { EventPreview } from '../../models/previews/event.interface';
import { MatSnackBar } from '@angular/material';
import { ContentTracksService } from '../_services/tracks.service';
import { NotificationClass } from '../../shared/classes/notification';
import { ActivatedRoute, Router } from '@angular/router';
import { UserService } from '../_services/user.service';
import { SharedService } from 'src/app/shared/services/shared.service';
import * as pdfform from 'pdfform.js/pdfform';
import { UtilService } from 'src/app/shared/services/util.service';
import { AuthService } from 'src/app/shared/services/auth.service';
import { HttpClient } from '@angular/common/http';
import { TrackEvent } from 'src/app/models/track-event.model';

@Component({
  selector: 'app-track',
  templateUrl: './track.component.html',
  styleUrls: ['./track.component.scss']
})
export class TrackComponent extends NotificationClass implements OnInit {

  public track: Track;
  private _trackId: string;
  public events: Array<EventPreview> = [];
  levelDict: any = {};
  moduleProgress: any = {};
  trackProgress: any = {};

  constructor(
    protected _snackBar: MatSnackBar,
    private _tracksService: ContentTracksService,
    private _activatedRoute: ActivatedRoute,
    private _userService: UserService,
    private _sharedService: SharedService,
    private _router: Router,
    private _authService: AuthService,
    private _utilService: UtilService,
    private _httpClient: HttpClient
  ) {
    super(_snackBar);
  }

  ngOnInit() {
    this._trackId = this._activatedRoute.snapshot.paramMap.get('trackId');
    this._tracksService.getTrackById(
      this._trackId, true
    ).subscribe((response) => {
      this.track = response.data;

    }, () => this.notify('Ocorreu um erro, por favor tente novamente mais tarde'));
    this._loadProgress();
    this._loadLevels();
  }

  public getCalendarEvents(): Array<TrackEvent> {
    const trackEvents: Array<TrackEvent> = [];

    if (this.track.eventsConfiguration)
      trackEvents.push.apply(trackEvents, this.track.eventsConfiguration);

    if (this.track.calendarEvents)
      trackEvents.push.apply(trackEvents, this.track.calendarEvents);

    return trackEvents;
  }

  public getAttendedEventsCount(): number {
    return this.track.eventsConfiguration.reduce((sum: number, ev) => {
      return ev.hasTakenPart ? sum + 1 : sum;
    }, 0);
  }

  public goToEvent(event) {
    this._router.navigate([ 'evento/' + event.eventId + '/' + event.eventScheduleId ]);
  }

  private _loadLevels(): void {
    this._sharedService.getLevels(true).subscribe((response) => {
      this.levelDict = {};
      response.data.forEach(level => {
        this.levelDict[level.id] = level.description;
      });
    }, () => this.notify('Ocorreu um erro, por favor tente novamente mais tarde'));
  }

  private _loadProgress(): any {
    this._userService.getUserModulesProgress().subscribe((response) => {
      this.moduleProgress = {};
      response.data.forEach(x => {
        this.moduleProgress[x.moduleId] = x;
      });

    }, () => {
      this.notify('Ocorreu um erro, por favor tente novamente mais tarde');
    });

    this._userService.getUserTrackProgress(this._trackId).subscribe((response) => {
      this.trackProgress = response.data;
    }, () => {
      this.notify('Ocorreu um erro, por favor tente novamente mais tarde');
    });
  }

  public generateCertificatePDF(): void {
    this._httpClient.get(
      this.track.certificateUrl, { responseType: 'arraybuffer' }
    ).subscribe(
      (response) => {
        const fields = { };
        fields['nome_trilha'] = [ this.track.title ];
        fields['nome_aluno'] = [ this._authService.getLoggedUser().name ];
        fields['data_conclusao'] = [ this._utilService.formatDateToDDMMYYYY(new Date())];
        fields['data_conclusao_extenso'] = [ this._utilService.formatDateToName(new Date())];
        const out_buf = pdfform().transform(response, fields);

        const blob = new Blob([out_buf], { type: 'application/pdf' });
        const fileURL = URL.createObjectURL(blob);
        window.open(fileURL);
    }, () => {
      this.notify('Ocorreu um erro ao carregar o certificado');
    });
  }
}
