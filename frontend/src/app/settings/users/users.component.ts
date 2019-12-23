import { Component, OnInit } from '@angular/core';
import { MatSnackBar, Sort } from '@angular/material';
import { Router } from '@angular/router';
import { UserPreview } from '../../models/previews/user.interface';
import { SettingsUsersService } from '../_services/users.service';
import { NotificationClass } from '../../shared/classes/notification';
import { UserTab } from '../../shared/components/users-tabs/user-tab.model';
import { UserStatusEnum } from '../../models/enums/user-status.enum';
import { PagedUserItem } from './user-models/paged-user-item';
import { AuthService } from 'src/app/shared/services/auth.service';

@Component({
  selector: 'app-settings-users',
  templateUrl: './users.component.html',
  styleUrls: ['./users.component.scss']
})
export class SettingsUsersComponent extends NotificationClass implements OnInit {

  public readonly pageSize: number = 50;
  public readonly displayedColumns: string[] = [
    'logo', 'name', 'user_name', 'matricula', 'responsible', 'manage'
  ];
  public tabs: Array<UserTab> = [
    new UserTab('USUÁRIOS NO TOTAL', 'var(--primary-color)', UserStatusEnum.Active),
    new UserTab('USUÁRIOS BLOQUEADOS', 'var(--danger-color)', UserStatusEnum.Blocked)
  ];
  public users: Array<PagedUserItem> = [];
  public selectedTabIndex: number = 0;
  private _usersPage: number = 1;
  private _searchValue = '';

  constructor(
    protected _snackBar: MatSnackBar,
    private _usersService: SettingsUsersService,
    private _router: Router,
    private _authService: AuthService
  ) {
    super(_snackBar);
  }

  ngOnInit() {
    this._loadUsers(this._usersPage, UserStatusEnum.Active);
  }

  public canCreateUser(): boolean {
    const user = this._authService.getLoggedUser();
    return user && user.role && (
      user.role === 'Admin' || user.role === 'HumanResources'
    );
  }

  public selectTab(index: number) {
    this.selectedTabIndex = index;
    this._loadUsers(
      this._usersPage,
      this.tabs[this.selectedTabIndex].status
    );
  }

  public triggerSearch(searchValue: string) {
    this._searchValue = searchValue;
    this._loadUsers(
      this._usersPage,
      this.tabs[this.selectedTabIndex].status,
      searchValue
    );
  }

  public sortData(sort: Sort) {
    let direction: boolean;
    let active: string;
    if (sort.direction === 'asc') {
      active = sort.active;
      direction = true;
      this._loadUsers(
        this._usersPage,
        this.tabs[this.selectedTabIndex].status,
        this._searchValue,
        active,
        direction
      );
    } else if (sort.direction === 'desc') {
      active = sort.active;
      direction = false;
      this._loadUsers(
        this._usersPage,
        this.tabs[this.selectedTabIndex].status,
        this._searchValue,
        active,
        direction
      );
    } else {
      active = '';
      direction = null;
      this._loadUsers(
        this._usersPage,
        this.tabs[this.selectedTabIndex].status,
        this._searchValue
      );
    }
  }

  public goToPage(page: number) {
    if (page !== this._usersPage) {
      this._usersPage = page;
      this._loadUsers(
        this._usersPage,
        this.tabs[this.selectedTabIndex].status
      );
    }
  }

  public createNewUser(): void {
    this._router.navigate(['/configuracoes/usuarios/novo']);
  }

  public viewUserDetails(user: UserPreview) {
    this._router.navigate(['/configuracoes/detalhes-usuario/' + user.id]);
  }

  public manageUser(user: UserPreview): void {
    this._router.navigate(['/configuracoes/usuarios/' + user.id]);
  }

  public updateUsersResponsible() {
    this._usersService.generateResponsibleTree().subscribe(() => {
      this.notify('Associação de responsavel criada com sucesso');
    }, () => this.notify('Ocorreu um erro, por favor tente novamente mais tarde'));
  }

  private _loadUsers(page: number, status: UserStatusEnum, searchValue: string = '',
    sortValue: string = '', sortAscending: boolean = false): void {
    const blocked = status === UserStatusEnum.Blocked;
    this._usersService.getPagedFilteredUsersList(page, this.pageSize,
      searchValue, blocked, sortValue, sortAscending
    ).subscribe((response) => {
      this.users = response.data.userItems;
      this.tabs[this.selectedTabIndex].count = response.data.itemsCount;
      if (response.data.blockedItemsCount && this.selectedTabIndex === 0) {
        this.tabs[1].count = response.data.blockedItemsCount;
      }
    }, () => this.notify('Ocorreu um erro, por favor tente novamente mais tarde'));
  }

}
