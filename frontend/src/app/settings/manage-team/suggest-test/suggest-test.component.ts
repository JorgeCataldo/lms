import { Component, OnInit } from '@angular/core';
import { MatSnackBar } from '@angular/material';
import { NotificationClass } from '../../../shared/classes/notification';
import { UserPreview } from '../../../models/previews/user.interface';
import { SettingsUsersService } from '../../_services/users.service';
import { SettingsProfileTestsService } from '../../_services/profile-tests.service';
import { ProfileTest } from 'src/app/models/profile-test.interface';
import { Router } from '@angular/router';

@Component({
  selector: 'app-settings-suggest-test',
  templateUrl: './suggest-test.component.html',
  styleUrls: ['./suggest-test.component.scss']
})
export class SettingsSuggestTestComponent extends NotificationClass implements OnInit {

  public tests: Array<ProfileTest> = [];
  public users: Array<UserPreview> = [];
  public selectedUsers: Array<UserPreview> = [];
  public showAllSelectedUsers: boolean = false;

  private _selectedTest: ProfileTest;

  constructor(
    protected _snackBar: MatSnackBar,
    private _usersService: SettingsUsersService,
    private _testsService: SettingsProfileTestsService,
    private _router: Router
  ) {
    super(_snackBar);
  }

  ngOnInit() {
    this._loadTests();
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

  public selectTest(test: ProfileTest): void {
    if (test && test.id) {
      this._selectedTest = test;
      this.tests.forEach(t => t.checked = false);
      this.tests.find(t => t.id === test.id).checked = true;
    }
  }

  public sendRecommendations(): void {
    this._testsService.suggestProfileTest(
      this.selectedUsers.map(u => u.id),
      this._selectedTest.id
    ).subscribe(() => {
      this.notify('Teste recomendado com sucesso!');
      this._router.navigate([ 'configuracoes/gerenciar-equipe' ]);

    }, (error) => this.notify( this.getErrorNotification(error) ));
  }

  private _loadUsers(searchValue: string = ''): void {
    this._usersService.getPagedFilteredUsersList(
      1, 4, searchValue
    ).subscribe(response => {
      this.users = response.data.userItems;
    });
  }

  private _loadTests(): void {
    this._testsService.getProfileTests().subscribe((response) => {
      this.tests = response.data;

    }, (error) => this.notify( this.getErrorNotification(error) ));
  }

}
