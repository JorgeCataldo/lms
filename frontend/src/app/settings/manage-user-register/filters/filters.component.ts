import { Component, Input, EventEmitter, Output } from '@angular/core';
import { UserCategoryFilter, CategoryFilterOption, UserCategoryFilterSearch } from './filters.interface';

@Component({
  selector: 'app-manage-user-register-filters',
  template: `
    <div class="filters" >
      <p class="header" >
        FILTROS POR CATEGORIA
      </p>

      <div class="chips" >
        <ng-container *ngFor="let filter of filters" >
          <p *ngFor="let option of filter.selectedOptions" >
            {{ filter.name }}: {{ option.description }}
            <span (click)="removeFilter(filter, option)" >X</span>
          </p>
        </ng-container>
      </div>

      <mat-accordion>

        <mat-expansion-panel hideToggle="true" disabled="true">
          <mat-expansion-panel-header>
            <mat-panel-title>
              <span class="cat-title" >
                FILTROS DE BUSCA
              </span>
            </mat-panel-title>
          </mat-expansion-panel-header>
        </mat-expansion-panel>

        <mat-expansion-panel *ngIf="userActive" [expanded]="true">
          <mat-expansion-panel-header>
            <mat-panel-title>
              <span class="cat" >
                BUSCAR POR NOME
              </span>
            </mat-panel-title>
          </mat-expansion-panel-header>
          <app-list-search
            [noPadding]="true"
            (triggerSearch)="triggerUserSearch($event)"
          ></app-list-search>
        </mat-expansion-panel>

        <mat-expansion-panel *ngIf="dateActive">
          <mat-expansion-panel-header >
            <mat-panel-title>
              <span class="cat" >
                Data de Criação
              </span>
            </mat-panel-title>
          </mat-expansion-panel-header>
          <mat-form-field>
            <input matInput
              [(ngModel)]="createdSince"
              [matDatepickerFilter]="maxDateFilter"
              [matDatepicker]="sincePicker"
              (ngModelChange)="setCreatedSince.emit(createdSince)"
              placeholder="Desde de..."
            />
            <mat-datepicker-toggle matSuffix [for]="sincePicker"></mat-datepicker-toggle>
            <mat-datepicker #sincePicker></mat-datepicker>
          </mat-form-field>
        </mat-expansion-panel>

        <mat-expansion-panel *ngFor="let filter of filters" >
          <mat-expansion-panel-header>
            <mat-panel-title>
              <span class="cat" >
                {{ filter.name }}
              </span>
            </mat-panel-title>
          </mat-expansion-panel-header>

          <div class="options" >
            <app-list-search
              *ngIf="!filter.hideSearch"
              placeholder="filtrar resultados"
              [noPadding]="true"
              (triggerSearch)="triggerSearch($event, filter)"
            ></app-list-search>
            <ng-container *ngFor="let option of filter.options; let i = index">
              <mat-checkbox
                [(ngModel)]="option.checked"
                (ngModelChange)="setFilters(filter, option)"
              >
                <input class="custom-input" *ngIf="option.customInput"
                  type="text"
                  (change)="inputChange($event.target.value, option)"
                />
                <ng-container *ngIf="!option.customInput">{{option.description}}</ng-container>
              </mat-checkbox>
            </ng-container>
            <p
              *ngIf="filter.maxLength < filter.itemsCount"
              (click)="moreItens(filter)"
              style="cursor: pointer; text-decoration: underline;"
            >
              Carregar mais...
            </p>
            <p
              *ngIf="filter.haveCustomInput"
              (click)="moreInputs(filter)"
              style="cursor: pointer; text-decoration: underline;"
            >
              Adicionar mais...
            </p>
          </div>
        </mat-expansion-panel>
      </mat-accordion>
    </div>`,
  styleUrls: ['./filters.component.scss']
})
export class ManageUserRegisterFiltersComponent {

  @Input() filters: Array<UserCategoryFilter> = [];
  @Input() userActive = true;
  @Input() dateActive = true;
  @Output() updateFilters: EventEmitter<Array<UserCategoryFilter>> = new EventEmitter();
  @Output() search: EventEmitter<UserCategoryFilterSearch> = new EventEmitter();
  @Output() setCreatedSince: EventEmitter<Date> = new EventEmitter();
  @Output() setUserTermFilter: EventEmitter<string> = new EventEmitter();

  public createdSince: Date;
  private _searchValue: string  = '';

  public removeFilter(filter: UserCategoryFilter, opt: CategoryFilterOption) {
    const index = filter.options.findIndex(x => x.id === opt.id);
    if (index !== -1) {
      filter.options[index].checked = false;
    }
    this.setFilters(filter, opt);
  }

  public setFilters(filter: UserCategoryFilter, opt: CategoryFilterOption) {
    const index = filter.selectedOptions.findIndex(x => x.id === opt.id);
    if (index === -1) {
      filter.selectedOptions.push(opt);
    } else {
      filter.selectedOptions.splice(index, 1);
    }
    this.updateFilters.emit(
      this.filters
    );
  }

  public triggerUserSearch(searchValue: string) {
    this.setUserTermFilter.emit(searchValue);
  }

  public triggerSearch(searchValue: string, filter: UserCategoryFilter) {
    this._searchValue =  searchValue;
    filter.page = 1;
    this.search.emit(
      {
        value: searchValue,
        filter: filter,
        page: filter.page,
        prop: filter.isAlternate ? 'title' : 'name'
      }
    );
  }

  public moreItens(filter: UserCategoryFilter) {
    filter.page = filter.page + 1;
    this.search.emit(
      {
        value: this._searchValue,
        filter: filter,
        page: filter.page,
        prop: filter.isAlternate ? 'title' : 'name'
      }
    );
  }

  public maxDateFilter(currentDate: Date): boolean {
    const today = new Date();
    return currentDate <= today;
  }

  public inputChange(value: string, opt: CategoryFilterOption) {
    opt.description = value;
    opt.id = value;
  }

  public moreInputs(filter: UserCategoryFilter) {
    filter.options.push(
      {
        customInput: true,
        checked: false,
        id: '',
        description: ''
      }
    );
  }
}
