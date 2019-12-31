import { Injectable } from '@angular/core';
import { BackendService } from '@tg4/http-infrastructure/dist/src';
import { Observable, of } from 'rxjs';
import { Track } from '../../models/track.model';
import { TrackEvent } from 'src/app/models/track-event.model';
import { EcommerceProduct } from 'src/app/models/ecommerce-product.model';

@Injectable()
export class SettingsTracksService {

  constructor(private _httpService: BackendService) { }

  public getTrackById(trackId: string): Observable<any>  {
    return this._httpService.get('getTrackById', [], [{
      'name': 'id', 'value': trackId
    }]);
  }

  public getTracksGrades(trackIds: string): Observable<any> {
    return this._httpService.get('getTrackCurrentStudentOverview', [], [{
      'name': 'trackId', 'value': trackIds
    }]);
  }

  public getPagedFilteredTracksList(
    page: number = 1, pageSize: number = 10,
    searchValue: string = '', published: boolean = null
  ): Observable<any> {
    return this._httpService.post('getPagedFilteredTracksList', {
      'page': page,
      'pageSize': pageSize,
      'filters': {
        'term': searchValue,
        'published': published
      }
    });
  }

  public manageTrackInfo(track: Track): Observable<any> {
    return this._httpService.post('manageTrackInfo', track);
  }

  public deleteTrackById(id: string): Observable<any> {
    return this._httpService.post('deleteTrackById', {
      'trackId': id
    });
  }

  public getTrackOverview(
    trackId: string, getManageInfo: boolean = false, page: number = 1, searchTerm: string = '', pageSize: number = 10
  ): Observable<any>  {
    return this._httpService.get('getTrackOverview', [], [
      { 'name': 'trackId', 'value': trackId },
      { 'name': 'page', 'value': page.toString() },
      { 'name': 'pageSize', 'value': pageSize.toString() },
      { 'name': 'searchTerm', 'value': searchTerm },
      { 'name': 'getManageInfo', 'value': getManageInfo.toString() }
    ]);
  }

  public getTrackOverviewEventInfo(trackId: string): Observable<any> {
    return this._httpService.get('getTrackOverviewEventInfo', [], [
      { 'name': 'trackId', 'value': trackId }
    ]);
  }

  public getTrackStudentOverview(trackId: string, studentId: string): Observable<any> {
    return this._httpService.get('getTrackStudentOverview', [], [
      { 'name': 'trackId', 'value': trackId },
      { 'name': 'studentId', 'value': studentId }
    ]);
  }

  public getTrackModuleOverview(trackId: string, moduleId: string): Observable<any> {
    return this._httpService.get('getTrackModuleOverview', [], [
      { 'name': 'trackId', 'value': trackId },
      { 'name': 'moduleId', 'value': moduleId }
    ]);
  }

  public manageCalendarEvents(trackId: string, calendarEvents: Array<TrackEvent>): Observable<any> {
    return this._httpService.put('manageCalendarEvents', {
      'trackId': trackId,
      'calendarEvents': calendarEvents
    });
  }

  public manageEcommerceProducts(trackId: string, products: Array<EcommerceProduct>): Observable<any> {
    return this._httpService.put('manageEcommerceProducts', {
      'trackId': trackId,
      'products': products
    });
  }

  public getTrackOverviewGrades(trackId: string): Observable<any> {
    return this._httpService.get('getTrackOverviewGrades', [], [
      { 'name': 'trackId', 'value': trackId }
    ]);
  }

  public getTrackOverviewStudents(trackId: string): Observable<any> {
    return this._httpService.get('getTrackOverviewStudents', [], [
      { 'name': 'trackId', 'value': trackId }
    ]);
  }

  public getPagedFilteredManagerTracksList(page: number = 1, pageSize: number = 10, searchValue: string = ''): Observable<any> {
    return this._httpService.post('getPagedFilteredManagerTracksList', {
      'page': page,
      'pageSize': pageSize,
      'filters': { 'term': searchValue }
    });
  }

  public addCalendarEventsFromFile(id: string, fileContent: string): Observable<any> {
    return this._httpService.post('addCalendarEventsFromFile', {
      'trackId': id,
      'fileContent': fileContent
    });
  }

  public getAllContent(id: string): Observable<any> {
    return this._httpService.get('getAllTrackContent', [], [ {'name': 'trackId', 'value': id } ] );
  }

  public getAlltracks(): Observable<any> {
    return this._httpService.get('getAllTracks', [], [] );
  }

  public getTrackReportStudents(): Observable<any> {
    return this._httpService.get('getTrackReportStudents', [], []);
  }

  public getTrackAnswers(trackIds: string, moduleIds: string): Observable<any> {
    return this._httpService.get('getTrackAnswers', [], [
      {'name': 'trackIds', 'value': trackIds },
      {'name': 'moduleIds', 'value': moduleIds }
    ]);
  }

  public getAtypicalMovements(trackIds: string): Observable<any> {
    return this._httpService.get('getAtypicalMovements', [], [
      {'name': 'trackIds', 'value': trackIds }
    ]);
  }

  public getTrackNps(): Observable<any> {
    return this._httpService.get('getTrackNps', [], []);
  }
}
