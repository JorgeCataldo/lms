import { Injectable } from '@angular/core';
import { BackendService } from '@tg4/http-infrastructure/dist/src';
import { Observable } from 'rxjs';
import { ForumQuestion, ForumAnswer } from 'src/app/models/forum.model';

@Injectable()
export class ContentForumService {

  constructor(private _httpService: BackendService) { }

  public getForumByModule(
    moduleId: string,
    page: number = 1, pageSize: number = 10,
    searchValue: string = '',
    subjectId: string = null, contentId: string = null
  ): Observable<any> {
    return this._httpService.post('getForumByModule', {
      'moduleId': moduleId,
      'page': page,
      'pageSize': pageSize,
      'filters': {
        'term': searchValue,
        'subjectId': subjectId,
        'contentId': contentId
      }
    });
  }

  public saveForumQuestion(forumQuestion: ForumQuestion): Observable<any> {
    return this._httpService.post('saveForumQuestion', forumQuestion);
  }

  public manageQuestionLike(questionId: string, liked: boolean): Observable<any> {
    return this._httpService.put('manageQuestionLike', {
      'questionId': questionId,
      'liked': liked
    });
  }

  public getForumQuestionById(
    questionId: string, moduleId: string, page: number = 1, pageSize: number = 10
  ): Observable<any> {
    return this._httpService.get('getForumQuestionById', [], [
      { 'name': 'questionId', 'value': questionId },
      { 'name': 'moduleId', 'value': moduleId },
      { 'name': 'page', 'value': page.toString() },
      { 'name': 'pageSize', 'value': pageSize.toString() }
    ]);
  }

  public saveForumQuestionAnswer(answer: ForumAnswer): Observable<any> {
    return this._httpService.post('saveForumQuestionAnswer', answer);
  }

  public manageAnswerLike(answerId: string, liked: boolean): Observable<any> {
    return this._httpService.put('manageAnswerLike', {
      'answerId': answerId,
      'liked': liked
    });
  }

  public removeForumQuestion(questionId: string, moduleId: string): Observable<any> {
    return this._httpService.put('removeForumQuestion', {
      'questionId': questionId,
      'moduleId': moduleId
    });
  }

  public removeForumAnswer(answerId: string, moduleId: string): Observable<any> {
    return this._httpService.put('removeForumAnswer', {
      'answerId': answerId,
      'moduleId': moduleId
    });
  }
}
