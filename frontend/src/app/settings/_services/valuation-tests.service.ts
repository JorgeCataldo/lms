import { Injectable } from '@angular/core';
import { BackendService } from '@tg4/http-infrastructure/dist/src';
import { Observable } from 'rxjs';
import { ValuationTest, ValuationTestAnswer } from 'src/app/models/valuation-test.interface';
import { SuggestedProduct } from 'src/app/models/previews/suggested-product.interface';

@Injectable()
export class SettingsValuationTestsService {

  constructor(private _httpService: BackendService) { }

  public getValuationTests(): Observable<any> {
    return this._httpService.get('getValuationTests');
  }

  public getValuationTestById(testId: string): Observable<any> {
    return this._httpService.get('getValuationTestById', [], [
      { 'name': 'testId', 'value': testId }
    ]);
  }

  public manageValuationTest(test: ValuationTest): Observable<any> {
    delete test.questions;
    return this._httpService.post('manageValuationTest', test);
  }

  public getValuationTestResponses(testId: string,
    page: number = 1, pageSize: number = 10
  ): Observable<any> {
    return this._httpService.post('getValuationTestResponses', {
      'page': page,
      'pageSize': pageSize,
      'testId': testId
    });
  }

  public getValuationTestResponseById(responseId: string): Observable<any> {
    return this._httpService.get('getValuationTestResponseById', [], [
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

  public suggestValuationTest(usersIds: Array<string>, testId: string): Observable<any> {
    return this._httpService.post('suggestValuationTest', {
      'usersIds': usersIds,
      'testId': testId
    });
  }

  public saveValuationTestResponse(test: ValuationTest): Observable<any> {
    return this._httpService.post('saveValuationTestResponse', {
      'id': test.id,
      'testQuestions': test.testQuestions
    });
  }

  public deleteValuationTest(testId: string): Observable<any> {
    return this._httpService.delete('deleteValuationTest', [
      { 'name': 'id', 'value': testId }
    ]);
  }

  public getAllValuationTestResponses(testId: string): Observable<any> {
    return this._httpService.get('getAllValuationTestResponses', [], [
      { 'name': 'testId', 'value': testId }
    ]);
  }

  public gradeValuationTestAnswers(responseId: string, answers: Array<ValuationTestAnswer>): Observable<any> {
    return this._httpService.put('gradeValuationTestAnswers', {
      'id': responseId,
      'answers': answers
    });
  }

  public getModuleValuationTests(moduleId: string): Observable<any> {
    return this._httpService.get('getModuleValuationTests', [], [
      { 'name': 'moduleId', 'value': moduleId }
    ]);
  }

  public getTrackValuationTests(trackId: string): Observable<any> {
    return this._httpService.get('getTrackValuationTests', [], [
      { 'name': 'trackId', 'value': trackId }
    ]);
  }
}
