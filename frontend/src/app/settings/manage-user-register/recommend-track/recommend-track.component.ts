import { Component, OnInit } from '@angular/core';
import { MatSnackBar, MatDialog } from '@angular/material';
import { NotificationClass } from '../../../shared/classes/notification';
import { UserPreview } from '../../../models/previews/user.interface';
import { SettingsUsersService } from '../../_services/users.service';
import { TrackPreview } from '../../../models/previews/track.interface';
import { SettingsTracksService } from '../../_services/tracks.service';
import { SuccessDialogComponent } from 'src/app/shared/dialogs/success/success.dialog';
import { Recommendations } from '../../users/user-models/user-track';
import { Router } from '@angular/router';
import { AuthService } from 'src/app/shared/services/auth.service';
import { SettingsModulesService } from '../../_services/modules.service';
import { ModulePreview } from 'src/app/models/previews/module.interface';
import { EventPreview } from 'src/app/models/previews/event.interface';
import { SettingsEventsService } from '../../_services/events.service';
import { EventSchedule } from 'src/app/models/event-schedule.model';

@Component({
  selector: 'app-settings-recommend-track',
  templateUrl: './recommend-track.component.html',
  styleUrls: ['./recommend-track.component.scss']
})
export class SettingsRecommendTrackComponent extends NotificationClass implements OnInit {

  public users: Array<UserPreview> = [];
  public selectedUsers: Array<UserPreview> = [];
  public showAllSelectedUsers: boolean = false;

  public tracks: Array<TrackPreview> = [];
  public selectedTracks: Array<TrackPreview> = [];

  public modules: Array<ModulePreview> = [];
  public selectedModules: Array<ModulePreview> = [];

  public events: Array<EventPreview> = [];
  public selectedEvents: Array<EventSchedule> = [];

  constructor(
    protected _snackBar: MatSnackBar,
    private _dialog: MatDialog,
    private _usersService: SettingsUsersService,
    private _tracksService: SettingsTracksService,
    private _modulesService: SettingsModulesService,
    private _eventsService: SettingsEventsService,
    private _router: Router,
    private _authService: AuthService
  ) {
    super(_snackBar);
  }

  ngOnInit() {
    this._loadModules();
    this._loadEvents();
    this._loadTracks();
    const usersStr = localStorage.getItem('SelectedUsers');
    if (usersStr && usersStr.trim() !== '')
      this.selectedUsers = JSON.parse( usersStr );
  }

  public triggerUserSearch(searchValue: string) {
    if (searchValue && searchValue.trim() !== '')
      this._loadUsers( searchValue );
  }

  public addUser(user: UserPreview) {
    const userExists = this.selectedUsers.find(u => u.id === user.id);
    if (!userExists)
      this.selectedUsers.push( user );
    this.resetUserSearch();
  }

  public removeSelectedUser(user: UserPreview) {
    const index = this.selectedUsers.findIndex(x => x.id === user.id);
    this.selectedUsers.splice(index , 1);
  }

  public resetUserSearch(): void {
    this.users = [];
  }

  public triggerModuleSearch(searchValue: string) {
    this._loadModules(searchValue);
  }

  public triggerEventSearch(searchValue: string) {
    this._loadEvents( searchValue );
  }

  public triggerTrackSearch(searchValue: string) {
    this._loadTracks( searchValue );
  }

  public removeSelectedModule(id: string): void {
    const selectedIndex = this.selectedModules.findIndex(x => x.id === id);
    this.selectedModules.splice(selectedIndex , 1);

    const moduleIndex = this.modules.findIndex(x => x.id === id);
    this.modules[moduleIndex].checked = false;
  }

  public removeSelectedEvent(id: string) {
    const selectedIndex = this.selectedEvents.findIndex(x => x.id === id);
    this.selectedEvents.splice(selectedIndex , 1);

    const eventIndex = this.events.findIndex(x => x.id === id);
    this.events[eventIndex].checked = false;
  }

  public removeSelectedTrack(id: string) {
    const selectedTrackIndex = this.selectedTracks.findIndex(x => x.id === id);
    this.selectedTracks.splice(selectedTrackIndex , 1);

    const trackIndex = this.tracks.findIndex(x => x.id === id);
    this.tracks[trackIndex].checked = false;
  }

  public updateModules(): void {
    const prevSelected = this.selectedModules.filter(track =>
      !this.modules.find(t => t.id ===  track.id)
    );
    const selectedTracks = this.modules.filter(track =>
      track.checked
    );
    this.selectedModules = [ ...prevSelected, ...selectedTracks];
  }

  public updateEvents(schedule: EventSchedule): void {
    const currentEvent = this.events.find(ev => ev.id === schedule.eventId);
    this.selectedEvents = currentEvent.checked ?
      [ ...this.selectedEvents, schedule ] :
      this.selectedEvents.filter(ev => ev.eventId !== schedule.eventId);
  }

  public updateTracks(): void {
    const prevSelected = this.selectedTracks.filter(track =>
      !this.tracks.find(t => t.id ===  track.id)
    );
    const selectedTracks = this.tracks.filter(track =>
      track.checked
    );
    this.selectedTracks = [ ...prevSelected, ...selectedTracks];
  }

  public sendRecommendations(): void {
    if (!this.selectedUsers || this.selectedUsers.length === 0) {
      this.notify('É necessario selecionar pelo menos um usuário');
      return;
    }

    if (this.selectedTracks.length > 0 || this.selectedModules.length > 0 || this.selectedEvents.length > 0) {
      const recommendations: Array<Recommendations> = this._setRecommendations();

      this._usersService.updateUserRecommendations(
        recommendations
      ).subscribe(() => {
        this._router.navigate(['/configuracoes/gerenciar-equipe']);
        this._dialog.open(SuccessDialogComponent, {
          'data': 'Recomendações enviadas com sucesso'
        });
      }, () => this.notify('Ocorreu um erro ao atualizar as trilhas dos usuários, por favor tente novamente mais tarde'));

    } else {
      this.notify('É necessario selecionar pelo menos um módulo, evento ou trilha');
    }
  }

  private _setRecommendations(): Array<Recommendations> {
    const recommendations: Array<Recommendations> = [];

    this.selectedUsers.forEach(user => {
      const recommendation: Recommendations = {
        userId: user.id,
        tracks: [], modules: [], events: []
      };

      this.selectedTracks.forEach(track => {
        recommendation.tracks.push({
          id: track.id,
          name: track.title,
          level: 0, percentage: 0
        });
      });

      this.selectedModules.forEach(module => {
        recommendation.modules.push({
          id: module.id,
          name: module.title
        });
      });

      this.selectedEvents.forEach(schedule => {
        recommendation.events.push({
          eventId: schedule.eventId,
          scheduleId: schedule.id,
          name: schedule.eventTitle
        });
      });

      recommendations.push(recommendation);
    });

    return recommendations;
  }

  private _loadTracks(searchValue: string = ''): void {
    const loggedUser = this._authService.getLoggedUser();
    const published = !loggedUser || loggedUser.role === 'Student' ?
      true : null;

    this._tracksService.getPagedFilteredTracksList(
      1, 4, searchValue, published
    ).subscribe(response => {
      response.data.tracks.forEach(track => {
        track.checked = this.selectedTracks.find(t => t.id ===  track.id) && true;
      });
      this.tracks = response.data.tracks;
    });
  }

  private _loadModules(searchValue: string = ''): void {
    this._modulesService.getPagedFilteredModulesList(
      1, 4, searchValue
    ).subscribe(response => {
      response.data.modules.forEach((mod: ModulePreview) => {
        mod.checked = this.selectedModules.find(t => t.id === mod.id) && true;
      });
      this.modules = response.data.modules;
    });
  }

  private _loadEvents(searchValue: string = ''): void {
    this._eventsService.getPagedFilteredEventsList(
      1, 4, searchValue, true
    ).subscribe(response => {
      response.data.events.forEach((ev: EventPreview) => {
        ev.checked = this.selectedEvents.find(t => t.id === ev.id) && true;
      });
      this.events = response.data.events;
    });
  }

  private _loadUsers(searchValue: string = ''): void {
    this._usersService.getPagedFilteredUsersList(
      1, 4, searchValue
    ).subscribe(response => {
      this.users = response.data.userItems;
    });
  }

}
