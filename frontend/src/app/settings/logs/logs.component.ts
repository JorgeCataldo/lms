import { Component, OnInit } from '@angular/core';
import { NotificationClass } from 'src/app/shared/classes/notification';
import { MatSnackBar, Sort } from '@angular/material';
import { LogsService } from '../_services/logs.service';
import { AuditLog } from 'src/app/models/audit-log.interface';
import { ExcelService } from 'src/app/shared/services/excel.service';
import { UpdatedQuestionExcel } from 'src/app/models/question.model';
import { EntityActionEnum } from '../../models/enums/audit-log-entity-action.enum';

@Component({
  selector: 'app-logs',
  templateUrl: './logs.component.html',
  styleUrls: ['./logs.component.scss']
})

export class LogsComponent  extends NotificationClass implements OnInit {

  public readonly pageSize: number = 10;
  public readonly displayedColumns: string[] = [
    'name', 'date', 'logAction', 'action'
  ];
  public logs: AuditLog[] = [];
  public logsCount: number = 0;
  private _logPage: number = 1;
  private _sortDirection: boolean = false;
  private _sortActive: string = '';
  private _fromDate: number = null;
  private _toDate: number = null;
  private _exportIgnoreList: string[] = [
    'QuestionId',
    'DraftId',
    'Id',
    'SubjectId',
    'ModuleId',
    'CreatedAt',
    'UpdatedAt',
    'DeletedAt',
    'CreatedBy',
    'UpdatedBy',
    'DeletedBy',
    'InstructorId',
    'TutorsIds',
    'EcommerceId',
    'newValuePlain',
    'oldValuePlain'
  ];

  constructor(
    protected _snackBar: MatSnackBar,
    private _excelService: ExcelService,
    private _logsService: LogsService
  ) {
    super(_snackBar);
  }

  ngOnInit() {
    this._loadProcesses(this._logPage, this._sortActive, this._sortDirection,
      this._fromDate, this._toDate);
  }

  private _loadProcesses(page: number, sortValue: string = '', sortAscending: boolean = false,
    fromDate: number = null, toDate: number = null): void {
    this._logsService.getPagedLogs(
      page, this.pageSize, sortValue, sortAscending, fromDate, toDate
    ).subscribe((response) => {
      this.logs = response.data.auditLogItems;
      this.logsCount = response.data.itemsCount;

    }, () => this.notify('Ocorreu um erro, por favor tente novamente mais tarde'));
  }

  public filterByDateRange(dates: Array<Date>) {
    this._fromDate = dates[0].getTime();
    this._toDate = dates[1].getTime();
    this._loadProcesses(this._logPage, this._sortActive, this._sortDirection,
      this._fromDate, this._toDate);
  }

  public sortData(sort: Sort) {
    if (sort.direction === 'asc') {
      this._sortActive = sort.active;
      this._sortDirection = true;
      this._loadProcesses(this._logPage, this._sortActive, this._sortDirection,
        this._fromDate, this._toDate);
    } else if (sort.direction === 'desc') {
      this._sortActive = sort.active;
      this._sortDirection = false;
      this._loadProcesses(this._logPage, this._sortActive, this._sortDirection,
        this._fromDate, this._toDate);
    } else {
      this._sortActive = '';
      this._sortDirection = false;
      this._loadProcesses(this._logPage, this._sortActive, this._sortDirection,
        this._fromDate, this._toDate);
    }
  }

  public goToPage(page: number) {
    if (page !== this._logPage) {
      this._logPage = page;
      this._loadProcesses(this._logPage, this._sortActive, this._sortDirection,
        this._fromDate, this._toDate);
    }
  }

  public exportUpdatedQuestions(entityId: string) {
    this._logsService.getAllUpdatedQuestionsDraft(entityId, this._fromDate, this._toDate).subscribe(res => {
      const updatedExcelQuestions: UpdatedQuestionExcel[] = [];

      for (let questionIndex = 0; questionIndex < res.data.length; questionIndex++) {

        const item = res.data[questionIndex];
        const newExcelLine = new UpdatedQuestionExcel();
        const flatData = this.flattenObject(item);

        // tslint:disable-next-line: forin
        for (const i in flatData) {
          newExcelLine[i] = flatData[i];
        }

        updatedExcelQuestions.push(newExcelLine);
      }
      this._excelService.exportAsExcelFile(updatedExcelQuestions, 'LOG - ' + entityId);
    });
  }

  public exportAllQuestions() {
    this._logsService.getAllLogs().subscribe(res => {
    });
  }

  private flattenObject(obj): any {
    const toReturn = {};

    for (const i in obj) {
      if (this._exportIgnoreList.includes(i)) continue;
      if (!obj.hasOwnProperty(i)) continue;

      if ((typeof obj[i]) === 'object' && obj[i] !== null) {
          const flatObject = this.flattenObject(obj[i]);
          for (const x in flatObject) {
              if (!flatObject.hasOwnProperty(x)) continue;

              toReturn[i + '.' + x] = flatObject[x];
          }
      } else {
          toReturn[i] = obj[i];
      }
    }
    return toReturn;
  }


}
