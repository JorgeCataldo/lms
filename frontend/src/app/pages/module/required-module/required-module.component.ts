import { Component, Input } from '@angular/core';
import { Requirement, RequirementProgress } from '../../../settings/modules/new-module/models/new-requirement.model';
import { Level } from '../../../models/shared/level.interface';
import { Router } from '@angular/router';

@Component({
  selector: 'app-required-module',
  templateUrl: './required-module.component.html',
  styleUrls: ['./required-module.component.scss']
})
export class RequiredModuleComponent {

  @Input() requirement: Requirement;
  @Input() levels: Array<Level>;
  @Input() last: boolean = false;

  constructor(
    private _router: Router
  ) { }

  public goToModule() {
    this._router.navigate([ '/modulo/' + this.requirement.moduleId ]);
  }

  public getLevelDescription(levelId: any): string {
    if (!this.levels || this.levels.length === 0) return '';
    const level = this.levels.find(lev => lev.id === levelId - 1);
    return level ? level.description : 'Sem badge';
  }

  public getBadgesProgressImageSrc(progress: RequirementProgress): string {
    if (!progress || progress.level === 0 && progress.progress === 0)
      return './assets/img/empty-badge.png';

    switch (progress.level) {
      case 1:
        return './assets/img/pencil-icon.png';
      case 2:
        return './assets/img/glasses-icon.png';
      case 3:
        return './assets/img/brain-icon.png';
      case 4:
        return './assets/img/brain-dark-icon-shadow.png';
      case 0:
      default:
        return './assets/img/empty-badge.png';
    }
  }

}
