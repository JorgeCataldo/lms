import { Component, Inject } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material';

@Component({
  selector: 'app-sidebar-event-application-note-dialog',
  templateUrl: './sidebar-event-application-note.dialog.html',
  styleUrls: ['./sidebar-event-application-note.dialog.scss']
})
export class SidebarEventApplicationNoteDialogComponent {

  constructor(
    public dialogRef: MatDialogRef<SidebarEventApplicationNoteDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data
  ) { }

  public dismiss(): void {
    this.dialogRef.close();
  }
}
