import { Component, Input, Output, EventEmitter, OnInit } from '@angular/core';
import { MatSnackBar } from '@angular/material';
import { NotificationClass } from '../../../../../shared/classes/notification';
import { Track } from '../../../../../models/track.model';
import { TrackModule } from '../../../../../models/track-module.model';
import { TrackEvent } from '../../../../../models/track-event.model';

@Component({
  selector: 'app-new-track-modules-weight',
  templateUrl: './modules-weight.component.html',
  styleUrls: ['../new-track-steps.scss', './modules-weight.component.scss']
})
export class NewTrackModulesEventsWeightComponent extends NotificationClass {

  public readonly displayedColumns: string[] = [
    'content', 'weight'
  ];

  @Input() readonly track: Track;
  @Output() addModulesAndEvents = new EventEmitter<Array<Array<any>>>();

  public modules: Array<TrackModule> = [];
  public events: Array<TrackEvent> = [];
  public trackModulesEvents: Array<TrackModule | TrackEvent> = [];
  public totalWeight: number = 0;

  constructor(
    protected _snackBar: MatSnackBar
  ) {
    super(_snackBar);
  }

  public getTrackCards(): Array<TrackModule | TrackEvent> {
    let modulesEvents = [... this.modules, ...this.events];
    modulesEvents = modulesEvents.sort((a, b) => {
      return a.order - b.order;
    });
    return modulesEvents;
  }

  public prepareStep(): void {
    if (this.track && this.track.modulesConfiguration) {
      this.modules = this.track.modulesConfiguration;
      this.modules.forEach(e => {
        e.isEvent = false;
        e.weight = e.weight ? e.weight : 0;
      } );
    }

    if (this.track && this.track.eventsConfiguration) {
      this.events = this.track.eventsConfiguration;
      this.events.forEach(e => {
        e.isEvent = true;
        e.weight = e.weight ? e.weight : 0;
      });
    }
    this.trackModulesEvents = this.getTrackCards();
    this.setTotalValue();
  }

  public nextStep(): void {
    if (this.checkDates(this.trackModulesEvents.filter(x => x.cutOffDate))) {
      if (this.totalWeight === 100) {
        this.modules = this.trackModulesEvents.filter(x => x.isEvent === false) as Array<TrackModule>;
        this.events = this.trackModulesEvents.filter(x => x.isEvent === true) as Array<TrackEvent>;
        this.addModulesAndEvents.emit( [ this.modules, this.events ] );
      } else {
        this.notify('A soma dos pesos dos itens deve dar 100');
      }
    } else {
      this.notify('A data limite do BDQ não pode ser menor que a data atual');
    }
  }

  public getCurrentProgress(): string {
    return this.totalWeight > 100 ? (0).toString() + '%' :
      (100 - this.totalWeight).toString() + '%';
  }

  public setTotalValue() {
    this.totalWeight = 0;
    const weights = this.trackModulesEvents.map(x => x.weight);
    weights.forEach(weight => {
      this.totalWeight += weight ? weight : 0;
    });
  }

  public setRowValue(value: number, row: TrackModule | TrackEvent) {

    if (value < 0) {
      this.notify('Valores negativos não são permitidos.');
      row.weight = 0;
      return false;
    }

    row.weight = +value;
    this.setTotalValue();
  }

public checkDates(trackModulesEvents: Array<TrackModule | TrackEvent>) {
  let returnValue: boolean = true;
  for (let i = 0; i < trackModulesEvents.length; i++) {
    if (this.checkDate(trackModulesEvents[i].cutOffDate)) {
      returnValue = true;
      break;
    }
  }
  return returnValue;
}

public checkDate(cutOffDate: Date) {
  if (cutOffDate) {
    const today = new Date();
    if (cutOffDate > today) {
      return false;
    } else {
      return true;
    }
  }
}

}
