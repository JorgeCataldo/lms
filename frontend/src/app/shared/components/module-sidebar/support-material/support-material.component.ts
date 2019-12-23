import { Component, Input } from '@angular/core';
import { SupportMaterial } from '../../../../models/support-material.interface';
import { ActionInfoPageEnum, ActionInfoTypeEnum, ActionInfo } from 'src/app/shared/directives/save-action/action-info.interface';

@Component({
  selector: 'app-support-material',
  templateUrl: './support-material.component.html',
  styleUrls: ['./support-material.component.scss']
})
export class SupportMaterialComponent {

  @Input() readonly material: SupportMaterial;
  @Input() readonly moduleId?: string;
  @Input() readonly isEvent: boolean = false;

  public getActionInfo(description: string): ActionInfo {
    return {
      page: this.isEvent ? ActionInfoPageEnum.Event : ActionInfoPageEnum.Module,
      description: description,
      type: ActionInfoTypeEnum.Click,
      moduleId: this.isEvent ? null : (this.moduleId || null),
      eventId: this.isEvent ? (this.moduleId || null) : null,
      supportMaterialId: this.material.id
    };
  }

}
