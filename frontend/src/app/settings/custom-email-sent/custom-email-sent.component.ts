import { Component, OnInit } from '@angular/core';
import { MatSnackBar, Sort, MatDialog } from '@angular/material';
import { SettingsUsersService } from '../_services/users.service';
import { NotificationClass } from '../../shared/classes/notification';
import { CustomEmailSentInfoDialogComponent } from './custom-email-sent-info-dialog/custom-email-sent-info.dialog';
import { CustomEmailPreview } from 'src/app/models/previews/custom-email';
import { Router } from '@angular/router';

@Component({
  selector: 'app-settings-custom-email-sent',
  templateUrl: './custom-email-sent.component.html',
  styleUrls: ['./custom-email-sent.component.scss']
})
export class SettingsCustomEmailSentComponent extends NotificationClass implements OnInit {

  public readonly pageSize: number = 10;
  public readonly displayedColumns: string[] = [
    'title', 'date', 'usersCount', 'action'
  ];
  public customEmails: Array<CustomEmailPreview> = [];
  public customEmailsCount: number = 0;
  private _customEmailsPage: number = 1;
  private _sortDirection: boolean = false;
  private _sortActive: string = '';

  constructor(
    protected _snackBar: MatSnackBar,
    private _router: Router,
    private _usersService: SettingsUsersService,
    private _dialog: MatDialog
  ) {
    super(_snackBar);
  }

  ngOnInit() {
    this._loadCustomEmails(this._customEmailsPage, this._sortActive, this._sortDirection);
  }

  public sortData(sort: Sort) {
    if (sort.direction === 'asc') {
      this._sortActive = sort.active;
      this._sortDirection = true;
      this._loadCustomEmails(this._customEmailsPage, this._sortActive, this._sortDirection);
    } else if (sort.direction === 'desc') {
      this._sortActive = sort.active;
      this._sortDirection = false;
      this._loadCustomEmails(this._customEmailsPage, this._sortActive, this._sortDirection);
    } else {
      this._sortActive = '';
      this._sortDirection = false;
      this._loadCustomEmails(this._customEmailsPage, this._sortActive, this._sortDirection);
    }
  }

  public goToPage(page: number) {
    if (page !== this._customEmailsPage) {
      this._customEmailsPage = page;
      this._loadCustomEmails(this._customEmailsPage, this._sortActive, this._sortDirection);
    }
  }

  public openEmailInfoDialog(process: CustomEmailPreview) {
    this._dialog.open(CustomEmailSentInfoDialogComponent, {
      width: '1000px',
      data: process
    });
  }

  public sendEmail() {
    this._router.navigate([ 'configuracoes/enviar-email' ]);
  }

  private _loadCustomEmails(page: number, sortValue: string = '', sortAscending: boolean = false): void {
    this._usersService.getPagedCustomEmails(
      page, this.pageSize, sortValue, sortAscending
    ).subscribe((response) => {
      this.customEmails = response.data.customEmailItems;
      this.customEmailsCount = response.data.itemsCount;

    }, () => this.notify('Ocorreu um erro, por favor tente novamente mais tarde'));
  }

}
