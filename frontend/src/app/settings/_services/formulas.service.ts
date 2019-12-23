import { Injectable } from '@angular/core';
import { BackendService } from '@tg4/http-infrastructure/dist/src';
import { Observable } from 'rxjs';
import { Formula, FormulaType } from 'src/app/models/formula.model';

@Injectable()
export class SettingsFormulasService {

  constructor(private _httpService: BackendService) { }

  public getFormulas(page: number = 1, pageSize: number = 10): Observable<any> {
    return this._httpService.get('getFormulas', [], [
      { 'name': 'page', 'value': page.toString() },
      { 'name': 'pageSize', 'value': pageSize.toString() }
    ]);
  }

  public getFormulaById(formulaId: string): Observable<any> {
    return this._httpService.get('getFormulaById', [], [
      { 'name': 'formulaId', 'value': formulaId }
    ]);
  }

  public addFormula(formula: Formula): Observable<any> {
    return this._httpService.post('addFormula', formula);
  }

  public manageFormula(formula: Formula): Observable<any> {
    return this._httpService.post('manageFormula', formula);
  }

  public deleteFormula(formulaId: string): Observable<any> {
    return this._httpService.delete('deleteFormula', [
      { 'name': 'formulaId', 'value': formulaId }
    ]);
  }

  public getFormulaVariables(): Observable<any> {
    return this._httpService.get('getFormulaVariables');
  }

}
