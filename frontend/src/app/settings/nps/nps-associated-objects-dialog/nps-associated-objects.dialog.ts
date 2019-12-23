import { Component, Inject } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material';

@Component({
  selector: 'app-nps-associated-objects-dialog',
  templateUrl: './nps-associated-objects.dialog.html',
  styleUrls: ['./nps-associated-objects.dialog.scss']
})
export class NpsAssociatedObjectsDialogComponent {

  constructor(
    public dialogRef: MatDialogRef<NpsAssociatedObjectsDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: {
      tracks: any[],
      modules: any[],
      events: any[]
    }
  ) { }

  public dismiss(): void {
    this.dialogRef.close();
  }
}
