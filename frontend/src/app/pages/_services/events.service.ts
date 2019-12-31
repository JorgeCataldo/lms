import { Injectable } from '@angular/core';
import { BackendService } from '@tg4/http-infrastructure/dist/src';
import { Observable } from 'rxjs';
import { EventApplication } from 'src/app/models/event-application.model';

@Injectable()
export class ContentEventsService {

  constructor(private _httpService: BackendService) { }

  public getPagedFilteredEventsList(page: number = 1, pageSize: number = 10, searchValue: string = ''): Observable<any> {
    return this._httpService.post('getPagedFilteredEventsList', {
      'page': page,
      'pageSize': pageSize,
      'filters': { 'term': searchValue }
    });
  }

  public getEventById(eventId: string): Observable<any>  {
    return this._httpService.get('getEventById', [], [{
      'name': 'id', 'value': eventId
    }]);
  }

  public getHomeEvents(): Observable<any>  {
    return this._httpService.get('getHomeEventsList');
  }

  public getEventApplication(eventId: string, scheduleId: string): Observable<any>  {
    return this._httpService.get('getEventApplicationByUserQuery', [], [
      { 'name': 'eventId', 'value': eventId },
      { 'name': 'scheduleId', 'value': scheduleId }
    ]);
  }

  public applyToEvent(eventApplication: EventApplication): Observable<any> {
    return this._httpService.post('applyToEvent', {
      'eventId': eventApplication.eventId,
      'scheduleId': eventApplication.scheduleId,
      'prepQuizAnswers': eventApplication.answers,
      'prepQuizAnswersList': eventApplication.prepQuizAnswersList
    });
  }

  public postEventUsersGrade(eventId: string, eventScheduleId: string, fileToUpload: any): Observable<any> {
    return this._httpService.post('addEventUsersGradeBaseValues', {
      'eventId': eventId,
      'eventScheduleId': eventScheduleId,
      'fileContent': fileToUpload
    });
  }

  public getEventStudentList(eventId: string, eventScheduleId: string): Observable<any> {
    return this._httpService.get('getEventStudentList', [], [
      { 'name': 'eventId', 'value': eventId },
      { 'name': 'eventScheduleId', 'value': eventScheduleId }
    ]);
  }
}
