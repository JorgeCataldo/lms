import { Injectable } from '@angular/core';
import { BackendService } from '@tg4/http-infrastructure/dist/src';
import { Observable } from 'rxjs';
import { ProfileTest, ProfileTestAnswer } from 'src/app/models/profile-test.interface';
import { SuggestedProduct } from 'src/app/models/previews/suggested-product.interface';

@Injectable()
export class SettingsProfileTestsService {

  constructor(private _httpService: BackendService) { }

  public getProfileTests(): Observable<any> {
    return this._httpService.get('getProfileTests');
  }

  public getProfileTestById(testId: string): Observable<any> {
    return this._httpService.get('getProfileTestById', [], [
      { 'name': 'testId', 'value': testId }
    ]);
  }

  public manageProfileTest(test: ProfileTest): Observable<any> {
    delete test.questions;
    return this._httpService.post('manageProfileTest', test);
  }

  public getProfileTestResponses(
    page: number = 1, pageSize: number = 10, testId: string = null
  ): Observable<any> {
    return this._httpService.post('getProfileTestResponses', {
      'page': page,
      'pageSize': pageSize,
      'filters': {
        'testId': testId
      }
    });
  }

  public getProfileTestResponseById(responseId: string): Observable<any> {
    return this._httpService.get('getProfileTestResponseById', [], [
      { 'name': 'responseId', 'value': responseId }
    ]);
  }

  public suggestProducts(responseId: string, products: Array<SuggestedProduct>): Observable<any> {
    return this._httpService.post('suggestProducts', {
      'responseId': responseId,
      'products': products
    });
  }

  public getSuggestedProducts(): Observable<any> {
    return this._httpService.get('getSuggestedProducts');
  }

  public suggestProfileTest(usersIds: Array<string>, testId: string): Observable<any> {
    return this._httpService.post('suggestProfileTest', {
      'usersIds': usersIds,
      'testId': testId
    });
  }

  public saveProfileTestResponse(test: ProfileTest): Observable<any> {
    return this._httpService.post('saveProfileTestResponse', {
      'id': test.id,
      'testQuestions': test.testQuestions
    });
  }

  public deleteProfileTest(testId: string): Observable<any> {
    return this._httpService.delete('deleteProfileTest', [
      { 'name': 'id', 'value': testId }
    ]);
  }

  public getAllProfileTestResponses(testId: string): Observable<any> {
    return this._httpService.get('getAllProfileTestResponses', [], [
      { 'name': 'testId', 'value': testId }
    ]);
  }

  public gradeProfileTestAnswers(responseId: string, answers: Array<ProfileTestAnswer>): Observable<any> {
    return this._httpService.put('gradeProfileTestAnswers', {
      'id': responseId,
      'answers': answers
    });
  }
}
