import { Component, OnInit } from '@angular/core';
import { MatSnackBar, Sort, MatDialog } from '@angular/material';
import { SettingsUsersService } from '../_services/users.service';
import { NotificationClass } from '../../shared/classes/notification';
import { ExcelService } from 'src/app/shared/services/excel.service';
import { NpsAssociatedObjectsDialogComponent } from './nps-associated-objects-dialog/nps-associated-objects.dialog';
import { Activation } from 'src/app/models/activation.model';

@Component({
  selector: 'app-settings-nps',
  templateUrl: './nps.component.html',
  styleUrls: ['./nps.component.scss']
})
export class NpsComponent extends NotificationClass implements OnInit {

  public readonly pageSize: number = 10;
  public readonly displayedColumns: string[] = [
    'image', 'name', 'email', 'grade', 'createdAt', 'objects'
  ];
  public processes: any[] = [];
  public processesCount: number = 0;
  private _processesPage: number = 1;
  private _name: string = '';
  private _sortDirection: boolean = false;
  private _sortActive: string = '';

  public nps: Activation;
  public npsEnable: boolean = false;

  constructor(
    protected _snackBar: MatSnackBar,
    private _usersService: SettingsUsersService,
    private _excelService: ExcelService,
    private _dialog: MatDialog
  ) {
    super(_snackBar);
  }

  ngOnInit() {
    this._loadProcesses(this._processesPage, this._name, this._sortActive, this._sortDirection);
  }

  public sortData(sort: Sort) {
    if (sort.direction === 'asc') {
      this._sortActive = sort.active;
      this._sortDirection = true;
      this._loadProcesses(this._processesPage, this._name, this._sortActive, this._sortDirection);
    } else if (sort.direction === 'desc') {
      this._sortActive = sort.active;
      this._sortDirection = false;
      this._loadProcesses(this._processesPage, this._name, this._sortActive, this._sortDirection);
    } else {
      this._sortActive = '';
      this._sortDirection = false;
      this._loadProcesses(this._processesPage, this._name, this._sortActive, this._sortDirection);
    }
  }

  public goToPage(page: number) {
    if (page !== this._processesPage) {
      this._processesPage = page;
      this._loadProcesses(this._processesPage, this._name, this._sortActive, this._sortDirection);
    }
  }

  public viewDetails(process: any) {
    this._dialog.open(NpsAssociatedObjectsDialogComponent, {
      width: '400px',
      data: {
        tracks: process.tracksInfo,
        modules: process.modulesInfo,
        events: process.eventsInfo
      }
    });
  }

  private _loadProcesses(page: number, name: string = '', sortValue: string = '', sortAscending: boolean = false): void {
    this._usersService.getUserNpsInfos(
      page, this.pageSize, name, sortValue, sortAscending
    ).subscribe((response) => {
      this.processes = response.data.npsItems;
      this.processesCount = response.data.itemsCount;
    }, () => this.notify('Ocorreu um erro, por favor tente novamente mais tarde'));
  }

  public getAnswersExcel(): void {
    this._usersService.getAllUserNpsInfos().subscribe((response) => {
      for (let index = 0; index < response.data.length; index++) {
        const item = response.data[index];

        item.eventsInfo = item.eventsInfo !== null ? item.eventsInfo.filter(x => x.id).map(x => x.name).join(',') : '';
        item.modulesInfo = item.modulesInfo !== null ? item.modulesInfo.filter(x => x.id).map(x => x.name).join(',') : '';
        item.tracksInfo = item.tracksInfo !== null ? item.tracksInfo.filter(x => x.id).map(x => x.name).join(',') : '';

      }

      this._excelService.exportAsExcelFile(response.data, 'Nps');
    }, (error) => this.notify( this.getErrorNotification(error) ));
  }
}
