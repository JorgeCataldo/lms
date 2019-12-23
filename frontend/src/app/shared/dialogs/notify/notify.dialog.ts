import { Component, Inject } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material';

@Component({
  selector: 'app-notify-dialog',
  template: `
    <div class="notify-dialog" >
      <p [innerHTML]="data?.message" ></p>

      <div class="selection-btns" >
        <button class="btn-test" (click)="dismiss()" >
          Ok
        </button>
      </div>
    </div>
  `,
  styleUrls: ['./notify.dialog.scss']
})
export class NotifyDialogComponent {

  constructor(public dialogRef: MatDialogRef<NotifyDialogComponent>,
              @Inject(MAT_DIALOG_DATA) public data: { message: string }) { }

  public dismiss(): void {
    this.dialogRef.close();
  }

}
