import { Component, OnInit } from '@angular/core';
import { MatSnackBar, Sort } from '@angular/material';
import { UserPreview } from '../../models/previews/user.interface';
import { SettingsUsersService } from '../_services/users.service';
import { NotificationClass } from '../../shared/classes/notification';
import { UserCategoryFilter, UserCategoryFilterSearchOption, UserCategoryFilterSearch } from './filters/filters.interface';
import { Router } from '@angular/router';
import { AuthService, LoginResponse } from 'src/app/shared/services/auth.service';
import { CategoryEnum } from 'src/app/models/enums/category.enum';
import { SharedService } from 'src/app/shared/services/shared.service';
import { SettingsEventsService } from '../_services/events.service';
import { ExcelService } from 'src/app/shared/services/excel.service';
import { environment } from 'src/environments/environment';
import { UserSummary } from '../users/user-models/user-summary';
import { UserPreviewInterceptor } from 'src/app/shared/interfaces/user-preview-interceptor.interface';


@Component({
  selector: 'app-settings-manage-user-register',
  templateUrl: './manage-user-register.component.html',
  styleUrls: ['./manage-user-register.component.scss']
})
export class SettingsManageUserRegisterComponent extends NotificationClass implements OnInit {

  public readonly pageSize: number = 20;
  public readonly displayedColumns: string[] = [
    'manage', 'logo', 'name', 'userName', 'registerId', 'responsible', 'manager', 'seeCard', 'blocked', 'impersonate'
  ];
  public filters: Array<UserCategoryFilter> = [];
  public users: Array<UserPreview> = [];
  public usersCount: number = 0;
  public selectedUsers: Array<UserPreview> = [];
  public showAllSelectedUsers: boolean = false;
  public hasProfileTest: boolean = environment.features.profileTest;
  public isBTG: boolean = true;
  public user: LoginResponse;
  public blocked: UserSummary;
  public hasCareer: boolean = environment.features.career;

  private _usersPage: number = 1;
  private _searchValue: string;
  private _selectedFilters: Array<UserCategoryFilterSearchOption> = [];
  private _createdSince: Date = null;
  private _sortDirection: boolean = null;
  private _sortActive: string = '';

  constructor(
    protected _snackBar: MatSnackBar,
    private _usersService: SettingsUsersService,
    private _router: Router,
    private _sharedService: SharedService,
    private _eventsService: SettingsEventsService,
    private _excelService: ExcelService,
    private _authService: AuthService
  ) {
    super(_snackBar);
    _authService.isLoggedIn().subscribe((x) => {
      this.user = _authService.getLoggedUser();
    });
  }

  ngOnInit() {

    this.isBTG = environment.logo === 'btg' ? true : false;

    if (localStorage.getItem('filters-manage-team') && localStorage.getItem('filters-manage-team-selected')) {
      this.filters = JSON.parse(localStorage.getItem('filters-manage-team'));
      console.log('this.filters -> ', this.filters);
      this._selectedFilters = JSON.parse(localStorage.getItem('filters-manage-team-selected'));
      this.updateFilters(this.filters);
    } else {
      this._getCategories();
    }
  }

  public triggerSearch(searchValue: string) {
    this._searchValue = searchValue;
    this._loadUsers(this._usersPage, this._searchValue);
  }

  public goToPage(page: number) {
    if (page !== this._usersPage) {
      this._usersPage = page;
      this._loadUsers(this._usersPage, this._searchValue);
    }
  }

  public updateFilters(filters: Array<UserCategoryFilter>) {

    console.log('updateFilters - filter -> ', filters);

    for (let i = 0; i < filters.length; i++) {
      this._selectedFilters[i].contentNames = [];
      filters[i].selectedOptions.forEach(opt => {
        const value = filters[i].demandsId ? opt.id : opt.description;
        this._selectedFilters[i].contentNames.push(value);
      });
    }
    this._loadUsers(this._usersPage, this._searchValue);
    localStorage.setItem('filters-manage-team', JSON.stringify(filters));
    localStorage.setItem('filters-manage-team-selected', JSON.stringify(this._selectedFilters));
  }

  public sortData(sort: Sort) {
    if (sort.direction === 'asc') {
      this._sortActive = sort.active;
      this._sortDirection = true;
      this._loadUsers(
        this._usersPage,
        this._searchValue
      );
    } else if (sort.direction === 'desc') {
      this._sortActive = sort.active;
      this._sortDirection = false;
      this._loadUsers(
        this._usersPage,
        this._searchValue
      );
    } else {
      this._sortActive = '';
      this._sortDirection = null;
      this._loadUsers(
        this._usersPage,
        this._searchValue
      );
    }
  }

  public hasSelectedUsers(): boolean {
    return this.selectedUsers && this.selectedUsers.length > 0;
  }

  public selectUser(event: any, user: UserPreview) {
    user.checked = event.checked;
    if (event.checked) {
      this.selectedUsers.push(user);
    } else {
      this.selectedUsers.splice(this.selectedUsers.findIndex(x => x.id === user.id), 1);
    }
  }

  public removeSelectedUser(user: UserPreview) {
    this.selectedUsers.splice(this.selectedUsers.findIndex(x => x.id === user.id), 1);
    const userIndex = this.users.findIndex(x => x.id === user.id);
    if (userIndex >= 0) this.users[userIndex].checked = false;
  }

  public cleanSelectedUsers(): void {
    this.users.forEach(user => user.checked = false);
    this.selectedUsers = [];
  }

  public selectAllUsers(): void {
    this._loadUsers(this._usersPage, this._searchValue, true);
  }

  public goToRecommendation(): void {
    if (!this.hasSelectedUsers()) {
      this.notify('Selecione usuários para poder prosseguir');
    } else {
      localStorage.setItem('SelectedUsers',
        JSON.stringify(this.selectedUsers)
      );
      this._router.navigate(['/configuracoes/enturmacao-matricula']);
    }
  }

  public goToSendCustomEmail(): void {
    if (!this.hasSelectedUsers()) {
      this.notify('Selecione usuários para poder prosseguir');
    } else {
      localStorage.setItem('SelectedUsers',
        JSON.stringify(this.selectedUsers)
      );
      this._router.navigate(['/configuracoes/enviar-email']);
    }
  }

  public goToSuggestTest(): void {
    if (!this.hasSelectedUsers()) {
      this.notify('Selecione usuários para poder prosseguir');
    } else {
      localStorage.setItem('SelectedUsers',
        JSON.stringify(this.selectedUsers)
      );
      this._router.navigate(['/configuracoes/indicar-teste']);
    }
  }

  public exportUsersCareer() {
    if (!this.hasSelectedUsers()) {
      this.notify('Selecione usuários para poder prosseguir');
    } else {
      const selectedUsers = this.selectedUsers.map(x => { return {
        userId: x.id,
        name: x.name,
        email: x.email
      }; });
      this._usersService.exportUsersCareer(selectedUsers).subscribe(res => {
        this._excelService.exportAsExcelFile(res.data.map(x => this.flattenObject(x)), 'Carreiras');
      }, err => { this.notify(this.getErrorNotification(err)); });
    }
  }

  public exportUsersGrade() {
    if (!this.hasSelectedUsers()) {
      this.notify('Selecione usuários para poder prosseguir');
    } else {
      const selectedUsers = this.selectedUsers.map(x => x.id);

      console.log('exportUsersGrade - selectedUsers -> ', selectedUsers);

      this._usersService.exportUsersGrade(selectedUsers).subscribe(res => {
        this._excelService.exportAsExcelFile(
          this._excelService.buildExportUsersGrade(res),
          'Notas'
        );
      }, err => { this.notify(this.getErrorNotification(err)); });
    }
  }

  public exportUsersEffectiveness() {
    if (!this.hasSelectedUsers()) {
      this.notify('Selecione usuários para poder prosseguir');
    } else {
      const selectedUsers = this.selectedUsers.map(x => { return {
        userId: x.id,
        name: x.name,
        email: x.email
      }; });
      this._usersService.exportUsersEffectiveness(selectedUsers).subscribe(res => {
        this._excelService.exportAsExcelFile(
          this._excelService.buildExportUsersEffectiveness(res),
          'Efetividade'
        );
      }, err => { this.notify(this.getErrorNotification(err)); });
    }
  }

  private flattenObject(obj): any {
    const toReturn = {};

    for (const i in obj) {
        if (!obj.hasOwnProperty(i)) continue;

        if ((typeof obj[i]) === 'object' && obj[i] !== null) {
            const flatObject = this.flattenObject(obj[i]);
            for (const x in flatObject) {
                if (!flatObject.hasOwnProperty(x)) continue;

                toReturn[i + '.' + x] = flatObject[x];
            }
        } else {
            toReturn[i] = obj[i];
        }
    }
    return toReturn;
  }

  public viewUserDetails(user: UserPreview) {
    this._router.navigate([ '/configuracoes/detalhes-usuario/' + user.id ]);
  }

  private _loadUsers(page: number, searchValue: string = '', allusers: boolean = false): void {
    this._usersService.getFilteredUserToManage(
      page, this.pageSize,
      searchValue, this._createdSince,
      this._selectedFilters,
      this.user.have_dependents ? this.user.name : '',
      this._sortActive, this._sortDirection,
      allusers
    ).subscribe((response) => {
      if (!allusers) {
        this.users = response.data.userItems;
        this.usersCount = response.data.itemsCount;
        this.selectedUsers.forEach(selectedUser => {
          const userIndex = this.users.findIndex(x => x.id === selectedUser.id);
          if (userIndex >= 0) this.users[userIndex].checked = true;
        });
      } else {
        this.selectedUsers = response.data.userItems;
        this.selectedUsers.forEach(selectedUser => {
          const userIndex = this.users.findIndex(x => x.id === selectedUser.id);
          if (userIndex >= 0) this.users[userIndex].checked = true;
        });
        if (this.selectedUsers.length > 4) {
          this.showAllSelectedUsers = false;
        }
      }
    }, () => this.notify('Ocorreu um erro, por favor tente novamente mais tarde'));
  }

  public triggerFilterSearch(search: UserCategoryFilterSearch) {
    if (search.filter.filterColumn === 'event.id') {
      this._eventsService.getPastEvents(
        search.page, search.value
      ).subscribe((response) => {
        this._resolveFilterSearch(search, response);
      });
    } else {
      this._usersService.getUserCategory(
        search.filter.category, search.value, search.page
      ).subscribe((response) => {
        this._resolveFilterSearch(search, response);
      });
    }
  }

  private _resolveFilterSearch(search: UserCategoryFilterSearch, response): void {
    const filterIndex = this.filters.findIndex(x =>
      x.name === search.filter.name
    );

    if (filterIndex !== -1) {
      this.filters[filterIndex].page = search.filter.page;
      this.filters[filterIndex].itemsCount = response.data.itemsCount;
      this.filters[filterIndex].options = search.page > 1 ? this.filters[filterIndex].options : [];
      for (let index = 0; index < response.data.items.length; index++) {
        this.filters[filterIndex].options.push({
          id: response.data.items[index].id,
          description: response.data.items[index][search.prop],
          checked: this.filters[filterIndex].selectedOptions.findIndex(x => x.id === response.data.items[index].id) !== -1
        });
      }
      this.filters[filterIndex].maxLength = this.filters[filterIndex].options.length;
    }
  }

  public setCreatedSince(createdSince: Date) {
    this._createdSince = createdSince;
    this._loadUsers(this._usersPage, this._searchValue);
  }

  public setUserTermFilter(term: string) {
    this._loadUsers(this._usersPage, term);
  }

  private _getCategories() {

    const userTypes: any[] = [
      {id: 'student', name: 'Estudante'},
      {id: 'secretary', name: 'Secretaria'},
      {id: 'humanResources', name: 'RH'},
      {id: 'recruiter', name: 'Recrutador'},
      {id: 'admin', name: 'Admin'},
      {id: 'blocked', name: 'Bloqueados'},
      {id: 'notBlocked', name: 'Não Bloqueados'}
    ];

    this._setFilter('Perfis de Usuário', CategoryEnum.Users, 'userType',
    userTypes.length, userTypes, 'name', true, true);

    // DELETE THIS FILTERS FROM PSK, IT JUST FOR BTG
    if (this.isBTG) {

      this._usersService.getUserCategory(CategoryEnum.BusinessGroups).subscribe((response) => {
        this._setFilter('Business Group', CategoryEnum.BusinessGroups, 'businessGroup.name',
        response.data.itemsCount, response.data.items);
      });

      this._usersService.getUserCategory(CategoryEnum.BusinessUnits).subscribe((response) => {
        this._setFilter('Business Unit', CategoryEnum.BusinessUnits, 'businessUnit.name',
        response.data.itemsCount, response.data.items);
      });

      this._usersService.getUserCategory(CategoryEnum.FrontBackOffices).subscribe((response) => {
        this._setFilter('Front Back Office', CategoryEnum.FrontBackOffices, 'frontBackOffice.name',
        response.data.itemsCount, response.data.items);
      });

      this._usersService.getUserCategory(CategoryEnum.Jobs).subscribe((response) => {
        this._setFilter('Jobs', CategoryEnum.Jobs, 'job.name',
        response.data.itemsCount, response.data.items);
      });

      this._usersService.getUserCategory(CategoryEnum.Ranks).subscribe((response) => {
        this._setFilter('Ranks', CategoryEnum.Ranks, 'rank.name',
        response.data.itemsCount, response.data.items);
      });

      this._usersService.getUserCategory(CategoryEnum.Sectors).subscribe((response) => {
        this._setFilter('Sector', CategoryEnum.Sectors, 'sector.name',
        response.data.itemsCount, response.data.items);
      });

      this._usersService.getUserCategory(CategoryEnum.Segments).subscribe((response) => {
        this._setFilter('Segment', CategoryEnum.Segments, 'segment.name',
        response.data.itemsCount, response.data.items);
      });
    }
    // DELETE THIS FILTERS FROM PSK, IT JUST FOR BTG

    this._usersService.getUserCategory(CategoryEnum.Tracks).subscribe((response) => {
      this._setFilter('Trilhas Associadas', CategoryEnum.Tracks, 'track.id',
      response.data.itemsCount, response.data.items, 'title', true);
    });

    this._usersService.getUserCategory(CategoryEnum.Modules).subscribe((response) => {
      this._setFilter('Módulos Iniciados', CategoryEnum.Modules, 'module.id',
      response.data.itemsCount, response.data.items, 'title', true);
    });

    this._eventsService.getPastEvents().subscribe((response) => {
      this._setFilter('Participações em Eventos', CategoryEnum.PastEvents, 'event.id',
      response.data.itemsCount, response.data.items, 'title', true);
    });

    this._loadLevels();
    this._loadUsers(this._usersPage);
  }

  private _setFilter(
    filterName: string, category: number, filterColumn: string,
    itemsCount: number, collection: any[], prop: string = 'name',
    demandsId: boolean = false, hideSearch: boolean = false
  ) {
    const newFilter: UserCategoryFilter = {
      name: filterName,
      category: category,
      filterColumn: filterColumn,
      page: 1,
      itemsCount: itemsCount,
      maxLength: collection.length,
      options: [],
      selectedOptions: [],
      isAlternate: prop === 'title',
      demandsId: demandsId,
      hideSearch: hideSearch
    };
    for (let index = 0; index < collection.length; index++) {
      newFilter.options.push({
        id: collection[index].id,
        description: collection[index][prop],
        checked: false
      });
    }

    let arrayPosition = 0;

    if (this.filters.length > 0) {
      arrayPosition = this.filters.filter(f =>
        f.category < category
      ).length;
    }

    this.filters.splice(arrayPosition, 0, newFilter);
    this._selectedFilters.splice(
      arrayPosition, 0,
      { columnName: filterColumn, contentNames: [] }
    );
  }

  private _loadLevels(): void {
    this._sharedService.getLevels(true).subscribe((response) => {
      this._setFilter(
        'Badges em Módulos', CategoryEnum.Badges, 'level.id',
        response.data.length, response.data, 'description', true, true
      );
    }, () => { });
  }

  public canCreateUser(): boolean {
    const user = this._authService.getLoggedUser();
    return user && user.role && (
      user.role === 'Admin' || user.role === 'HumanResources'
    );
  }

  public updateUsersResponsible() {
    this._usersService.generateResponsibleTree().subscribe(() => {
      this.notify('Associação de responsavel criada com sucesso');
    }, () => this.notify('Ocorreu um erro, por favor tente novamente mais tarde'));
  }

  public manageUser(user: UserPreview): void {
    this._router.navigate(['/configuracoes/detalhes-usuario/' + user.id]);
  }

  public goToRecomendation(user: UserPreview): void {
      this._router.navigate([ '/configuracoes/card-recomendacao/' + user.id ]);
  }

  public blockUser(user: UserSummary) {
    if (!user || !user.id) { return; }

    this._usersService.changeUserBlockedStatus(
      user.id.toString()
    ).subscribe(() => {
      user.isBlocked = !user.isBlocked;
      this.notify(`Usuario ${user.isBlocked ? 'bloqueado' : 'desbloqueado' } com sucesso`);
      this._loadUsers(this._usersPage, this._searchValue );
    }, (error) => this.notify( this.getErrorNotification(error) ) );
  }

  public impersonateUser(user: UserSummary) {
    if (!user || !user.id) { return; }

    this._usersService.getUserToImpersonate(
      user.id.toString()
    ).subscribe(res => {
      this._authService.setImpersonationInfo(res.data);
      this._router.navigate(['home']);
    }, (error) => this.notify( this.getErrorNotification(error) ) );
  }
}
