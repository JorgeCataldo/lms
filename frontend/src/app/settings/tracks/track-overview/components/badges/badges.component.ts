import { Component, Input } from '@angular/core';
import { StudentsProgress, StudentPerformance, StudentProgress } from 'src/app/models/track-overview.interface';
import { MatDialog, MatSnackBar } from '@angular/material';
import { SettingsTracksService } from 'src/app/settings/_services/tracks.service';
import { NotificationClass } from 'src/app/shared/classes/notification';
import { TrackOverviewTopBottomPerformesDialogComponent } from '../top-bottom-performes-dialog/top-bottom-performes.dialog';
import { TrackModule } from 'src/app/models/track-module.model';
import { TrackOverviewBadgesDialogComponent } from '../badges-dialog/badges.dialog';

@Component({
  selector: 'app-track-overview-badges',
  template: `
    <div class="badges" *ngIf="progresses && progresses.length > 0" >
      <div class="badge" (click)="checkBadgeProgress()" >
        <img src="./assets/img/empty-badge.png" />
        <p class="not" >
          {{ progresses[0].count }}<br>
          <span>NÃO INICIARAM</span>
        </p>
      </div>
      <div class="badge" (click)="checkBadgeProgress(1, 'INICIANTE')" >
        <img src="./assets/img/pencil-icon-shadow.png" />
        <p class="beginner" >
          {{ progresses[1].count }}<br>
          <span>INICIANTE</span>
        </p>
      </div>
      <div class="badge" (click)="checkBadgeProgress(2, 'INTERMEDIÁRIO')" >
        <img src="./assets/img/glasses-icon-shadow.png" />
        <p class="intermediate" >
          {{ progresses[2].count }}<br>
          <span>INTERMEDIÁRIO</span>
        </p>
      </div>
      <div class="badge" (click)="checkBadgeProgress(3, 'AVANÇADO')" >
        <img src="./assets/img/brain-icon-shadow.png" />
        <p class="advanced" >
          {{ progresses[3].count }}<br>
          <span>AVANÇADO</span>
        </p>
      </div>
      <div class="badge" (click)="checkBadgeProgress(4, 'EXPERT')" >
        <img src="./assets/img/brain-dark-icon-shadow.png" />
        <p class="expert" >
          {{ progresses[4].count }}<br>
          <span>EXPERT</span>
        </p>
      </div>
    </div>`,
  styleUrls: ['./badges.component.scss']
})
export class TrackOverviewBadgesComponent extends NotificationClass {

  @Input() readonly progresses: Array<StudentsProgress> = [];
  @Input() readonly modulesConfiguration: Array<TrackModule> = [];
  @Input() readonly topPerformants: Array<StudentPerformance> = [];
  @Input() readonly students: Array<StudentProgress> = [];

  constructor(
    protected _snackBar: MatSnackBar,
    private _dialog: MatDialog
  ) {
    super(_snackBar);
  }

  public checkBadgeProgress(badge: number = null, title: string = 'NÃO INICIARAM') {
    let moduleStudentsBadges = [];
    if (this.topPerformants.length > 0) {
      moduleStudentsBadges = this._trackOverviewPopUp(badge);
    } else if (this.students.length > 0) {
      moduleStudentsBadges = this._trackModuleOverviewPopUp(badge);
    }
    this._dialog.open(TrackOverviewBadgesDialogComponent, {
      width: '600px',
      data: {
        'title': title,
        'modulesBadge': moduleStudentsBadges
      }
    });
  }

  private _trackOverviewPopUp(badge: number): any[] {
    const moduleStudentsBadges = [];

    if (badge !== null) {
      for (let i = 0; i < this.modulesConfiguration.length; i++) {
        const moduleStudentsBadge = this.modulesConfiguration[i].students.filter(x => x.level === badge);
        if (moduleStudentsBadge.length === 0)
          continue;
        moduleStudentsBadges.push({
          'moduleName': this.modulesConfiguration[i].title,
          'students': moduleStudentsBadge
        });
      }
    } else {
      const notStarted = this.topPerformants.filter(x => x.points === 0);
      if (notStarted) {
        moduleStudentsBadges.push({
          'moduleName': 'Não Iniciaram Nenhum Módulo',
          'students': notStarted
        });
      }
    }
    return moduleStudentsBadges;
  }

  private _trackModuleOverviewPopUp(badge: number): any[] {
    const moduleStudentsBadges = [];
    const moduleStudentsBadge = this.students.filter(x => x.level === badge);
    moduleStudentsBadges.push({
      'moduleName': this.modulesConfiguration[0].title,
      'students': moduleStudentsBadge
    });
    return moduleStudentsBadges;
  }
}
