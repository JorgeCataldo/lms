import { Component, OnInit } from '@angular/core';
import { NotificationClass } from '../../shared/classes/notification';
import { MatSnackBar } from '@angular/material';
import { FormGroup, FormControl, Validators } from '@angular/forms';
import { UserPreview } from 'src/app/models/previews/user.interface';
import { SettingsUsersService } from '../_services/users.service';
import * as Editor from 'tui-editor';
import { MarkdownToHtmlPipe } from 'markdown-to-html-pipe';

@Component({
  selector: 'app-settings-custom-email',
  templateUrl: './custom-email.component.html',
  styleUrls: ['./custom-email.component.scss']
})
export class SettingsCustomEmailComponent extends NotificationClass implements OnInit {

  public formGroup: FormGroup;
  public editor: Editor;
  public users: Array<UserPreview> = [];
  public selectedUsers: Array<UserPreview> = [];
  public showAllSelectedUsers: boolean = false;

  constructor(
    protected _snackBar: MatSnackBar,
    private _usersService: SettingsUsersService,
    private markdownPipe: MarkdownToHtmlPipe
  ) {
    super(_snackBar);
    this.formGroup = this._createForm();
  }

  ngOnInit() {
    const usersStr = localStorage.getItem('SelectedUsers');
    if (usersStr && usersStr.trim() !== '')
      this.selectedUsers = JSON.parse( usersStr );

    this.editor = new Editor({
      el: document.querySelector('#htmlEditor'),
      initialEditType: 'markdown',
      hideModeSwitch: true,
      previewStyle: 'vertical',
      height: '400px'
    });
  }

  public triggerUserSearch(searchValue: string) {
    if (searchValue && searchValue.trim() !== '')
      this._loadUsers( searchValue );
  }

  public sendMail(): void {
    if (!this.selectedUsers || this.selectedUsers.length === 0) {
      this.notify('É necessario selecionar pelo menos um usuário');
      return;
    }
    const formInfo = this.formGroup.getRawValue();
    const markdown = this.markdownPipe.transform(this.editor.getMarkdown());
    this._usersService.sendCustomEmail(
      this.selectedUsers.map(x => x.id), formInfo.title, markdown
    ).subscribe(() => {
      this.notify('Mensagem enviada com sucesso!');
    }, () => {
      this.notify('Ocorreu um erro, por favor tente novamente maist arde');
    });
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
    (<HTMLInputElement>document.getElementById('list-search-input')).value = '';
    this.users = [];
  }

  private _createForm(): FormGroup {
    return new FormGroup({
      'title': new FormControl('', [ Validators.required ])
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
