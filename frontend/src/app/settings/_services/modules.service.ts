import { Injectable } from '@angular/core';
import { BackendService } from '@tg4/http-infrastructure/dist/src';
import { Observable } from 'rxjs';
import { Module } from '../../models/module.model';
import { Subject } from '../../models/subject.model';
import { Content } from '../../models/content.model';
import { SupportMaterial } from '../../models/support-material.interface';
import { Requirement } from '../modules/new-module/models/new-requirement.model';
import { Question } from '../../models/question.model';
import { EcommerceProduct } from 'src/app/models/ecommerce-product.model';

@Injectable()
export class SettingsModulesService {

  constructor(private _httpService: BackendService) { }

  public addNewModule(module: Module): Observable<any> {
    return this._httpService.post('addNewModule', module);
  }

  public updateModuleInfo(module: Module): Observable<any> {
    return this._httpService.post('updateModuleInfo', module);
  }

  public manageSubjects(moduleId: string, subjects: Array<Subject>): Observable<any> {
    return this._httpService.post('manageSubjects', {
      'moduleId': moduleId,
      'deleteNonExistent': true,
      'subjects': subjects
    });
  }

  public manageContents(moduleId: string, subjectId: string, contents: Array<Content>): Observable<any> {
    return this._httpService.post('manageContents', {
      'moduleId': moduleId,
      'subjectId': subjectId,
      'deleteNonExistent': true,
      'contents': contents
    });
  }

  public manageSupportMaterials(moduleId: string, materials: Array<SupportMaterial>): Observable<any> {
    return this._httpService.post('manageSupportMaterials', {
      'moduleId': moduleId,
      'deleteNonExistent': true,
      'supportMaterials': materials
    });
  }

  public manageRequirements(moduleId: string, requirements: Array<Requirement>): Observable<any> {
    return this._httpService.post('manageRequirements', {
      'moduleId': moduleId,
      'requirements': requirements
    });
  }

  public manageQuestion(moduleId: string, question: Question): Observable<any> {
    question.moduleId = moduleId;
    return this._httpService.post('manageQuestion', question);
  }

  public removeQuestion(questionId: string): Observable<any> {
    return this._httpService.delete('removeQuestion', [{
      'name': 'id', 'value': questionId
    }]);
  }

  public getPagedFilteredModulesList(page: number = 1, pageSize: number = 10, searchValue: string = ''): Observable<any> {
    return this._httpService.post('getPagedFilteredModulesList', {
      'page': page,
      'pageSize': pageSize,
      'filters': { 'term': searchValue }
    });
  }


  public importQdb(file: string, moduleId: string, addQuestions: boolean): Observable<any> {
    return this._httpService.post('importQdb', {
      'file': file,
      'moduleId': moduleId,
      'addQuestions': addQuestions
    });
  }

  public getPagedQuestionsList(page: number = 1, pageSize: number = 10): Observable<any> {
    return this._httpService.get('getPagedQuestionsList', [], [
      { 'name': 'page', 'value': page.toString() },
      { 'name': 'pageSize', 'value': pageSize.toString() }
    ]);
  }

  public getQuestionsList(moduleId: string): Observable<any> {
    return this._httpService.post('getQuestionsList', {
      'moduleId': moduleId
    });
  }

  public getPagedFilteredQuestionsList(subjectId: string,
    page: number = 1, pageSize: number = 10, searchValue: string = ''
  ): Observable<any> {
    return this._httpService.post('getPagedFilteredQuestionsList', {
      'page': page,
      'pageSize': pageSize,
      'filters': {
        'term': searchValue,
        'subjectId': subjectId
      }
    });
  }

  public checkModuleQuestions(moduleId: string, subjectIds: Array<string>): Observable<any> {
    return this._httpService.post('validateModuleQuestions', {
      'moduleId': moduleId,
      'subjectIds': subjectIds
    });
  }

  public setQuestionsLimit(moduleId: string, questionsLimit: number = null): Observable<any> {
    return this._httpService.put('setQuestionsLimit', {
      'moduleId': moduleId,
      'questionsLimit': questionsLimit
    });
  }

  public getPagedFilteredManagerModulesList(page: number = 1, pageSize: number = 10, searchValue: string = ''): Observable<any> {
    return this._httpService.post('getPagedManagerFilteredModulesList', {
      'page': page,
      'pageSize': pageSize,
      'filters': { 'term': searchValue }
    });
  }

  public getEffectivenessIndicators(trackIds: string = ''): Observable<any> {
    return this._httpService.get('effectivenessIndicators', [], [
      { 'name': 'trackIds', 'value': trackIds },
    ]);
  }

  public getModuleOverview(moduleId: string): Observable<any> {
    return this._httpService.get('getModuleOverview', [], [
      { 'name': 'moduleId', 'value': moduleId }
    ]);
  }

  public manageEcommerceProducts(trackId: string, products: Array<EcommerceProduct>): Observable<any> {
    return this._httpService.put('manageEcommerceModuleDraft', {
      'moduleId': trackId,
      'products': products
    });
  }
}
