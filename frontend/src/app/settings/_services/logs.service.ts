import { Injectable } from '@angular/core';
import { BackendService } from '@tg4/http-infrastructure/dist/src';
import { Observable } from 'rxjs';

@Injectable()
export class LogsService {

  constructor(private _httpService: BackendService) { }

  public getAllLogs(): Observable<any> {
    return this._httpService.get('getAllLogs');
  }

  public getPagedLogs(page: number = 1, pageSize: number = 10,
     sort: string = '', sortAscending: boolean = false, fromDate: number = null, toDate: number = null): Observable<any> {
    return this._httpService.post('getPagedAuditLogs', {
      'page': page,
      'pageSize': pageSize,
      'filters': {
        'fromDate': fromDate,
        'toDate': toDate,
        'sortBy': sort,
        'isSortAscending': sortAscending
      }
    });
  }

  public getAllUpdatedQuestionsDraft(moduleId: string, fromDate: number = null, toDate: number = null): Observable<any> {
    return this._httpService.post('getAllUpdatedQuestionsDraft', {
      'filters': {
        'moduleId': moduleId,
        'fromDate': fromDate,
        'toDate': toDate
      }
    });
  }
}
