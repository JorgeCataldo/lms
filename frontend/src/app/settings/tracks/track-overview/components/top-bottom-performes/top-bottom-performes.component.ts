import { Component, Input, EventEmitter, Output } from '@angular/core';
import { TrackOverview, StudentPerformance } from 'src/app/models/track-overview.interface';
import { Router } from '@angular/router';
import { User } from 'src/app/models/user.model';
import { MatDialog } from '@angular/material';
import { TrackOverviewTopBottomPerformesDialogComponent } from '../top-bottom-performes-dialog/top-bottom-performes.dialog';

@Component({
  selector: 'app-top-bottom-performes',
  template: `
    <div class="students" >
      <p class="title" >
        TOP PERFORMERS
      </p>
      <p class="no-students" *ngIf="_track && itemsCount === 0" >
        Ainda não há alunos inscritos nesta trilha.
      </p>
      <ul *ngIf="_track && itemsCount > 0" >
        <li *ngFor="let item of topPerformants; let index = index">
          <img class="photo"
            [src]="item.imageUrl || './assets/img/user-image-placeholder.png'"
          />
          <p class="item-title" >
            {{ item.name }}
          </p>
          <p class="top-performer-point">
            {{ item.points }}
          </p>
        </li>
      </ul>
      <div class="action" *ngIf="_track?.students && _track?.students.length > 0" >
        <button (click)="viewAllStudents(_track?.topPerformants, true)" >
          VER TUDO
        </button>
      </div>
    </div>
    <div class="students" style="margin-top: 15px">
      <p class="title" >
        BOTTOM PERFORMERS
      </p>
      <p class="no-students" *ngIf="_track && itemsCount === 0" >
        Ainda não há alunos inscritos nesta trilha.
      </p>
      <ul *ngIf="_track && itemsCount > 0" >
        <li *ngFor="let item of bottomPerformants; let index = index">
          <img class="photo"
            [src]="item.imageUrl || './assets/img/user-image-placeholder.png'"
          />
          <p class="item-title" >
            {{ item.name }}
          </p>
          <p class="bottom-performer-point">
            {{ item.points }}
          </p>
        </li>
      </ul>
      <div class="action" *ngIf="_track?.students && _track?.students.length > 0" >
        <button (click)="viewAllStudents(_track?.topPerformants, false)" >
          VER TUDO
        </button>
      </div>
    </div>`,
  styleUrls: ['./top-bottom-performes.component.scss']
})
export class TrackOverviewTopBottomPerformesComponent {
  public _track: TrackOverview;
  public topPerformants: Array<StudentPerformance>;
  public bottomPerformants: Array<StudentPerformance>;

  @Input() set track(ptrack: TrackOverview) {
    this._track = ptrack;
    this.topPerformants = ptrack.topPerformants.slice(0, 3);
    ptrack.topPerformants.sort((a, b) => a.points < b.points ? -1 : 1);
    this.bottomPerformants = ptrack.topPerformants.slice(0, 3);
    ptrack.topPerformants.sort((a, b) => a.points < b.points ? 1 : -1);
  }
  @Input() readonly itemsCount: number = 0;

  constructor(
    private _dialog: MatDialog
  ) { }

  public viewAllStudents(students: Array<User>, isTop: boolean): void {
    this._dialog.open(TrackOverviewTopBottomPerformesDialogComponent, {
      width: '600px',
      data: { 'students': students, 'isTop': isTop }
    });
  }
}
