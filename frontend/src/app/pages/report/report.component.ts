import { Component, OnInit } from '@angular/core';
import { NotificationClass } from '../../shared/classes/notification';
import { MatSnackBar } from '@angular/material';
import { ActivatedRoute, Router } from '@angular/router';
import { ExcelService } from 'src/app/shared/services/excel.service';
import { SettingsTracksService } from 'src/app/settings/_services/tracks.service';
import { SettingsUsersService } from 'src/app/settings/_services/users.service';
import { UserPreview } from 'src/app/models/previews/user.interface';
import { UserCategoryFilterSearchOption } from 'src/app/settings/manage-team/filters/filters.interface';
import { TrackPreview } from 'src/app/models/previews/track.interface';
import { ModulePreview } from 'src/app/models/previews/module.interface';
import { EventPreview } from 'src/app/models/previews/event.interface';
import { SettingsModulesService } from 'src/app/settings/_services/modules.service';
import { SettingsEventsService } from 'src/app/settings/_services/events.service';
import { TrackOverview } from 'src/app/models/track-overview.interface';
import { ReportsService } from 'src/app/settings/_services/reports.service';

@Component({
  selector: 'app-report',
  templateUrl: './report.component.html',
  styleUrls: ['./report.component.scss']
})
export class ReportComponent extends NotificationClass implements OnInit {

  public users: Array<UserPreview> = [];
  public selectedTrackId: string = '';
  public readonly pageSize: number = 20;
  public selectedUsers: Array<UserPreview> = [];
  private _selectedFilters: Array<UserCategoryFilterSearchOption> = [];
  private _createdSince: Date = null;
  private _sortDirection: boolean = null;
  private _sortActive: string = '';
  private user: any;
  private _searchValue: string;
  public usersCount: number = 0;
  private _usersPage: number = 1;
  public getManageInfo: boolean;
  public displayedContent: boolean = true;

  public tracks: Array<TrackPreview> = [];
  public tracksCount: number;
  public selectedTracks: Array<TrackPreview> = [];

  public modules: Array<ModulePreview> = [];
  public modulesCount: number;
  public selectedModules: Array<ModulePreview> = [];

  public events: Array<EventPreview> = [];
  public eventsCount: number;
  public selectedEvents: Array<EventPreview> = [];
  public userId: Array<string> = ['5c0eabafab1c6871c5905f87'];

  public viewOptions = [
    { selected: true, title: 'INFORMAÇÕES CADASTRAIS', tag: 'INFO' },
    { selected: false, title: 'EXECUÇÃO DOS PROGRAMAS', tag: 'PROGRAMAS' },
    { selected: false, title: 'OBJETOS DE APRENDIZAGEM E AVALIAÇÃO', tag: 'APRENDIZAGEM' },
    { selected: false, title: 'PERFIL PROFISSIONAL', tag: 'CARREIRA' },
    { selected: false, title: 'PESQUISAS', tag: 'PESQUISA' }
  ];


  constructor(
    protected _snackBar: MatSnackBar,
    private _excelService: ExcelService,
    private _tracksService: SettingsTracksService,
    private _usersService: SettingsUsersService,
    private _modulesService: SettingsModulesService,
    private _eventsService: SettingsEventsService,
    private _reportService: ReportsService
  ) {
    super(_snackBar);
  }

  ngOnInit() {
    this._loadTracks();
    this._loadModules();
    this._loadEvents();
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
    this._removeFromCollection(
      this.modules, this.selectedModules, id
    );
  }

  public removeSelectedEvent(id: string) {
    this._removeFromCollection(
      this.events, this.selectedEvents, id
    );
  }

  public removeSelectedTrack(id: string) {
    this._removeFromCollection(
      this.tracks, this.selectedTracks, id
    );
  }

  private _removeFromCollection(collection, selected, id: string): void {
    const selectedTrackIndex = selected.findIndex(x => x.id === id);
    selected.splice(selectedTrackIndex , 1);

    const trackIndex = collection.findIndex(x => x.id === id);
    collection[trackIndex].checked = false;
  }

  public updateModules(): void {
    this.selectedModules = this._updateCollection(
      this.modules, this.selectedModules
    );
  }

  public updateEvents(): void {
    this.selectedEvents = this._updateCollection(
      this.events, this.selectedEvents
    );
  }

  public updateTracks(): void {
    this.selectedTracks = this._updateCollection(
      this.tracks, this.selectedTracks
    );
  }

  private _updateCollection(collection, selected): Array<any> {
    const prevSelected = selected.filter(x =>
      !collection.find(t => t.id === x.id)
    );
    const selectedColl = collection.filter(track =>
      track.checked
    );
    return [ ...prevSelected, ...selectedColl];
  }

  public selectViewOption(optTitle: string) {
    this.viewOptions.forEach(opt => { opt.selected = false; });
    this.viewOptions.find(x => x.title === optTitle).selected = true;
  }

  public isViewOption(tag: string): boolean {
    return this.viewOptions.find(x => x.tag === tag).selected;
  }

  private _loadTracks(searchValue: string = ''): void {
    this._tracksService.getPagedFilteredTracksList(
      1, 2, searchValue
    ).subscribe(response => {
      response.data.tracks.forEach(track => {
        track.checked = this.selectedTracks.find(t => t.id === track.id) && true;
      });
      this.tracks = response.data.tracks;
      this.tracksCount = response.data.itemsCount;
    });
  }

  private _loadModules(searchValue: string = ''): void {
    this._modulesService.getPagedFilteredModulesList(
      1, 2, searchValue
    ).subscribe(response => {
      response.data.modules.forEach((mod: ModulePreview) => {
        mod.checked = this.selectedModules.find(t => t.id === mod.id) && true;
      });
      this.modules = response.data.modules;
      this.modulesCount = response.data.itemsCount;
    });
  }

  private _loadEvents(searchValue: string = ''): void {
    this._eventsService.getPagedFilteredEventsList(
      1, 2, searchValue, true
    ).subscribe(response => {
      response.data.events.forEach((ev: EventPreview) => {
        ev.checked = this.selectedEvents.find(t => t.id === ev.id) && true;
      });
      this.events = response.data.events;
      this.eventsCount = response.data.itemsCount;
    });
  }

  public exportUsersGrade() {
    if (!this.userId) {
      this.notify('Selecione usuários para poder prosseguir');
    } else {
      // const selectedUsers = this.getSelectedUsers().map(x => x.id);
      const selectedUsers = this.userId;
      this._usersService.exportUsersGrade(selectedUsers).subscribe(res => {
        this._excelService.exportAsExcelFile(
          this._excelService.buildExportUsersGrade(res),
          'Notas'
        );
      }, err => { this.notify(this.getErrorNotification(err)); });
    }
  }

  public exportStudents() {

    if (!this.selectedTrackId) {
      this.notify('Selecione uma trilha para exportar os dados.');
      return;
    }

    this._tracksService.getTrackOverviewStudents(this.selectedTrackId).subscribe(res => {
      this._excelService.exportAsExcelFile(res.data, 'Alunos-Trilha-' + this.selectedTrackId);
    }, err => { this.notify(this.getErrorNotification(err)); });
  }

  // public exportUsersEffectiveness() {
  //   if (this.getSelectedUsers().length === 0) {
  //     this.notify('Selecione usuários para poder prosseguir');
  //   } else {
  //     const selectedUsers = this.getSelectedUsers().map(x => { return {
  //       userId: x.id,
  //       name: x.name
  //     }; });
  //     this._usersService.exportUsersEffectiveness(selectedUsers).subscribe(res => {
  //       this._excelService.exportAsExcelFile(
  //         this._excelService.buildExportUsersEffectiveness(res),
  //         'Efetividade'
  //       );
  //     }, err => { this.notify(this.getErrorNotification(err)); });
  //   }
  // }

  private _getLevelName(level: number): string {
    switch (level) {
      case 1:
        return 'Iniciante';
      case 2:
        return 'Intermediário';
      case 3:
        return 'Avançado';
      case 4:
        return 'Expert';
      default:
        return 'Sem Badge';
    }
  }

  public triggerSearch(searchValue: string) {
    this._searchValue = searchValue;
    this._loadUsers(this._usersPage, this._searchValue);
  }

  private _loadUsers(page: number, searchValue: string = ''): void {
    this._usersService.getFilteredUserToManage(
      page, this.pageSize,
      searchValue, this._createdSince,
      this._selectedFilters,
      this.user.have_dependents ? this.user.name : '',
      this._sortActive, this._sortDirection
    ).subscribe((response) => {
      this.users = response.data.userItems;
      this.usersCount = response.data.itemsCount;
      this.selectedUsers.forEach(selectedUser => {
        const userIndex = this.users.findIndex(x => x.id === selectedUser.id);
        if (userIndex >= 0) this.users[userIndex].checked = true;
      });
    }, () => this.notify('Ocorreu um erro, por favor tente novamente mais tarde'));
  }

  public exportEffectiveness() {
    this._modulesService.getEffectivenessIndicators().subscribe(res => {
      this._excelService.exportAsExcelFile(
        this._excelService.buildExportUsersEffectiveness(res),
        'Efetividade'
      );
    }, err => { this.notify(this.getErrorNotification(err)); });
  }

  public getAnswersExcel(): void {
    this._usersService.getAllUserNpsInfos().subscribe((response) => {
      this._excelService.exportAsExcelFile(response.data, 'Nps');
    }, (error) => this.notify( this.getErrorNotification(error) ));
  }
}
