import { Component, ViewChild, OnInit } from '@angular/core';
import { MatStepper, MatSnackBar, MatDialog, MatStep } from '@angular/material';
import { Track } from '../../../models/track.model';
import { NewTrackTrackInfoComponent } from './steps/1_track-info/track-info.component';
import { NotificationClass } from '../../../shared/classes/notification';
import { SettingsTracksService } from '../../_services/tracks.service';
import { NewTrackModulesEventsComponent } from './steps/3_modules-events/modules-events.component';
import { TrackEvent } from '../../../models/track-event.model';
import { TrackModule } from '../../../models/track-module.model';
import { Router } from '@angular/router';
import { CreatedTrackDialogComponent } from './steps/6_created-track/created-track.dialog';
import { NewTrackVideoComponent } from './steps/2_video/video.component';
import { NewTrackRelevantDatesComponent } from './steps/4_relevant-dates/relevant-dates.component';
import { environment } from 'src/environments/environment';
import { NewTrackModulesEventsWeightComponent } from './steps/3.6_modules-weight/modules-weight.component';
import { NewTrackModulesGradesComponent } from './steps/7_modules-grades/modules-grades.component';
import { NewTrackEcommerceComponent } from './steps/5_ecommerce/ecommerce.component';
import { EcommerceProduct } from 'src/app/models/ecommerce-product.model';
import { NewTrackModulesEventsDatesComponent } from './steps/3.5_modules-dates/modules-dates.component';

@Component({
  selector: 'app-settings-new-track',
  templateUrl: './new-track.component.html',
  styleUrls: ['./new-track.component.scss']
})
export class SettingsNewTrackComponent extends NotificationClass implements OnInit {

  @ViewChild('stepper') stepper: MatStepper;
  @ViewChild('firstStep') firstStep: MatStep;
  @ViewChild('trackVideo') trackVideo: NewTrackVideoComponent;
  @ViewChild('trackInfo') trackInfo: NewTrackTrackInfoComponent;
  @ViewChild('modulesEvents') modulesEvents: NewTrackModulesEventsComponent;
  @ViewChild('modulesEventsWeight') modulesEventsWeight: NewTrackModulesEventsWeightComponent;
  @ViewChild('modulesEventsAvailability') modulesEventsAvailability: NewTrackModulesEventsDatesComponent;
  @ViewChild('relevantDates') relevantDates: NewTrackRelevantDatesComponent;
  @ViewChild('modulesGradesWeight') modulesGradesWeight: NewTrackModulesGradesComponent;
  @ViewChild('ecommerce') ecommerce: NewTrackEcommerceComponent;

  public newTrack = new Track();
  public stepIndex: number = 0;
  public lastStep: number = 7;
  public loading: boolean = false;
  public allowEditing: boolean = false;
  public hasEcommerceIntegration = environment.ecommerceIntegration;

  private _savingContent: boolean = false;
  private _shouldFinish: boolean = true;

  constructor(
    protected _snackBar: MatSnackBar,
    private _tracksService: SettingsTracksService,
    private _dialog: MatDialog,
    private _router: Router,
  ) {
    super(_snackBar);
  }

  ngOnInit() {
    const trackStr = localStorage.getItem('editingTrack');
    if (trackStr && trackStr.trim() !== '') {
      this.newTrack = new Track( JSON.parse(trackStr) );
      this.allowEditing = true;
    }
  }

  public saveContent(): void {
    if (!this._savingContent) {
      this._savingContent = true;
      this._shouldFinish = (!this.hasEcommerceIntegration && this.stepIndex === this.lastStep - 1) || this.stepIndex === this.lastStep;
      this.nextStep();
    }
  }

  public nextStep() {
    switch (this.stepIndex) {
      case 3:
        this.modulesEventsWeight.prepareStep();
        break;
      case 4:
        this.modulesEventsAvailability.prepareStep();
        break;
      // case 6:
      //   this.modulesGradesWeight.prepareStep();
      //   break;
      default:
        break;
    }
    switch (this.stepper.selectedIndex) {
      case 0:
        this.trackInfo.nextStep(); break;
      case 1:
        this.trackVideo.nextStep(); break;
      case 2:
        this.modulesEvents.nextStep();
        break;
      case 3:
        this.modulesEventsWeight.nextStep();
        break;
      case 4:
        this.modulesEventsAvailability.nextStep(); break;
      case 5:
        this.relevantDates.nextStep();
        break;
      // case 6:
      //   this.modulesGradesWeight.nextStep();
      //   break;
      case 6:
        if (this.hasEcommerceIntegration)
          this.ecommerce.nextStep();
        break;
      default:
        break;
    }
  }

  public previousStep() {
    this.stepIndex--;
    this.stepper.previous();
  }

  public stepChanged(event, shouldFinish: boolean = true) {
    this._shouldFinish = shouldFinish;
    this.stepIndex = event.selectedIndex;
    this.nextStep();
  }

  public setTrackInfo(trackInfo: Track) {
    this.newTrack.setTrackInfo(trackInfo);
    this._updateTrackInfo( this.newTrack );
  }

  public setTrackVideo(trackVideo: Track) {
    this.newTrack.setVideoInfo(trackVideo);
    this._updateTrackInfo( this.newTrack );
  }

  public addModulesAndEvents(modulesEvents: Array<Array<TrackModule | TrackEvent>>) {
    this.newTrack.modulesConfiguration = modulesEvents[0] as Array<TrackModule>;
    this.newTrack.eventsConfiguration = modulesEvents[1] as Array<TrackEvent>;
    this._updateTrackInfo( this.newTrack );
  }

  public addModulesGradesWeight(modulesEvents: Array<Array<TrackModule>>) {
    this.newTrack.modulesConfiguration = modulesEvents[0] as Array<TrackModule>;
    this._updateTrackInfo( this.newTrack );
  }

  public manageRelevantDates(calendarEvents: Array<TrackEvent>) {
    this.loading = true;
    this._tracksService.manageCalendarEvents(
      this.newTrack.id, calendarEvents
    ).subscribe(() => {
      if (this._shouldFinish) {
        if (!this.hasEcommerceIntegration) {
          this._dialog.open(CreatedTrackDialogComponent);
          this._router.navigate([ 'configuracoes/trilhas' ]);
        } else {
          this.stepIndex++;
          this.stepper.next();
        }
      } else {
        this._shouldFinish = true;
      }

      this._savingContent = false;
      this.loading = false;
    }, (error) => this._errorHandlingFunc(error) );
  }

  public manageEcommerceInfo(ecommerceProducts: Array<EcommerceProduct>) {
    this.loading = true;
    this._tracksService.manageEcommerceProducts(
      this.newTrack.id, ecommerceProducts
    ).subscribe(() => {
      if (this._shouldFinish) {
        if (this._dialog.openDialogs.length <= 0) {
          this._dialog.open(CreatedTrackDialogComponent);
          this._router.navigate([ 'configuracoes/trilhas' ]);
        }
      }
    });
  }

  private _updateTrackInfo(track: Track, finish: boolean = false) {
    this.loading = true;
    this._tracksService.manageTrackInfo(track).subscribe((response) => {
      if (!this.newTrack.id)
        this.newTrack.id = response.data.id;

      if (this._shouldFinish) {
        if (finish) {
          if (this._dialog.openDialogs.length <= 0) {
            this._dialog.open(CreatedTrackDialogComponent);
          }
          this._router.navigate([ 'configuracoes/trilhas' ]);
        } else { this._updateFooter(response.data); }

      } else {
        this._shouldFinish = true;
      }

      this._savingContent = false;
      this.loading = false;
    }, (response) => this._errorHandlingFunc(response) );
  }

  private _updateFooter(track: Track) {
    this.newTrack.id = track.id;
    this.loading = false;
    this.stepIndex++;
    this.stepper.next();
  }

  private _errorHandlingFunc(response) {
    this._savingContent = false;
    this.loading = false;
    this.notify(
      this.getErrorNotification(response)
    );
  }
}
