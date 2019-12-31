import { Injectable } from '@angular/core';
import { BackendService } from '@tg4/http-infrastructure/dist/src';
import { Observable } from 'rxjs';
import { Module, ModuleWeights } from '../../models/module.model';
import { Subject } from '../../models/subject.model';
import { Content } from '../../models/content.model';
import { SupportMaterial } from '../../models/support-material.interface';
import { Requirement } from '../modules/new-module/models/new-requirement.model';
import { Question } from 'src/app/models/question.model';

@Injectable()
export class SettingsModulesDraftsService {

  constructor(private _httpService: BackendService) { }

  public getPagedModulesAndDrafts(
    page: number = 1, pageSize: number = 10, searchTerm: string = ''
  ): Observable<any> {
    return this._httpService.post('getPagedModulesAndDrafts', {
      'page': page,
      'pageSize': pageSize,
      'filters': {
        'term': searchTerm
      }
    });
  }

  public getDraftById(moduleId: string): Observable<any> {
    return this._httpService.get('getDraftById', [], [
      { 'name': 'id', 'value': moduleId }
    ]);
  }

  public addNewModuleDraft(module: Module): Observable<any> {
    return this._httpService.post('addNewModuleDraft', module);
  }

  public cloneNewModuleDraft(module: Module): Observable<any> {
    return this._httpService.post('cloneModuleDraft', module);
  }

  public updateModuleDraft(module: Module): Observable<any> {
    return this._httpService.post('updateModuleDraft', module);
  }

  public manageDraftSubjects(moduleId: string, subjects: Array<Subject>): Observable<any> {
    return this._httpService.post('manageDraftSubjects', {
      'moduleId': moduleId,
      'deleteNonExistent': true,
      'subjects': subjects
    });
  }

  public manageDraftModuleWeight(moduleId: string, weight: Array<ModuleWeights>): Observable<any> {
    console.log('weight -> ', weight);
    return this._httpService.put('manageModuleWeight', {
      'moduleId': moduleId,
      'weight': weight[0]
    });
  }

  public manageDraftContents(moduleId: string, subjectId: string, contents: Array<Content>): Observable<any> {
    return this._httpService.post('manageDraftContents', {
      'moduleId': moduleId,
      'subjectId': subjectId,
      'deleteNonExistent': true,
      'contents': contents
    });
  }

  public manageDraftSupportMaterials(moduleId: string, materials: Array<SupportMaterial>): Observable<any> {
    return this._httpService.post('manageDraftSupportMaterials', {
      'moduleId': moduleId,
      'deleteNonExistent': true,
      'supportMaterials': materials
    });
  }

  public manageDraftRequirements(moduleId: string, requirements: Array<Requirement>): Observable<any> {
    return this._httpService.post('manageDraftRequirements', {
      'moduleId': moduleId,
      'requirements': requirements
    });
  }

  public publishDraft(moduleId: string): Observable<any> {
    return this._httpService.get('publishDraft', [], [
      { 'name': 'moduleId', 'value': moduleId }
    ]);
  }

  public rejectDraft(moduleId: string): Observable<any> {
    return this._httpService.post('rejectDraft', {
      'moduleId': moduleId
    });
  }

  public manageQuestionDraft(moduleId: string, question: Question): Observable<any> {
    question.moduleId = moduleId;
    return this._httpService.post('manageQuestionDraft', question);
  }

  public removeQuestionDraft(questionId: string): Observable<any> {
    return this._httpService.delete('removeQuestionDraft', [{
      'name': 'id', 'value': questionId
    }]);
  }

  public getAllQuestionsDraft(moduleId: string): Observable<any> {
    return this._httpService.get('getAllQuestionsDraft', [], [{
      'name': 'moduleId', 'value': moduleId
    }]);
  }

  public getPagedQuestionsDraft(subjectId: string, moduleId: string,
    page: number = 1, pageSize: number = 10, searchValue: string = ''
  ): Observable<any> {
    return this._httpService.post('getPagedQuestionsDraft', {
      'page': page,
      'pageSize': pageSize,
      'filters': {
        'term': searchValue,
        'subjectId': subjectId,
        'moduleId': moduleId
      }
    });
  }

  public setDraftQuestionsLimit(moduleId: string, questionsLimit: number = null): Observable<any> {
    return this._httpService.put('setDraftQuestionsLimit', {
      'moduleId': moduleId,
      'questionsLimit': questionsLimit
    });
  }

  public importDraftQdb(file: string, moduleId: string, addQuestions: boolean): Observable<any> {
    return this._httpService.post('importDraftQdb', {
      'file': file,
      'moduleId': moduleId,
      'addQuestions': addQuestions
    });
  }
}
