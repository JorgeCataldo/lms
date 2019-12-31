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
import { RecruitingCompanyService } from '../../_services/recruiting-company.service';
import { JobApplication, JobPosition, JobListItem } from 'src/app/models/previews/user-job-application.interface';


@Component({
    selector: 'app-student-search-job',
    templateUrl: './student-search-job.component.html',
    styleUrls: ['./student-search-job.component.scss']
})

export class StudentSearchJobComponent extends NotificationClass implements OnInit {

  public readonly pageSize: number = 20;
  public filters: Array<UserCategoryFilter> = [];
  public customFilters: Array<UserCategoryFilter> = [];
  public regularFilters: Array<UserCategoryFilter> = [];
  public jobs: JobListItem[] = [];
  public jobsCount: number = 0;

  private _jobsPage: number = 1;
  private _searchValue: string;
  private _selectedFilters: Array<UserCategoryFilterSearchOption> = [];
  public userId: string;

  constructor(
    protected _snackBar: MatSnackBar,
    private _usersService: SettingsUsersService,
    private _router: Router,
    private _eventsService: SettingsEventsService,
    private _authService: AuthService,
    private _recruitmentService: RecruitingCompanyService
  ) {
    super(_snackBar);
  }

  ngOnInit() {
    this.userId = this._authService.getLoggedUserId();
    this._setCareerFilters();
    this._loadJobs();
  }

  public triggerSearch(searchValue: string) {
    this._searchValue = searchValue;
    this._loadJobs();
  }

  public goToPage(page: number) {
    if (page !== this._jobsPage) {
      this._jobsPage = page;
      this._loadJobs();
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
    this._loadJobs();
    localStorage.setItem('filters-student-search-job', JSON.stringify(filters));
    localStorage.setItem('filters-student-search-job-selected', JSON.stringify(this._selectedFilters));
  }

  public viewUserDetails(user: UserPreview) {
    this._router.navigate([ '/configuracoes/card-recomendacao/' + user.id ]);
  }

  private _loadJobs(): void {
    this._recruitmentService.getJobsList(
      this._jobsPage, this.pageSize, this._searchValue,
      this._selectedFilters
    ).subscribe((response) => {
      this.jobs = response.data.jobItems;
      this.jobsCount = response.data.itemsCount;
    }, (err) => this.notify( this.getErrorNotification(err) ));
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

  private _setCareerFilters() {

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

    const complementaryLanguageLevels = [
      {id: 'Básico', name: 'Básico'},
      {id: 'Intermediário', name: 'Intermediário'},
      {id: 'Avançado', name: 'Avançado'},
      {id: 'Fluente', name: 'Fluente'}
    ];

    const jobStatus = [
      {id: 'Pending', name: 'Pendentes de ação'},
      {id: 'Candidate', name: 'Já sou candidato'}
    ];

    this._setFilter('Ações', -1, 'candidate.type',
      jobStatus.length, jobStatus, 'name', true, true, false);

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
    this._setFilter('Certificações', CareerEnum.ComplementaryInfo, 'compInfo.certificates',
      customOption.length, customOption, 'name', true, true, true);
    this._setFilter('Nível de Idiomas', CareerEnum.ComplementaryInfo, 'compInfo.languages.level',
      complementaryLanguageLevels.length, complementaryLanguageLevels, 'name', true, true);
    this._setFilter('Idiomas', CareerEnum.ComplementaryInfo, 'compInfo.languages',
      customOption.length, customOption, 'name', true, true, true);

    if (localStorage.getItem('filters-student-search-job') && localStorage.getItem('filters-student-search-job-selected')) {
      this.filters = JSON.parse(localStorage.getItem('filters-student-search-job'));
      this.filters.forEach(filter => {
        if (filter.category >= 0) {
          this.regularFilters.push(filter);
        } else {
          this.customFilters.push(filter);
        }
      });
      this._selectedFilters = JSON.parse(localStorage.getItem('filters-student-search-job-selected'));
      this.updateFilters(this.filters);
    } else {
      this.filters.forEach(filter => {
        if (filter.category >= 0) {
          this.regularFilters.push(filter);
        } else {
          this.customFilters.push(filter);
        }
      });
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

  public applyToJob(job: JobListItem) {
    this._recruitmentService.applyTojob(job.jobPositionId).subscribe(() => {
      job.applied = true;
    }, err => {
      this.notify(this.getErrorNotification(err));
    });
  }

  public viewJobDetails(jobPositionId: string) {
    this._router.navigate(['gerenciar-vaga-alunos/' + jobPositionId]);
  }

  public checkUserId(candidateById: string): boolean {
    if (candidateById) {
      return this.userId === candidateById;
    }
    return null;
  }
}
