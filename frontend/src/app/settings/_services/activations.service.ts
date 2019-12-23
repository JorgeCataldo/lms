import { Injectable } from '@angular/core';
import { BackendService } from '@tg4/http-infrastructure/dist/src';
import { Observable } from 'rxjs';
import { ValuationTest, ValuationTestAnswer } from 'src/app/models/valuation-test.interface';
import { SuggestedProduct } from 'src/app/models/previews/suggested-product.interface';
import { ActivationTypeEnum } from 'src/app/models/enums/activation-status.enum';
import { Activation } from 'src/app/models/activation.model';

@Injectable()
export class ActivationsService {

  constructor(private _httpService: BackendService) { }

  public getActivations(): Observable<any> {
    return this._httpService.get('getActivations');
  }

  public getValuationTestById(type: ActivationTypeEnum): Observable<any> {
    return this._httpService.get('getActivationByType', [], [
      { 'name': 'type', 'value': type.toString() }
    ]);
  }

  public updateActivationStatus(activation: Activation): Observable<any> {
    return this._httpService.post('updateActivationStatus', {
      'type': activation.type,
      'active': activation.active,
      'title': activation.title,
      'text': activation.text,
      'percentage': activation.percentage
    });
  }

  public createCustomActivation(activation: Activation): Observable<any> {
    return this._httpService.post('createCustomActivation', {
      'active': activation.active,
      'title': activation.title,
      'text': activation.text,
      'percentage': activation.percentage
    });
  }

  public updateCustomActivation(activation: Activation): Observable<any> {
    return this._httpService.post('updateCustomActivation', {
      'ActivationId': activation.id,
      'active': activation.active,
      'title': activation.title,
      'text': activation.text,
      'percentage': activation.percentage
    });
  }

  public deleteActivation(activationId: string): Observable<any> {
    return this._httpService.delete('deleteActivation', [
      { 'name': 'activationId', 'value': activationId }
    ]);
  }
}
