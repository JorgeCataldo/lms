import { Component, Input } from '@angular/core';
import { MatDialog } from '@angular/material';
import { Module } from '../../../models/module.model';
import { UtilService } from '../../../shared/services/util.service';
import { Level } from 'src/app/models/shared/level.interface';
import { UserService } from '../../_services/user.service';
import { NotifyDialogComponent } from 'src/app/shared/dialogs/notify/notify.dialog';

@Component({
  selector: 'app-module-header',
  templateUrl: './module-header.component.html',
  styleUrls: ['./module-header.component.scss']
})
export class ModuleHeaderComponent {

  @Input() module: Module;
  @Input() moduleProgress: any;
  @Input() levels: any;
  @Input() levelList: any;
  @Input() disableGrade: boolean;

  constructor(
    private _utilService: UtilService,
    private _userService: UserService,
    private _dialog: MatDialog,
  ) { }

  public getFormattedHour(): string {
    if (!this.module || !this.module.duration) return '';
    return this._utilService.formatSecondsToHourMinute(this.module.duration);
  }

  public getLevelClass(level) {
    return this._userService.getLevelClass(level);
  }

  public getLevelImage(level: number, currentLevel: number) {
    if (!currentLevel || currentLevel <= level)
      return './assets/img/empty-badge.png';
    else
      return this._userService.getLevelImage(level);
  }

  public getLevelColor(level: number, currentLevel: number) {
    if (currentLevel > level)
      return this._userService.getLevelColor(level);
    else
      return 'var(--semi-primary-color)';
  }

  public openGradeDialog() {

    let message = '';

    if (this.module.moduleConfiguration.bdqWeight === 0 &&
      this.module.moduleConfiguration.evaluationWeight === 0) {
        message = 'A sua nota do módulo foi calculada pela média aritmética entre a nota do BDQ e a nota da prova.';
    } else {
      if (this.module.moduleConfiguration.bdqWeight === 0 &&
        this.module.moduleConfiguration.evaluationWeight > 0) {
          message = 'A sua nota do módulo foi calculada pela sua nota do BDQ.';
      } else if (this.module.moduleConfiguration.bdqWeight > 0 &&
        this.module.moduleConfiguration.evaluationWeight === 0) {
          message = 'A sua nota do módulo foi calculada pela nota da sua prova.';
      } else if (this.module.moduleConfiguration.bdqWeight > 0 &&
        this.module.moduleConfiguration.evaluationWeight > 0) {
          message = 'A sua nota do módulo foi calculada pela média ponderada entre a nota da sua prova e a nota do BDQ.';
      }
    }

    this._dialog.open(NotifyDialogComponent, {
      width: '400px',
      data: {
        message: message
      }
    });
  }

}
