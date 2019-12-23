import { Component, Input, Output, EventEmitter, OnInit } from '@angular/core';
import { MatSnackBar } from '@angular/material';
import { NotificationClass } from '../../../../../shared/classes/notification';
import { Track } from '../../../../../models/track.model';
import { TrackModule } from '../../../../../models/track-module.model';
import { TrackEvent } from '../../../../../models/track-event.model';
import { Level } from '../../../../../models/shared/level.interface';
import { SharedService } from '../../../../../shared/services/shared.service';
import { ModulePreview } from '../../../../../models/previews/module.interface';
import { Subject } from 'rxjs';
import { debounceTime } from 'rxjs/operators';
import { SettingsModulesService } from '../../../../_services/modules.service';
import { EventPreview } from '../../../../../models/previews/event.interface';
import { SettingsEventsService } from '../../../../_services/events.service';

@Component({
  selector: 'app-new-track-modules-events',
  templateUrl: './modules-events.component.html',
  styleUrls: ['../new-track-steps.scss', './modules-events.component.scss']
})
export class NewTrackModulesEventsComponent extends NotificationClass implements OnInit {

  @Input() readonly track: Track;
  @Output() addModulesAndEvents = new EventEmitter<Array<Array<any>>>();

  public newModule: TrackModule = new TrackModule();
  public modules: Array<TrackModule> = [];
  public searchModuleResults: Array<ModulePreview> = [];
  private _searchModuleSubject: Subject<string> = new Subject();

  public newEvent: TrackEvent = new TrackEvent();
  public events: Array<TrackEvent> = [];
  public searchEventResults: Array<EventPreview> = [];
  private _searchEventSubject: Subject<string> = new Subject();

  public levels: Array<Level> = [];
  public trackModulesEvents: Array<TrackModule | TrackEvent> = [];

  constructor(
    protected _snackBar: MatSnackBar,
    private _sharedService: SharedService,
    private _modulesService: SettingsModulesService,
    private _eventsService: SettingsEventsService
  ) {
    super(_snackBar);
  }

  ngOnInit() {
    this._setLevels();
    this._setModuleSearchSubscription();
    this._setEventSearchSubscription();

    if (this.track && this.track.modulesConfiguration) {
      this.modules = this.track.modulesConfiguration;
      this.modules.forEach(e => e.isEvent = false);
    }

    if (this.track && this.track.eventsConfiguration) {
      this.events = this.track.eventsConfiguration;
      this.events.forEach(e => e.isEvent = true);
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

  public updateModuleSearch(searchTextValue: string) {
    this._searchModuleSubject.next( searchTextValue );
  }

  public updateEventSearch(searchTextValue: string) {
    this._searchEventSubject.next( searchTextValue );
  }

  public setModule(module: ModulePreview) {
    this.newModule.title = module.title;
    this.newModule.moduleId = module.id;
    this.newModule.percentage = 1;
    this.newEvent.isEvent = false;
    this.searchModuleResults = [];
    (document.getElementById('moduleInput') as HTMLInputElement).value = module.title;
  }

  public setEvent(event: EventPreview) {
    this.newEvent.title = event.title;
    this.newEvent.eventId = event.id;
    this.newEvent.eventScheduleId = event.nextSchedule.id;
    this.newEvent.isEvent = true;
    this.searchEventResults = [];
    (document.getElementById('eventInput') as HTMLInputElement).value = event.title;
  }

  public addModule() {
    if (!this.newModule.moduleId || (this.newModule.level == null)) {
      this.notify('Selecione o módulo e defina o nível para adicioná-lo à Trilha');

    } else if (
      this.newModule.percentage != null &&
      (this.newModule.percentage <= 0 ||
       this.newModule.percentage > 100)
    ) {
      this.notify('O aproveitameneto deve ser maior que 0% e menor que 100%');
    } else {
      this.newModule.order = this.trackModulesEvents.length;
      this.trackModulesEvents.push( this.newModule );
      this.newModule = new TrackModule();
      (document.getElementById('moduleInput') as HTMLInputElement).value = '';
    }
  }

  public addEvent() {
    if (!this.newEvent.eventId) {
      this.notify('Selecione o evento e defina o nível para adicioná-lo à Trilha');
    } else {
      this.newEvent.order = this.trackModulesEvents.length;
      this.trackModulesEvents.push( this.newEvent );
      this.newEvent = new TrackEvent();
      (document.getElementById('eventInput') as HTMLInputElement).value = '';
    }
  }

  public removeItemFromTrack(order: number) {
    const removeIndex = this.trackModulesEvents.findIndex(x => x.order === order);
    this.trackModulesEvents.splice(removeIndex, 1);
    this.trackModulesEvents.forEach((x, index) => x.order = index);
  }

  public updateTrackItemsOrder(fromTo: Array<number>) {
    const fromItem = this.trackModulesEvents.splice(fromTo[0], 1)[0];
    this.trackModulesEvents.splice(fromTo[1], 0, fromItem);
    this.trackModulesEvents.forEach((x, index) => x.order = index);
  }

  public nextStep(): void {
    this.modules = this.trackModulesEvents.filter(x => x.isEvent === false) as Array<TrackModule>;
    this.events = this.trackModulesEvents.filter(x => x.isEvent === true) as Array<TrackEvent>;
    this.addModulesAndEvents.emit( [ this.modules, this.events ] );
  }

  private _setModuleSearchSubscription() {
    this._searchModuleSubject.pipe(
      debounceTime(500)
    ).subscribe((searchValue: string) => {
      this._modulesService.getPagedFilteredModulesList(1, 4, searchValue).subscribe((response) => {
        this.searchModuleResults = response.data.modules;
      });
    });
  }

  private _setEventSearchSubscription() {
    this._searchEventSubject.pipe(
      debounceTime(500)
    ).subscribe((searchValue: string) => {
      this._getEventResults( searchValue );
    });
  }

  private _getEventResults(searchValue: string): void {
    this._eventsService.getPagedFilteredEventsList(
      1, 4, searchValue, true
    ).subscribe((response) => {
      this.searchEventResults = response.data.events.filter(
        (ev: EventPreview) => ev.nextSchedule !== null
      );
    });
  }

  private _setLevels(): void {
    this._sharedService.getLevels().subscribe((response) => {
      this.levels = response.data;
    }, () => this.notify('Ocorreu um erro ao buscar as dificuldades, por favor tente novamente mais tarde') );
  }
}
