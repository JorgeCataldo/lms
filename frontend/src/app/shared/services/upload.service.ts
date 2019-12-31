import { Injectable, EventEmitter } from '@angular/core';
import { BackendService } from '@tg4/http-infrastructure/dist/src';
import { Observable, of, Subject } from 'rxjs';
import { HttpClient, HttpRequest, HttpEventType, HttpResponse } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { tap } from 'rxjs/operators';
import { ActionInfo } from '../directives/save-action/action-info.interface';
import JSZip from 'jszip';
// import * as JSZipUtils from 'jszip-utils';

@Injectable()
export class UploadService {

  public showLoader$ = new EventEmitter();
  public progress: number;
  public message: string;

  constructor(private _httpService: BackendService, private http: HttpClient) { }

  public getLoaderSubject() {
    return this.showLoader$;
  }

  public setLoaderValue(value: boolean) {
    this.showLoader$.next(value);
  }

  public uploadFile(file): Observable<string> {
    const formData = new FormData();
    formData.append(file.name, file);

    const uploadReq = new HttpRequest('POST', environment.apiUrl + '/api/moduleDraft/sendFileToS3', formData, {
      reportProgress: true,
    });

    this.http.request(uploadReq).subscribe(event => {

      if (event.type === HttpEventType.UploadProgress) {
        this.progress = Math.round(100 * event.loaded / event.total);
      } else if (event.type === HttpEventType.Response) {
        this.message = event.body['data'].toString();
        return of(this.message);
      }
    });

    return of(this.message);
  }

  private  hex (value) {
    return Math.floor(value).toString(16);
  }
}
