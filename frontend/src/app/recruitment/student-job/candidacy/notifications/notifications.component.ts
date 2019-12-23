import { Component, Input } from '@angular/core';
import { MatDialog } from '@angular/material';
import { AvailableCandidate } from 'src/app/models/previews/user-job-application.interface';
import { Router } from '@angular/router';
import { UserJobsNotifications } from 'src/app/models/user-jobs-position.interface';
import { NotificationsDialogComponent } from '../notifications-dialog/notifications.dialog';


@Component({
  selector: 'app-notifications',
  template: `
    <div class="students" >
      <p class="title" >
        <span>AVISOS</span>
      </p>
      <p class="no-students" *ngIf="!jobNotifications || jobNotifications.length === 0" >
        Ainda não há avisos há serem mostrado.
      </p>
      <ul *ngIf="jobNotifications">
        <ng-container *ngFor="let item of jobNotifications; let index = index">
          <li [ngClass]= "{ 'selected': item.read }">
            <div class="item-title" [ngClass]= "{ 'selected': item.read }" >
              <p>{{item.title}}<br></p>
              <span>{{item.text}}</span>
            </div>
          </li>
        </ng-container>
      </ul>
      <div class="action" *ngIf="jobNotifications && jobNotifications.length > 0" >
        <button (click)="openNotifications()">
          VER TUDO
        </button>
      </div>
    </div>`,
  styleUrls: ['./notifications.component.scss']
})
export class NotificationsComponent {

  public jobNotifications: UserJobsNotifications[] = [];
  public allJobNotifications: UserJobsNotifications[] = [];

  @Input() set notifications(notifications: UserJobsNotifications[]) {
    this.jobNotifications = notifications.slice(0, 4);
    this.allJobNotifications = notifications;
  }

  constructor(
    private _dialog: MatDialog
  ) { }

  public openNotifications() {
    this._dialog.open(NotificationsDialogComponent, {
      width: '400px',
      data: this.allJobNotifications
    });
  }
}
