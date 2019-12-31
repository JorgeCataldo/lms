import { Injectable } from '@angular/core';
import { BackendService } from '@tg4/http-infrastructure/dist/src';
import { Observable } from 'rxjs';

@Injectable()
export class ContentModulesService {

  constructor(private _httpService: BackendService) { }

  public getPagedModulesList(page: number = 1, pageSize: number = 10): Observable<any> {
    return this._httpService.get('getPagedModulesList', [], [
      { 'name': 'page', 'value': page.toString() },
      { 'name': 'pageSize', 'value': pageSize.toString() }
    ]);
  }

  public getAllContent(moduleId: string): Observable<any> {
    return this._httpService.get('getAllContent', [], [
      { 'name': 'moduleId', 'value': moduleId }
    ]);
  }

  public getPagedFilteredModulesList(
    page: number = 1, pageSize: number = 10,
    searchValue: string = '', tags: Array<string> = [], onlyPublished: boolean = false
  ): Observable<any> {
    return this._httpService.post('getPagedFilteredModulesList', {
      'page': page,
      'pageSize': pageSize,
      'filters': {
        'term': searchValue,
        'tags': tags,
        'onlyPublished': onlyPublished
      }
    });
  }

  public getPagedFilteredMyCoursesModulesList(
    page: number = 1, pageSize: number = 10,
    searchValue: string = '', tags: Array<string> = [], onlyPublished: boolean = false
  ): Observable<any> {
    return this._httpService.post('getPagedMyCoursesFilteredModulesList', {
      'page': page,
      'pageSize': pageSize,
      'filters': {
        'term': searchValue,
        'tags': tags,
        'onlyPublished': onlyPublished
      }
    });
  }

  public getPagedHomeModulesList(
    page: number = 1, pageSize: number = 10,
    searchValue: string = '', tags: Array<string> = []
    ): Observable<any> {
    return this._httpService.post('getPagedHomeModulesList', {
      'page': page,
      'pageSize': pageSize,
      'filters': {
        'term': searchValue,
        'tags': tags
      }
    });
  }

  public getModuleById(moduleId: string): Observable<any> {
    return this._httpService.get('getModuleById', [], [
      { 'name': 'id', 'value': moduleId }
    ]);
  }

  public deleteModuleById(id: string): Observable<any> {
    return this._httpService.post('deleteModuleById', {
      'moduleId': id
    });
  }
}
