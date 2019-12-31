import { Component, OnInit } from '@angular/core';
import { Event } from 'src/app/models/event.model';
import { NotificationClass } from 'src/app/shared/classes/notification';
import { MatSnackBar } from '@angular/material';
import { SettingsEventsService } from '../../_services/events.service';
import { Router, ActivatedRoute } from '@angular/router';
import { EventSchedule } from 'src/app/models/event-schedule.model';
import { Level } from 'src/app/models/shared/level.interface';
import { SharedService } from 'src/app/shared/services/shared.service';
import { SettingsEventsDraftsService } from '../../_services/events-drafts.service';

@Component({
  selector: 'app-settings-event-details',
  templateUrl: './event-details.component.html',
  styleUrls: ['./event-details.component.scss']
})
export class SettingsEventDetailsComponent extends NotificationClass implements OnInit {

  public event: Event;
  public nextSchedules: Array<EventSchedule> = [];
  public currentSchedules: Array<EventSchedule> = [];
  public pastSchedules: Array<EventSchedule> = [];
  public levels: Array<Level> = [];
  levelDict: {};

  constructor(
    protected _snackBar: MatSnackBar,
    private _eventsService: SettingsEventsService,
    private _draftsService: SettingsEventsDraftsService,
    private _activatedRoute: ActivatedRoute,
    private _router: Router,
    private _sharedService: SharedService
  ) {
    super(_snackBar);
    this._loadLevels();
  }

  ngOnInit() {
    const eventId = this._activatedRoute.snapshot.paramMap.get('eventId');
    const isDraft = this._router.url.includes('rascunho');

    if (isDraft) {
      this._draftsService.getEventDraftById(eventId).subscribe((response) => {
        this.event = response.data;
        this._adjustSchedules();

      }, () => this.notify('Ocorreu um erro, por favor tente novamente mais tarde'));

    } else {
      this._eventsService.getEventById(eventId).subscribe((response) => {
        this.event = response.data;
        this._adjustSchedules();

      }, () => this.notify('Ocorreu um erro, por favor tente novamente mais tarde'));
    }
  }

  private _adjustSchedules() {
    this.nextSchedules = this.event.schedules.filter(schedule =>
      new Date(schedule.eventDate) > new Date()
    );
    const finishedEvents = this.event.schedules.filter(schedule =>
      new Date(schedule.eventDate) < new Date()
    );
    this.currentSchedules = finishedEvents.filter(x => x.finishedAt === null);
    this.pastSchedules = finishedEvents.filter(x => x.finishedAt !== null);
  }

  public goEditEvent(): void {
    localStorage.setItem('editingEventInitialIndex', '1');
    localStorage.setItem('editingEvent',
      JSON.stringify( this.event )
    );
    this._router.navigate([ '/configuracoes/evento' ]);
  }

  public goManageApplications(scheduleId: string): void {
    this._router.navigate(
      [ '/configuracoes/gerenciar-inscricoes/' + this.event.id + '/' + scheduleId ]
    );
  }

  public goManageApplicationsGrades(scheduleId: string): void {
    this._router.navigate(
      [ '/configuracoes/gerenciar-inscricoes-notas/' + this.event.id + '/' + scheduleId ]
    );
  }

  public viewResults(scheduleId: string): void {
    this._router.navigate(
      [ '/configuracoes/gerenciar-eventos/' + this.event.id + '/resultados/' + scheduleId ]
    );
  }

  private _loadLevels(): void {
    this._sharedService.getLevels().subscribe((response) => {
      this.levels = response.data;
      this.levelDict = {};
      response.data.forEach(level => {
        this.levelDict[level.id] = level.description;
      });

    }, () => this.notify('Ocorreu um erro, por favor tente novamente mais tarde'));
  }
}
