import { Directive, HostListener, Input } from '@angular/core';
import { ActionInfo, ActionInfoTypeEnum } from './action-info.interface';
import { AnalyticsService } from '../../services/analytics.service';

@Directive({
  // tslint:disable-next-line:directive-selector
  selector: '[saveAction]'
})
export class SaveActionDirective {

  constructor(
    private _actionsService: AnalyticsService
  ) { }

  @Input('saveAction') saveAction: ActionInfo;

  @HostListener('click') onClick() {
    if (this.saveAction.type === ActionInfoTypeEnum.Click) {
      this._saveAction( this.saveAction );
    }
  }

  private _saveAction(actionInfo: ActionInfo): void {
    this._actionsService.saveAction(
      actionInfo
    ).subscribe(
      () => { },
      (error) => { console.error(error); }
    );
  }
}
