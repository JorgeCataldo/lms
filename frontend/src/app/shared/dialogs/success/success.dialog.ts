import { Component, Input, Inject } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material';

@Component({
  selector: 'app-success-dialog',
  template: `
    <div class="created" >
      <img src="./assets/img/module-created.png" />
      <h2>{{ message }}</h2>
    </div>
    <p (click)="dismiss()" >OK</p>`,
  styleUrls: ['./success.dialog.scss']
})
export class SuccessDialogComponent {

  constructor(
    public dialogRef: MatDialogRef<SuccessDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public message: string
  ) { }

  public dismiss(): void {
    this.dialogRef.close();
  }

}
