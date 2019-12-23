import { Component, Input, Output, EventEmitter, OnInit } from '@angular/core';
import { MatSnackBar } from '@angular/material';
import { NotificationClass } from '../../../../../shared/classes/notification';
import { Track } from '../../../../../models/track.model';
import { TrackModule } from '../../../../../models/track-module.model';
import { TrackEvent } from '../../../../../models/track-event.model';

@Component({
  selector: 'app-new-track-modules-dates',
  templateUrl: './modules-dates.component.html',
  styleUrls: ['../new-track-steps.scss', './modules-dates.component.scss']
})
export class NewTrackModulesEventsDatesComponent extends NotificationClass {

  public readonly displayedColumns: string[] = [
    'content', 'empty', 'moduleOpening', 'data', 'evaluation'
  ];

  @Input() readonly track: Track;
  @Output() addModulesAndEvents = new EventEmitter<Array<Array<any>>>();

  public modules: Array<TrackModule> = [];
  public events: Array<TrackEvent> = [];
  public trackModulesEvents: Array<TrackModule | TrackEvent> = [];

  constructor(
    protected _snackBar: MatSnackBar
  ) {
    super(_snackBar);
  }

  public prepareStep(): void {
    if (this.track && this.track.modulesConfiguration) {
      this.modules = this.track.modulesConfiguration;
      this.modules.forEach(e => {
        e.isEvent = false;
        e.alwaysAvailable = e.alwaysAvailable === null ||
          e.alwaysAvailable === undefined ||
          (e.alwaysAvailable === false && (e.openDate === null || e.openDate === undefined)) ?
          true : e.alwaysAvailable;
      } );
    }

    if (this.track && this.track.eventsConfiguration) {
      this.events = this.track.eventsConfiguration;
      this.events.forEach(e => {
        e.isEvent = true;
        e.alwaysAvailable = e.alwaysAvailable === null ||
          e.alwaysAvailable === undefined ||
          (e.alwaysAvailable === false && (e.openDate === null || e.openDate === undefined)) ?
          true : e.alwaysAvailable;
      });
    }

    this.trackModulesEvents = this.getTrackCards();
  }

  public getTrackCards(): Array<TrackModule | TrackEvent> {
    let modulesEvents = [... this.modules, ...this.events];
    modulesEvents = modulesEvents.sort((a, b) => {
      return a.order - b.order;
    });
    return modulesEvents;
  }

  public nextStep(): void {
    if (this.checkDates(this.trackModulesEvents.filter(x => !x.alwaysAvailable))) {
      this.modules = this.trackModulesEvents.filter(x => x.isEvent === false) as Array<TrackModule>;
      this.events = this.trackModulesEvents.filter(x => x.isEvent === true) as Array<TrackEvent>;
      this.addModulesAndEvents.emit( [ this.modules, this.events ] );
    } else {
      this.notify('A data final não pode ser maior do que a inicial');
    }
  }

  public selectProfessionalExperience(row: TrackEvent | TrackModule) {
    row.alwaysAvailable = !row.alwaysAvailable;
    if (row.alwaysAvailable) {
      row.openDate = null;
      row.valuationDate = null;
    }
  }

  public checkDates(trackModulesEvents: Array<TrackModule | TrackEvent>): boolean {
    let returnValue: boolean = true;
    for (let i = 0; i < trackModulesEvents.length; i++) {
      if (!this.checkDate(trackModulesEvents[i].openDate, trackModulesEvents[i].valuationDate)) {
        returnValue = false;
        break;
      }
    }
    return returnValue;
  }

  public checkDate(fromDate: Date, toDate: Date): boolean {
    if (fromDate && toDate) {
      fromDate = new Date(fromDate);
      toDate = new Date(toDate);
      if (fromDate > toDate) {
        return false;
      } else {
        return true;
      }
    } else if (fromDate) {
      return true;
    } else {
      return false;
    }
  }
}
