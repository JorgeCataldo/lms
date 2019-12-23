import { Injectable, EventEmitter } from '@angular/core';
import { BackendService } from '@tg4/http-infrastructure/dist/src';
import { UploadResource } from '../../models/shared/upload-resource.interface';
import { Observable, of, Subject } from 'rxjs';
import { tap } from 'rxjs/operators';
import { ActionInfo } from '../directives/save-action/action-info.interface';

@Injectable()
export class SharedService {

  public showLoader$ = new EventEmitter();

  public forumQuestion = new Subject();
  public forumQuestionResponse = new Subject<string>();


  constructor(private _httpService: BackendService) { }

  public getLoaderSubject() {
    return this.showLoader$;
  }

  public setLoaderValue(value: boolean) {
    this.showLoader$.next(value);
  }

  public uploadImage(resource: UploadResource): Observable<any> {
    resource.data = resource.data.split('base64,')[1];
    return this._httpService.post('uploadImage', resource);
  }

  public uploadFile(resource: UploadResource): Observable<any> {
    resource.data = resource.data.split('base64,')[1];
    return this._httpService.post('uploadFile', resource);
  }

  public getLevels(getAll: boolean = false): Observable<any> {
    return this._httpService.get('getLevels', [], [
      { 'name': 'getAll', 'value': getAll.toString() }
    ]);
  }

  public saveAction(actionInfo: ActionInfo): Observable<any> {
    return of({
      'data': { }
    });
    // return this._httpService.post('saveAction', actionInfo);
  }

  public getNotifications(): Observable<any> {
    return this._httpService.get('getNotifications');
  }

  public manageNotification(notificationId: string, read: boolean = true): Observable<any> {
    return this._httpService.put('manageNotification', {
      'notificationId': notificationId,
      'read': read
    });
  }
}
