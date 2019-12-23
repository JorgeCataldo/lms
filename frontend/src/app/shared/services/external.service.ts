import { Injectable } from '@angular/core';
import { HttpClient, HttpParams, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { BackendService } from '@tg4/http-infrastructure/dist/src';

@Injectable()
export class ExternalService {

  constructor(private _http: HttpClient, private _httpService: BackendService) { }

  public getAddressByCep(cep: string): Observable<any> {
    return this._httpService.get('getCEP', [], [
      { name: 'cep', value: cep }
    ]);
  }

  public getVideoInfoFromVimeo(videoId: string): Observable<any> {
    return this._http.get('//vimeo.com/api/v2/video/' + videoId + '.json');
  }

  public getVideoIdFromUrlIfValid(url: string): string | null {
    let splittedUrl = url.split('/vimeo.com/');

    if (!splittedUrl || splittedUrl.length !== 2) {
      splittedUrl = url.split('player.vimeo.com/video/');

      if (!splittedUrl || splittedUrl.length !== 2)
        return null;
    }

    const idPart = splittedUrl[1];
    if (idPart.indexOf('?') === -1 && idPart.indexOf('/') === -1)
      return idPart;
    else if (idPart.indexOf('?') >= -1)
      return idPart.split('?')[0];
    else if (idPart.indexOf('/') >= -1)
      return idPart.split('/')[0];
  }

  public linkedInLogin(code: string): Observable<any> {
    return this._httpService.post('loginLinkedIn', {
      'code': code
    });
  }

  public bindLinkedIn(code: string, userId: string): Observable<any> {
    return this._httpService.post('bindLinkedIn', {
      'code': code,
      'userId': userId
    });
  }

}
