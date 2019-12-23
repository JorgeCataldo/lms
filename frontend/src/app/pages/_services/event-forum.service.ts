import { Injectable } from '@angular/core';
import { BackendService } from '@tg4/http-infrastructure/dist/src';
import { Observable } from 'rxjs';
import { EventForumQuestion, EventForumAnswer } from 'src/app/models/event-forum.model';

@Injectable()
export class ContentEventForumService {

  constructor(private _httpService: BackendService) { }

  public getEventForumByEventSchedule(
    eventScheduleId: string,
    page: number = 1, pageSize: number = 10,
    searchValue: string = ''
  ): Observable<any> {
    return this._httpService.post('getEventForumByEventSchedule', {
      'eventScheduleId': eventScheduleId,
      'page': page,
      'pageSize': pageSize,
      'filters': {
        'term': searchValue
      }
    });
  }

  public saveEventForumQuestion(forumQuestion: EventForumQuestion): Observable<any> {
    return this._httpService.post('saveEventForumQuestion', forumQuestion);
  }

  public manageEventForumQuestionLike(questionId: string, liked: boolean): Observable<any> {
    return this._httpService.put('manageEventForumQuestionLike', {
      'questionId': questionId,
      'liked': liked
    });
  }

  public getEventForumQuestionById(
    questionId: string, eventScheduleId: string, page: number = 1, pageSize: number = 10
  ): Observable<any> {
    return this._httpService.get('getEventForumQuestionById', [], [
      { 'name': 'questionId', 'value': questionId },
      { 'name': 'eventScheduleId', 'value': eventScheduleId },
      { 'name': 'page', 'value': page.toString() },
      { 'name': 'pageSize', 'value': pageSize.toString() }
    ]);
  }

  public saveEventForumQuestionAnswer(answer: EventForumAnswer): Observable<any> {
    return this._httpService.post('saveEventForumQuestionAnswer', answer);
  }

  public manageEventForumAnswerLike(answerId: string, liked: boolean): Observable<any> {
    return this._httpService.put('manageEventForumAnswerLike', {
      'answerId': answerId,
      'liked': liked
    });
  }

  public removeEventForumQuestion(questionId: string, eventScheduleId: string): Observable<any> {
    return this._httpService.put('removeEventForumQuestion', {
      'questionId': questionId,
      'eventScheduleId': eventScheduleId
    });
  }

  public removeForumAnswer(answerId: string, eventScheduleId: string): Observable<any> {
    return this._httpService.put('removeEventForumAnswer', {
      'answerId': answerId,
      'eventScheduleId': eventScheduleId
    });
  }

  public getUserEventForumByEventSchedule(eventScheduleId: string, userId: string): Observable<any> {
    return this._httpService.post('getUserEventForumPreviewByEventSchedule', {
      'eventScheduleId': eventScheduleId,
      'userId': userId
    });
  }
}
