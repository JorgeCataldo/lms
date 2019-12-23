import { Component, OnInit } from '@angular/core';
import { MatSnackBar } from '@angular/material';
import { NotificationClass } from '../../../shared/classes/notification';
import { UserSyncPreview, ImportUser } from '../../../models/previews/user-sync.interface';
import { UserTab } from '../../../shared/components/users-tabs/user-tab.model';
import { UserStatusEnum } from '../../../models/enums/user-status.enum';

@Component({
  selector: 'app-settings-sync-details',
  templateUrl: './sync-details.component.html',
  styleUrls: ['./sync-details.component.scss']
})
export class SettingsUsersSyncDetailsComponent extends NotificationClass implements OnInit {

  public readonly pageSize: number = 120;
  public readonly displayedColumns: string[] = [
    'imageUrl', 'name', 'jobTitle', 'responsible', 'status'
  ];
  public readonly userStatus = {
    0: { status: 'Novo Usuário', color: 'var(--semi-primary-color)' },
    1: { status: 'Usuário Atualizado', color: 'var(--warn-color)' },
    2: { status: 'Usuário Bloqueado', color: 'var(--danger-color)' }
  };
  public tabs: Array<UserTab> = [
    new UserTab('USUÁRIOS NO TOTAL', 'var(--primary-color)', UserStatusEnum.Active),
    new UserTab('USUÁRIOS NOVOS', 'var(--semi-primary-color)', UserStatusEnum.New),
    new UserTab('USUÁRIOS ATUALIZADOS', 'var(--warn-color)', UserStatusEnum.Updated),
    new UserTab('USUÁRIOS BLOQUEADOS', 'var(--danger-color)', UserStatusEnum.Blocked)
  ];
  public selectedSyncProccess: UserSyncPreview;
  public selectedTabIndex: number = 0;
  public users: Array<ImportUser> = [];
  public searchValue: string = '';
  public notSearching: boolean = true;

  private _userSyncPreview: UserSyncPreview = JSON.parse(localStorage.getItem('SyncProcess'));
  private _page: number = 0;
  private _importUsers: Array<Array<ImportUser>> = [ [], [], [], [] ];
  private _importUsersPaged: Array<Array<Array<ImportUser>>> = [ [], [], [], [] ];
  private _searchValue: string = '';

  constructor(
    protected _snackBar: MatSnackBar
  ) {
    super(_snackBar);
    this.tabs[1].count = this._userSyncPreview.newUsers.users.length;
    this.tabs[2].count = this._userSyncPreview.updatedUsers.users.length;
    this.tabs[3].count = this._userSyncPreview.blockedUsers.users.length;
    this.tabs[0].count = this.tabs[1].count + this.tabs[2].count + this.tabs[3].count;
  }

  ngOnInit() {
    this._importUsers[1] = this._userSyncPreview.newUsers.users;
    this._importUsers[2] = this._userSyncPreview.updatedUsers.users;
    this._importUsers[3] = this._userSyncPreview.blockedUsers.users;
    this._importUsers[0] = this._importUsers[1].concat(this._importUsers[2]).concat(this._importUsers[3]);
    this._importUsersPaged[1] = this.splitIntoSubArray(this._userSyncPreview.newUsers.users.concat());
    this._importUsersPaged[2] = this.splitIntoSubArray(this._userSyncPreview.updatedUsers.users.concat());
    this._importUsersPaged[3] = this.splitIntoSubArray(this._userSyncPreview.blockedUsers.users.concat());
    this._importUsersPaged[0] = this._importUsersPaged[1].concat(this._importUsersPaged[2]).concat(this._importUsersPaged[3]);
    this._loadSyncedUsers(UserStatusEnum.Active);
  }

  private splitIntoSubArray(arr: Array<ImportUser>): Array<Array<ImportUser>> {
    const newArray: Array<Array<ImportUser>> = [];
    while (arr.length > 0) {
      newArray.push(arr.splice(0, this.pageSize));
    }
    return newArray;
  }

  public goToPage(page: number) {
    if (page !== this._page) {
      this._page = page;
      this.users = this._importUsersPaged[this.selectedTabIndex][this._page];
    }
  }

  public selectTab(index: number) {
    this.selectedTabIndex = index;
    if (this._searchValue === '' || this._searchValue === null) {
      this._loadSyncedUsers(this.tabs[ this.selectedTabIndex ].status);
    } else {
      this.triggerSearch(this._searchValue);
    }
  }

  public triggerSearch(searchValue: string) {
    this._searchValue = searchValue;
    if (searchValue === '' || searchValue === null) {
      this.notSearching = true;
      this._loadSyncedUsers(this.tabs[this.selectedTabIndex].status);
    } else {
      this.notSearching = false;
      this.users = this._importUsers[this.selectedTabIndex].filter(
        x => x.name.toUpperCase().includes(searchValue.toUpperCase()));
    }
  }

  private _loadSyncedUsers(status: UserStatusEnum): void {
    switch (status) {
      case UserStatusEnum.New:
        this.users = this._importUsersPaged[1][this._page];
        break;

      case UserStatusEnum.Updated:
        this.users = this._importUsersPaged[2][this._page];
        break;

      case UserStatusEnum.Blocked:
        this.users = this._importUsersPaged[3][this._page];
        break;

      default:
        this.users = this._importUsersPaged[0][this._page];
    }
  }
}
