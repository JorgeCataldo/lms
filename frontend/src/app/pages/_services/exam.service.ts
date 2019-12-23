import { Injectable } from '@angular/core';
import { BackendService } from '@tg4/http-infrastructure/dist/src';
import { Observable, of } from 'rxjs';
import { ContentTypeEnum } from 'src/app/models/enums/content-type.enum';

@Injectable()
export class ContentExamService {

  constructor(private _httpService: BackendService) { }

  public startExam(moduleId: string, subjectId: string): Observable<any> {
    return this._httpService.post('examstart', {
      'moduleId': moduleId,
      'subjectId': subjectId
    });
  }

  public answerQuestion(
    moduleId: string, subjectId: string, questionId: string, answerId: string,
    moduleName = '', concepts: Array<string> = []
  ): Observable<any> {
    return this._httpService.post('examanswer', {
      'moduleId': moduleId,
      'subjectId': subjectId,
      'questionId': questionId,
      'answerId': answerId,
      'moduleName': moduleName,
      'concepts': concepts
    });
  }


}
