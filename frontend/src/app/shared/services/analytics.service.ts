import { Injectable, EventEmitter } from '@angular/core';
import { BackendService } from '@tg4/http-infrastructure/dist/src';
import { Observable, of } from 'rxjs';
import { ActionInfo } from '../directives/save-action/action-info.interface';

@Injectable()
export class AnalyticsService {

  public showLoader$ = new EventEmitter();

  constructor(private _httpService: BackendService) { }

  public saveAction(actionInfo: ActionInfo): Observable<any> {
    return this._httpService.get('saveAction', [], [
      { name: 'pageId', value: actionInfo.page.toString() },
      { name: 'description', value: actionInfo.description },
      { name: 'typeId', value: actionInfo.type.toString() },
      { name: 'moduleId', value: actionInfo.moduleId || null },
      { name: 'eventId', value: actionInfo.eventId || null },
      { name: 'subjectId', value: actionInfo.subjectId || null },
      { name: 'contentId', value: actionInfo.contentId || null },
      { name: 'concept', value: actionInfo.concept || null },
      { name: 'supportMaterialId', value: actionInfo.supportMaterialId || null },
      { name: 'questionId', value: actionInfo.questionId || null }
    ]);
  }

  public removeAction(actionInfo: ActionInfo): Observable<any> {
    return this._httpService.post('removeAction', {
      'description': actionInfo.description,
      'moduleId': actionInfo.moduleId,
      'subjectId': actionInfo.subjectId,
      'contentId': actionInfo.contentId,
      'typeId': actionInfo.type
    });
  }

  public setWaitingAction(actionInfo: ActionInfo): string {
    const actions: Array<ActionInfo> = this.getStorageWaitingActions();
    actionInfo.id = this._generateUid();
    actions.push(actionInfo);
    this._setStorageWaitingActions( actions );
    return actionInfo.id;
  }

  public clearWaitingAction(id: string): void {
    const actions: Array<ActionInfo> = this.getStorageWaitingActions();
    const actionIndex = actions.findIndex(action => action.id === id);
    if (actionIndex != null) {
      actions.splice(actionIndex, 1);
      this._setStorageWaitingActions( actions );
    }
  }

  public clearAllActions() {
    localStorage.removeItem('waitingActions');
  }

  public getStorageWaitingActions(): Array<ActionInfo> {
    let actions: Array<ActionInfo> = [];
    const actionsStr = localStorage.getItem('waitingActions');
    if (actionsStr && actionsStr !== '' && actionsStr !== 'undefined') {
      actions = JSON.parse(actionsStr);
    }
    return actions;
  }

  public getStorageWaitingActionById(id: string): ActionInfo {
    const actions = this.getStorageWaitingActions();
    return actions.find(act => act.id === id);
  }

  private _setStorageWaitingActions(actions: Array<ActionInfo>): void {
    const actionsStr = JSON.stringify(actions);
    localStorage.setItem('waitingActions', actionsStr);
  }

  private _generateUid(): string {
    function s4() {
      return Math.floor((1 + Math.random()) * 0x10000)
        .toString(16)
        .substring(1);
    }
    return s4() + s4() + '-' + s4() + '-' + s4() + '-' + s4() + '-' + s4() + s4() + s4();
  }
}
