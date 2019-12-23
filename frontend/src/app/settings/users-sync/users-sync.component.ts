import { Component, OnInit } from '@angular/core';
import { MatSnackBar, Sort, MatDialog } from '@angular/material';
import { SettingsUsersService } from '../_services/users.service';
import { NotificationClass } from '../../shared/classes/notification';
import { UserSyncPreview } from '../../models/previews/user-sync.interface';
import { Router } from '@angular/router';
import { SharedService } from 'src/app/shared/services/shared.service';
import { AbstractControl } from '@angular/forms';
import { UploadResource } from 'src/app/models/shared/upload-resource.interface';
import { UsersSyncErrorDialogComponent } from './users-sync-error-dialog/users-sync-error.dialog';

@Component({
  selector: 'app-settings-users-sync',
  templateUrl: './users-sync.component.html',
  styleUrls: ['./users-sync.component.scss']
})
export class SettingsUsersSyncComponent extends NotificationClass implements OnInit {

  public readonly pageSize: number = 10;
  public readonly displayedColumns: string[] = [
    'status', 'date', 'usersCount', 'newUsersCount', 'updatedUsers', 'blockedUsers', 'actions'
  ];
  public processes: Array<UserSyncPreview> = [];
  public processesCount: number = 0;
  private _processesPage: number = 1;
  private _sortDirection: boolean = false;
  private _sortActive: string = '';
  private _fromDate: number = null;
  private _toDate: number = null;

  constructor(
    protected _snackBar: MatSnackBar,
    private _usersService: SettingsUsersService,
    private _router: Router,
    private _sharedService: SharedService,
    private _dialog: MatDialog
  ) {
    super(_snackBar);
  }

  ngOnInit() {
    this._loadProcesses(this._processesPage, this._sortActive, this._sortDirection,
      this._fromDate, this._toDate);
  }

  public filterByDateRange(dates: Array<Date>) {
    this._fromDate = dates[0].getTime();
    this._toDate = dates[1].getTime();
    this._loadProcesses(this._processesPage, this._sortActive, this._sortDirection,
      this._fromDate, this._toDate);
  }

  public sortData(sort: Sort) {
    if (sort.direction === 'asc') {
      this._sortActive = sort.active;
      this._sortDirection = true;
      this._loadProcesses(this._processesPage, this._sortActive, this._sortDirection,
        this._fromDate, this._toDate);
    } else if (sort.direction === 'desc') {
      this._sortActive = sort.active;
      this._sortDirection = false;
      this._loadProcesses(this._processesPage, this._sortActive, this._sortDirection,
        this._fromDate, this._toDate);
    } else {
      this._sortActive = '';
      this._sortDirection = false;
      this._loadProcesses(this._processesPage, this._sortActive, this._sortDirection,
        this._fromDate, this._toDate);
    }
  }

  public goToPage(page: number) {
    if (page !== this._processesPage) {
      this._processesPage = page;
      this._loadProcesses(this._processesPage, this._sortActive, this._sortDirection,
        this._fromDate, this._toDate);
    }
  }

  public viewDetails(process: UserSyncPreview) {
    if (process) {
      localStorage.setItem('SyncProcess', JSON.stringify(process));
      this._router.navigate(['configuracoes/processos-de-sincronia/' + process.id]);
    }
  }

  public openErrorsDialog(process: UserSyncPreview) {
    this._dialog.open(UsersSyncErrorDialogComponent, {
      width: '1000px',
      data: process.importErrors
    });
  }

  private _loadProcesses(page: number, sortValue: string = '', sortAscending: boolean = false,
    fromDate: number = null, toDate: number = null): void {
    this._usersService.getPagedFilteredUsersSyncProcesses(
      page, this.pageSize, sortValue, sortAscending, fromDate, toDate
    ).subscribe((response) => {
      this.processes = response.data.userSyncProcesseItems;
      this.processesCount = response.data.itesmCount;

    }, () => this.notify('Ocorreu um erro, por favor tente novamente mais tarde'));
  }

  public setDocumentFile(files: FileList) {
    const file = files.item(0);
    const callback = this._sendToServer.bind(this);
    const reader = new FileReader();
    reader.onloadend = function (e) {
      callback(
        this.result as string,
        file.name,
        file.name,
        file.name
      );
    };
    reader.readAsDataURL(file);
  }

  private _sendToServer(result: string, fileName: string, valueControl: AbstractControl, nameControl: AbstractControl) {
    const resource = {
      fileContent: result
    };
    this.notify('Dependendo do tamanho da planilha o processo de importação pode demorar\
     um pouco. Assim que completar ele aparecerá na lista abaixo');

    this._usersService.postUsersSyncProcess(resource).subscribe((response) => {
      this.notify('Arquivo enviado com sucesso!');

      this._loadProcesses(this._processesPage, this._sortActive, this._sortDirection,
        this._fromDate, this._toDate);
    }, (err) => {
      this.notify('Ocorreu um erro ao enviar o arquivo, por favor tente novamente mais tarde');
    });
  }

}
