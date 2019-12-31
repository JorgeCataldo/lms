import { Injectable } from '@angular/core';
import { BackendService } from '@tg4/http-infrastructure/dist/src';
import { Observable } from 'rxjs';
import { Module } from '../../models/module.model';
import { Subject } from '../../models/subject.model';
import { Content } from '../../models/content.model';
import { SupportMaterial } from '../../models/support-material.interface';
import { Requirement } from '../modules/new-module/models/new-requirement.model';
import { Question } from '../../models/question.model';

@Injectable()
export class ReportsService {

  constructor(private _httpService: BackendService) { }

  public getUserProgresses(trackIds: string): Observable<any> {
    return this._httpService.get('getUserProgressReport', [], [{
      'name': 'trackIds', 'value': trackIds
    }]);
  }

  public getTracksGrades(trackIds: string): Observable<any> {
    return this._httpService.get('getTracksGrades', [], [{
      'name': 'trackIds', 'value': trackIds
    }]);
  }

  public getFinanceReport(): Observable<any> {
    return this._httpService.get('getFinanceReport', [], [{
      'name': 'trackIds', 'value': 'trilha'
    }]);
  }
}
