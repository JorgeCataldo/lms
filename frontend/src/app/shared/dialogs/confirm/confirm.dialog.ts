import { Component, Inject } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material';

@Component({
  selector: 'app-confirm-dialog',
  template: `
    <div class="confirm-dialog" >
      <p [innerHTML]="data?.message" ></p>

      <div class="selection-btns" >
        <button class="btn-test" (click)="dismiss(true)" >
          Sim
        </button>
        <button class="btn-outline" (click)="dismiss(false)" >
          Não
        </button>
      </div>
    </div>
  `,
  styleUrls: ['./confirm.dialog.scss']
})
export class ConfirmDialogComponent {

  constructor(public dialogRef: MatDialogRef<ConfirmDialogComponent>,
              @Inject(MAT_DIALOG_DATA) public data: { message: string }) { }

  public dismiss(result: boolean): void {
    this.dialogRef.close( result );
  }

}
