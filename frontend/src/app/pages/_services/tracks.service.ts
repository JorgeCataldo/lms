import { Injectable } from '@angular/core';
import { BackendService } from '@tg4/http-infrastructure/dist/src';
import { Observable } from 'rxjs';

@Injectable()
export class ContentTracksService {

  constructor(private _httpService: BackendService) { }

  public getTrackById(trackId: string, getApplications: boolean = false): Observable<any>  {
    return this._httpService.get('getTrackById', [], [
      { 'name': 'id', 'value': trackId },
      { 'name': 'getApplications', 'value': getApplications.toString() }
    ]);
  }

  public getPagedFilteredTracksList(
    page: number = 1, pageSize: number = 10, searchValue: string = '',
    tags: Array<string> = [], published: boolean = true, attending: boolean = null
  ): Observable<any> {
    return this._httpService.post('getPagedFilteredTracksList', {
      'page': page,
      'pageSize': pageSize,
      'filters': {
        'term': searchValue,
        'tags': tags,
        'published': published,
        'attending': attending
      }
    });
  }

  public getAllFilteredTracksList(
    searchValue: string = '',
    tags: Array<string> = [], published: boolean = true
  ): Observable<any> {
    return this._httpService.post('getAllFilteredTracksList', {
      'filters': {
        'term': searchValue,
        'tags': tags,
        'published': published
      }
    });
  }

  public getPagedFilteredMycoursesTracksList(
    page: number = 1, pageSize: number = 10, searchValue: string = '',
    tags: Array<string> = [], published: boolean = true
  ): Observable<any> {
    return this._httpService.post('getPagedFilteredMyCoursesTracksList', {
      'page': page,
      'pageSize': pageSize,
      'filters': {
        'term': searchValue,
        'tags': tags,
        'published': published
      }
    });
  }

  public getPagedFilteredEffortPerformancesTracksList(
    page: number = 1, searchValue: string = '', pageSize: number = 10
  ): Observable<any> {
    return this._httpService.post('getPagedFilteredEffortPerformancesTracksList', {
      'page': page,
      'pageSize': pageSize,
      'filters': {
        'term': searchValue
      }
    });
  }

  public getTrackCurrentStudentOverview(trackId: string): Observable<any> {
    return this._httpService.get('getTrackCurrentStudentOverview', [], [{
      'name': 'trackId', 'value': trackId
    }]);
  }

  public markMandatoryVideoViewed(trackId: string): Observable<any> {
    return this._httpService.put('markMandatoryVideoViewed', {
      'trackId': trackId
    });
  }
}
