import { Component, Inject } from '@angular/core';
import { MatDialogRef, MatSnackBar, MAT_DIALOG_DATA } from '@angular/material';
import { NotificationClass } from '../../classes/notification';
import { Router } from '@angular/router';

@Component({
  selector: 'app-event-forum-participation-dialog',
  templateUrl: './event-forum-participation.dialog.html',
  styleUrls: ['./event-forum-participation.dialog.scss']
})
export class EventForumParticipationDialogComponent extends NotificationClass {

  constructor(
    protected _snackBar: MatSnackBar,
    public dialogRef: MatDialogRef<EventForumParticipationDialogComponent>,
    private _router: Router,
    @Inject(MAT_DIALOG_DATA) public data
  ) {
    super(_snackBar);
  }

  public dismiss(): void {
    this.dialogRef.close();
  }

  public goToForum(eventId: string, eventScheduleId: string, questionId: string) {
    this.dialogRef.close();
    this._router.navigate([ '/forum-evento/' + eventId + '/' + eventScheduleId + '/' + questionId ]);
  }
}
