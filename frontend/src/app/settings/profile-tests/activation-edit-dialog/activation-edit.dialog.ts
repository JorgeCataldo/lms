import { Component, Inject } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material';
import { Activation } from 'src/app/models/activation.model';

@Component({
  selector: 'app-activation-edit-dialog',
  templateUrl: './activation-edit.dialog.html',
  styleUrls: ['./activation-edit.dialog.scss']
})
export class ActivationEditDialogComponent {

  constructor(
    public dialogRef: MatDialogRef<ActivationEditDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: Activation
  ) { }

  public save(): void {
    this.dialogRef.close(this.data);
  }

  public dismiss(): void {
    this.dialogRef.close();
  }
}
