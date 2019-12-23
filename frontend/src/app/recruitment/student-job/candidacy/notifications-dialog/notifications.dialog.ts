import { Component, Inject } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material';
import { UserJobsNotifications } from 'src/app/models/user-jobs-position.interface';

@Component({
  selector: 'app-notifications-dialog',
  template: `
    <div class="concepts" >
      <ul>
        <li *ngFor="let item of data" >
        <div class="item-title">
          <p>{{item.title}}<br></p>
          <span>{{item.text}}</span>
        </div>
        </li>
      </ul>
    </div>
    <p class="dismiss" (click)="dismiss()" >
      fechar
    </p>`,
  styleUrls: ['./notifications.dialog.scss']
})
export class NotificationsDialogComponent {

  constructor(
    public dialogRef: MatDialogRef<NotificationsDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: UserJobsNotifications[]
  ) { }

  public dismiss(): void {
    this.dialogRef.close();
  }

}
