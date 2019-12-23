import { Component, OnInit } from '@angular/core';
import { MatSnackBar, Sort } from '@angular/material';
import { Router } from '@angular/router';
import { AuthService } from 'src/app/shared/services/auth.service';
import { CategoryEnum, CareerEnum } from 'src/app/models/enums/category.enum';
import { SharedService } from 'src/app/shared/services/shared.service';
import { environment } from 'src/environments/environment';
import { NotificationClass } from 'src/app/shared/classes/notification';
import {
  UserCategoryFilter,
  UserCategoryFilterSearchOption,
  UserCategoryFilterSearch
} from 'src/app/settings/manage-team/filters/filters.interface';
import { UserPreview } from 'src/app/models/previews/user.interface';
import { SettingsUsersService } from 'src/app/settings/_services/users.service';
import { SettingsEventsService } from 'src/app/settings/_services/events.service';
import { RecruitingCompanyService } from '../_services/recruiting-company.service';
import { JobApplication } from 'src/app/models/previews/user-job-application.interface';


@Component({
    selector: 'app-manage-talents',
    templateUrl: './manage-talents.component.html',
    styleUrls: ['./manage-talents.component.scss']
})

export class RecruitmentManageTalentsComponent extends NotificationClass implements OnInit {

  public readonly pageSize: number = 20;
  public readonly displayedColumns: string[] = [
    'manage', 'logo', 'name', 'registerId', 'responsible', 'favorite', 'arrow'
  ];
  public filters: Array<UserCategoryFilter> = [];
  public users: Array<UserPreview> = [];
  public usersCount: number = 0;
  public selectedUsers: any[] = [];
  public showAllSelectedUsers: boolean = false;
  public hasProfileTest: boolean = environment.features.profileTest;
  public jobApplication: JobApplication;
  public mandatoryFields: boolean = true;

  private _usersPage: number = 1;
  private _searchValue: string;
  private _selectedFilters: Array<UserCategoryFilterSearchOption> = [];
  private _sortDirection: boolean = null;
  private _sortActive: string = '';

  constructor(
    protected _snackBar: MatSnackBar,
    private _usersService: SettingsUsersService,
    private _router: Router,
    private _sharedService: SharedService,
    private _eventsService: SettingsEventsService,
    private _recruitmentService: RecruitingCompanyService,
    private _authService: AuthService
  ) {
    super(_snackBar);
  }

  ngOnInit() {
    this.jobApplication = JSON.parse(localStorage.getItem('jobApplication'));
    this._getCategories();
  }

  public triggerSearch(searchValue: string) {
    this._searchValue = searchValue;
    this._loadUsers(this._usersPage, this._searchValue);
  }

  public setUserTermFilter(term: string) {
    this._loadUsers(this._usersPage, term);
  }

  public goToPage(page: number) {
    if (page !== this._usersPage) {
      this._usersPage = page;
      this._loadUsers(this._usersPage, this._searchValue);
    }
  }

  public updateFilters(filters: Array<UserCategoryFilter>) {
    for (let i = 0; i < filters.length; i++) {
      this._selectedFilters[i].contentNames = [];
      filters[i].selectedOptions.forEach(opt => {
        const value = filters[i].demandsId ? opt.id : opt.description;
        this._selectedFilters[i].contentNames.push(value);
      });
    }
    this._loadUsers(this._usersPage, this._searchValue);
    localStorage.setItem('filters-manage-talents', JSON.stringify(filters));
    localStorage.setItem('filters-manage-talents-selected', JSON.stringify(this._selectedFilters));
  }

  public sortData(sort: Sort) {
    if (sort.direction === 'asc') {
      this._sortActive = sort.active;
      this._sortDirection = true;
    } else if (sort.direction === 'desc') {
      this._sortActive = sort.active;
      this._sortDirection = false;
    } else {
      this._sortActive = '';
      this._sortDirection = null;
    }
    this._loadUsers(this._usersPage, this._searchValue);
  }

  public viewUserDetails(user: UserPreview) {
    this._router.navigate([ '/configuracoes/card-recomendacao/' + user.id ]);
  }

  private _loadUsers(page: number, searchValue: string = '', allusers: boolean = false): void {
    this._recruitmentService.getTalentsList(
      page, this.pageSize, searchValue,
      this._selectedFilters,
      this._sortActive, this._sortDirection,
      this.mandatoryFields, allusers
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
        if (this.selectedUsers.length > 4)
          this.showAllSelectedUsers = false;
      }
    }, (err) => this.notify(this.getErrorNotification(err)));
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

  private _getCategories() {
    if (localStorage.getItem('filters-manage-talents') && localStorage.getItem('filters-manage-talents-selected')) {
      this.filters = JSON.parse(localStorage.getItem('filters-manage-talents'));
      this._selectedFilters = JSON.parse(localStorage.getItem('filters-manage-talents-selected'));
      this.updateFilters(this.filters);
    } else {
      if (this.isAdmin()) {
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
      }
      this._setCareerFilters();
      this._loadUsers(this._usersPage);
    }
  }

  private _setCareerFilters() {

    const singleOption = [
      {id: 'true', name: 'Sim'}
    ];

    const booleanOptions = [
      {id: 'true', name: 'Sim'},
      {id: 'false', name: 'Não'}
    ];

    const customOption = [
      {id: '', name: '', customInput: true}
    ];

    const collegeDegree = [
      {id: 'Graduação', name: 'Graduação'},
      {id: 'Pós-Graduação', name: 'Pós-Graduação'},
      {id: 'Mestrado', name: 'Mestrado'},
      {id: 'Doutorado', name: 'Doutorado'}
    ];

    const yearRange: number = 6;
    const yearHalf: number = yearRange / 2;
    const currentYear: number = new Date().getFullYear();
    const yearOptions = [];
    for (let i = 0; i <= yearRange; i++) {
      const yearAdd = currentYear - (yearHalf - i);
      const yearAddString = yearAdd.toString();
      yearOptions.push(
        {id: yearAddString + '.1', name: yearAddString + '.1'}
      );
      yearOptions.push(
        {id: yearAddString + '.2', name: yearAddString + '.2'}
      );
    }

    const complementaryFormations = [
      {id: 'VBA', name: 'VBA'},
      {id: 'Excel', name: 'Excel'},
      {id: 'Software Estatístico', name: 'Software Estatístico'},
      {id: 'Pacote Office', name: 'Pacote Office'},
      {id : '', name: '', customInput: true}
    ];

    const complementaryFormationsLevels = [
      {id: 'Básico', name: 'Básico'},
      {id: 'Intermediário', name: 'Intermediário'},
      {id: 'Avançado', name: 'Avançado'}
    ];

    const proseekPerfils = [
      {id: 'ANALÍTICO', name: 'ANALÍTICO'},
      {id: 'TEAM-PLAYER', name: 'TEAM-PLAYER'},
      {id: 'ASSERTIVO', name: 'ASSERTIVO'}
    ];

    const proseekVies = [
      {id: 'PROPOSITIVO', name: 'PROPOSITIVO'},
      {id: 'QUESTIONADOR', name: 'QUESTIONADOR'},
      {id: 'REATIVO', name: 'REATIVO'},
      {id: 'PROATIVO', name: 'PROATIVO'}
    ];

    const complementaryLanguagesLevels = [
      {id: 'Básico', name: 'Básico'},
      {id: 'Intermediário', name: 'Intermediário'},
      {id: 'Avançado', name: 'Avançado'},
      {id: 'Fluente', name: 'Fluente'}
    ];

    this._setFilter('Favoritos', CategoryEnum.Users, 'isFavorite',
      booleanOptions.length, booleanOptions, 'name', true, true);

    this._setFilter('CR acima de', CareerEnum.College, 'hist.cr',
      customOption.length, customOption, 'name', true, true);
    this._setFilter('Ano/Período de Conclusão', CareerEnum.College, 'hist.year',
      yearOptions.length, yearOptions, 'name', true, true);
    this._setFilter('Grau do curso', CareerEnum.College, 'hist.degree',
      collegeDegree.length, collegeDegree, 'name', true, true);
    this._setFilter('Nome do curso', CareerEnum.College, 'hist.collegeName',
      customOption.length, customOption, 'name', true, true, true);

    this._setFilter('Nível de Formação Complementar', CareerEnum.ComplementaryFormation, 'compForm.level',
      complementaryFormationsLevels.length, complementaryFormationsLevels, 'name', true, true);
    this._setFilter('Formação Complementar', CareerEnum.ComplementaryFormation, 'compForm.name',
      complementaryFormations.length, complementaryFormations, 'name', true, true, true);
    this._setFilter('Mudança de residência', CareerEnum.ComplementaryInfo, 'compInfo.travel',
      singleOption.length, singleOption, 'name', true, true);
    this._setFilter('Certificações', CareerEnum.ComplementaryInfo, 'compInfo.certificates',
      customOption.length, customOption, 'name', true, true, true);
    this._setFilter('Nível de Idiomas', CareerEnum.ComplementaryInfo, 'compInfo.languages.level',
      complementaryLanguagesLevels.length, complementaryLanguagesLevels, 'name', true, true);
    this._setFilter('Idiomas', CareerEnum.ComplementaryInfo, 'compInfo.languages.name',
      customOption.length, customOption, 'name', true, true, true);
    this._setFilter('Competências', CareerEnum.ComplementaryInfo, 'compInfo.skills',
      customOption.length, customOption, 'name', true, true, true);

    if (this.isAdmin()) {
      this._setFilter('Vies', CareerEnum.PerfilProseek, 'proseek.vies',
        proseekVies.length, proseekVies, 'name', true, true);
      this._setFilter('Perfil de usuário', CareerEnum.PerfilProseek, 'proseek.perfil',
        proseekPerfils.length, proseekPerfils, 'name', true, true);
    }

    if (localStorage.getItem('isFavorited')) {
      localStorage.removeItem('isFavorited');
      const filterIndex = this.filters.findIndex(x => x.filterColumn === 'isFavorite');
      if (filterIndex !== -1) {
        const optionIndex = this.filters[filterIndex].options.findIndex(x => x.id === 'true');
        if (optionIndex !== -1) {
          this.filters[filterIndex].options[optionIndex].checked = true;
          this.filters[filterIndex].selectedOptions.push(
            this.filters[filterIndex].options[optionIndex]
          );
          this.updateFilters(this.filters);
        }
      }
    }
  }

  private _setFilter(
    filterName: string, category: number, filterColumn: string,
    itemsCount: number, collection: any[], prop: string = 'name',
    demandsId: boolean = false, hideSearch: boolean = false,
    multipleCustomInput: boolean = false
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
      hideSearch: hideSearch,
      haveCustomInput: multipleCustomInput
    };

    for (let index = 0; index < collection.length; index++) {
      newFilter.options.push({
        id: collection[index].id,
        description: collection[index][prop],
        checked: false,
        customInput: collection[index].customInput
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

  public selectAllUsers(): void {
    this._loadUsers(this._usersPage, this._searchValue, true);
  }

  public cleanSelectedUsers(): void {
    this.users.forEach(user => user.checked = false);
    this.selectedUsers = [];
  }

  public removeSelectedUser(user: UserPreview) {
    this.selectedUsers.splice(this.selectedUsers.findIndex(x => x.id === user.id), 1);
    const userIndex = this.users.findIndex(x => x.id === user.id);
    if (userIndex >= 0) this.users[userIndex].checked = false;
  }

  public selectUser(event: boolean, user: UserPreview) {
    user.checked = event;
    if (event) {
      this.selectedUsers.push(user);
    } else {
      this.selectedUsers.splice(this.selectedUsers.findIndex(x => x.id === user.id), 1);
    }
  }

  public addApplicants() {
    if (this.selectedUsers.length === 0) {
      this.notify('Selecione usuários para poder prosseguir');
    } else {
      const candidates = this.selectedUsers.map(x => ({'UserId': x.id, 'UserName': x.name}));
      this._recruitmentService.addCandidateToJobPosition(candidates, this.jobApplication.id).subscribe(() => {
        this._router.navigate(['configuracoes/gerenciar-inscricoes-vagas/' + this.jobApplication.id]);
      }, err => {
        this.notify(this.getErrorNotification(err));
      });
    }
  }

  public isAdmin() {
    const type = this._authService.getLoggedUserRole();
    return type === 'Admin';
  }

  public changeMandatoryFields() {
    this.mandatoryFields = !this.mandatoryFields;
    this._loadUsers(this._usersPage, this._searchValue);
  }
}
