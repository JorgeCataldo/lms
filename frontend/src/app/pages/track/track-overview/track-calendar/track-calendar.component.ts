import { Component, Input } from '@angular/core';
import { TrackEvent } from '../../../../models/track-event.model';

import {
  CalendarEvent, CalendarView, CalendarDateFormatter
} from 'angular-calendar';
import { Subject } from 'rxjs';
import { isSameMonth, isSameDay, addSeconds } from 'date-fns';
import { CustomDateFormatter } from 'src/app/shared/classes/date-formatter';
import { Router } from '@angular/router';
import { EventAction, EventColor } from 'calendar-utils';
import * as moment from 'moment/moment';

const colors = {
  red: {
    primary: 'rgba(255, 67, 118, 0.6)',
    secondary: 'rgba(255, 67, 118, 0.6)'
  },
  green: {
    primary: 'rgba(6, 226, 149, 0.6)',
    secondary: 'rgba(6, 226, 149, 0.6)'
  },
  purple: {
    primary: 'rgba(189, 98, 255, 0.6)',
    secondary: 'rgba(189, 98, 255, 0.6)'
  },
  orange: {
    primary: 'rgba(255, 165, 0, 0.6)',
    secondary: 'rgba(255, 165, 0, 0.6)'
  }
};

@Component({
  selector: 'app-track-overview-calendar',
  templateUrl: './track-calendar.component.html',
  providers: [{
    provide: CalendarDateFormatter,
    useClass: CustomDateFormatter
  }]
})
export class TrackCalendarComponent {

  @Input() set setEvents(events: Array<TrackEvent>) {
    if (events) {
      this.events = [];

      const trackIds = events.filter(
        ev => ev.trackId
      ).map(
        ev => ev.trackId
      ).sort().filter((item, pos, ary) => {
        return !pos || item !== ary[pos - 1];
      });

      events.forEach(ev => {
        if (ev.eventDate) {
          const eventDate = new Date(ev.eventDate);
          const actionClass = this._getActionClass(ev.trackId, trackIds);

          this.events.push({
            meta: {
              eventId: ev.eventId,
              scheduleId: ev.eventScheduleId
            },
            start: eventDate,
            end: ev.duration ? addSeconds(eventDate, (ev.duration as number)) : eventDate,
            title: this._getFullTitle(ev),
            color: this._getColor(ev.trackId, trackIds),
            actions: this._getActions(ev, actionClass),
            allDay: ev.duration === null
          });
        }
      });
    }
  }

  public events: CalendarEvent[];
  public view: CalendarView = CalendarView.Month;
  public CalendarView = CalendarView;
  public viewDate: Date = new Date();
  public refresh: Subject<any> = new Subject();
  public activeDayIsOpen: boolean = false;

  constructor(
    private _router: Router
  ) { }

  public dayClicked({ date, events }: { date: Date; events: CalendarEvent[] }): void {
    if (isSameMonth(date, this.viewDate)) {
      this.viewDate = date;
      if (
        (isSameDay(this.viewDate, date) && this.activeDayIsOpen === true) ||
        events.length === 0
      ) {
        this.activeDayIsOpen = false;
      } else {
        this.activeDayIsOpen = true;
      }
    }
  }

  public handleEvent(event: CalendarEvent): void {
    if (event && event.meta && event.meta.eventId && event.meta.scheduleId)
      this._router.navigate(['evento/' + event.meta.eventId + '/' + event.meta.scheduleId]);
  }

  private _getFullTitle(ev: TrackEvent): string {
    if (!ev.duration)
      return '<span>' + ev.title + '</span>';
    return ev.title + ' - ' + moment(ev.eventDate).format('HH:mm');
  }

  private _getColor(trackId: string, trackIds: Array<string>): EventColor {
    if (trackId) {
      const index = trackIds.findIndex(t => t === trackId) + 1;
      if (index % 4 === 0)
        return colors.red;
      else if (index % 3 === 0)
        return colors.green;
      else if (index % 2 === 0)
        return colors.orange;
      else
        return colors.purple;
    }
    return colors.green;
  }

  private _getActionClass(trackId: string, trackIds: Array<string>): string {
    if (trackId) {
      const index = trackIds.findIndex(t => t === trackId) + 1;
      if (index % 4 === 0)
        return 'red';
      else if (index % 3 === 0)
        return 'green';
      else if (index % 2 === 0)
        return 'orange';
      else
        return 'purple';
    }
    return'green';
  }

  private _getActions(ev: TrackEvent, actionClass: string): Array<EventAction> {
    if (!ev.eventId || !ev.eventScheduleId)
      return [];

    return [{
      label: '<span class="' + actionClass + '" >Ver</span>',
      onClick: ({ event }: { event: CalendarEvent }) => {
        this.handleEvent(event);
      }
    }];
  }

}
