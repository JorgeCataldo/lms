import { Component, Inject } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material';

@Component({
  selector: 'app-event-application-note-dialog',
  templateUrl: './event-application-note.dialog.html',
  styleUrls: ['./event-application-note.dialog.scss']
})
export class EventApplicationNoteDialogComponent {

  constructor(public dialogRef: MatDialogRef<EventApplicationNoteDialogComponent>,
              @Inject(MAT_DIALOG_DATA) public data: {message: string}) { }

  public dismiss(): void {
    this.dialogRef.close();
  }
}
