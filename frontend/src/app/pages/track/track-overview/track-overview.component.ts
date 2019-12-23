import { Component, OnInit, Input } from '@angular/core';
import { MatSnackBar, MatDialog } from '@angular/material';
import { ActivatedRoute, Router } from '@angular/router';
import { NotificationClass } from 'src/app/shared/classes/notification';
import { ContentTracksService } from '../../_services/tracks.service';
import { TrackStudentOverview } from 'src/app/models/track-overview.interface';
import { UserService } from '../../_services/user.service';
import * as pdfform from 'pdfform.js/pdfform';
import { UtilService } from 'src/app/shared/services/util.service';
import { AuthService } from 'src/app/shared/services/auth.service';
import { SettingsValuationTestsService } from 'src/app/settings/_services/valuation-tests.service';
import { HttpClient } from '@angular/common/http';
import Player from '@vimeo/player';
import { VideoDialogComponent } from 'src/app/shared/components/video-dialog/video.dialog';
import { ValuationTestTypeEnum, ValuationTestModuleTypeEnum } from 'src/app/models/enums/valuation-test-type-enum';

@Component({
  selector: 'app-track-overview',
  templateUrl: './track-overview.component.html',
  styleUrls: ['./track-overview.component.scss']
})
export class TrackOverviewComponent extends NotificationClass implements OnInit {

  @Input() showHeader: boolean = true;

  public track: TrackStudentOverview;
  public trackTests: any[] = [];
  public player;
  public coursePlayer;
  userTrackInfo: any = {};

  public trackTestsResearch: any[] = [];
  public trackTestsFree: any[] = [];
  public trackTestsOrdered: any[] = [];

  constructor(
    protected _snackBar: MatSnackBar,
    private _activatedRoute: ActivatedRoute,
    private _tracksService: ContentTracksService,
    private _userService: UserService,
    private _authService: AuthService,
    private _utilService: UtilService,
    private _httpClient: HttpClient,
    private _dialog: MatDialog,
    private _router: Router,
    private _testService: SettingsValuationTestsService
  ) {
    super(_snackBar);
  }

  ngOnInit() {
    const trackId = this._activatedRoute.snapshot.paramMap.get('trackId');
    this._tracksService.getTrackCurrentStudentOverview(
      trackId
    ).subscribe((response) => {
      this.track = response.data;
      console.log('this.track -> ', this.track);
      this._loadTrackTests(this.track.id);

      this._setIntroductionVideo();
    }, () => this.notify('Ocorreu um erro, por favor tente novamente mais tarde'));

    this._loadUserTrackInfo(trackId);
  }

  public fillUserCareer() {
    this._router.navigate(['configuracoes/usuarios/carreira/' + this._authService.getLoggedUser().user_id]);
  }

  public getCalendarEvents(): Array<TrackEvent> {
    const trackEvents: Array<TrackEvent> = [];

    if (this.track.eventsConfiguration)
      trackEvents.push.apply(trackEvents, this.track.eventsConfiguration);

    if (this.track.calendarEvents)
      trackEvents.push.apply(trackEvents, this.track.calendarEvents);

    return trackEvents;
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

  public getVideoDurationFormatted(): string {
    return this._utilService.formatDurationToHour(this.track.videoDuration);
  }

  private _setIntroductionVideo(): void {
    if (this.track.videoUrl && this.track.videoUrl !== '') {
      const options = {
        id: this.track.videoUrl
      };
      if (document.getElementById('videoContent')) {
        this.player = new Player('videoContent', options);
        this._handleVideoLoaded( this.player );
      }
    } else {
      const videoEl = document.getElementById('videoContent');
      if (videoEl)
        videoEl.innerHTML = '';
    }
  }

  private _handleVideoLoaded(player): void {
    player.on('loaded', () => {
      const frame = document.querySelector('iframe');
      if (frame) { frame.style.width = '100%'; }
      const divFrame = document.getElementById('videoContent');
      divFrame.style.visibility = 'initial';
    });
  }

  public watchMandatoryVideo(): void {
    const dialogRef = this._dialog.open(VideoDialogComponent, {
      data: { videoUrl: this.track.courseVideoUrl }
    });

    dialogRef.afterClosed().subscribe((result: boolean) => {
      if (result)
      this._tracksService.markMandatoryVideoViewed(this.track.id).subscribe(() => {
        this.track.trackInfo.viewedMandatoryVideo = true;
      });
    });
  }

  private _loadTrackTests(trackId: string) {
    this._testService.getTrackValuationTests(trackId).subscribe(res => {
      if (res.data.some(x => x.type === ValuationTestTypeEnum.Percentile)) {
        this.trackTestsResearch = res.data.filter(x => x.type === ValuationTestTypeEnum.Percentile);
      }
      if (res.data.some(x => x.type === ValuationTestTypeEnum.Free)) {
        const trackTestsFree = res.data.filter(x => x.type === ValuationTestTypeEnum.Free);
        if (trackTestsFree.some(x => x.testTracks.some(y => y.order === -1))) {
          this.trackTestsFree = trackTestsFree.filter(x =>
            x.testTracks.some(y => y.order === -1)
          );
        }
        if (trackTestsFree.some(x => x.testTracks.some(y => y.order !== -1))) {
          this.trackTestsOrdered = trackTestsFree.filter(x =>
            x.testTracks.some(y => y.order !== -1)
          );
        }
      }
    }, err => {
      this.notify(this.getErrorNotification(err));
    });
  }

  private _loadUserTrackInfo(trackId: string): void {
    this._userService.getBasicProgressInfo(trackId).subscribe((response) => {
      this.userTrackInfo = response.data;
      this.userTrackInfo.tracksInfo.forEach(element => {
        if (element.validFor && element.validFor > 0 && element.id === trackId) {
          if (new Date(element.dueDate).getTime() < new Date().getTime()) {
            localStorage.setItem('expiredTrack', 'Seu acesso à trilha expirou');
            this._router.navigate([ 'home' ]);
          }
        }
      });
    }, () => this.notify('Ocorreu um erro, por favor tente novamente mais tarde'));
  }
}
