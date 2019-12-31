import { Component, Inject } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material';

@Component({
  selector: 'app-clone-module-dialog',
  template: `
    <div class="created-module" >
      <h2>
        Tem certeza que deseja duplicar este módulo?
      </h2>
    </div>
    <p style="padding-bottom: 20px;" (click)="dismiss(true)">OK</p>
    <p (click)="dismiss(false)" >Cancelar</p>`,
  styleUrls: ['./clone-module.dialog.scss']
})
export class CloneModuleDialogComponent {

  constructor(
    public dialogRef: MatDialogRef<CloneModuleDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public hasUsersProgress: boolean
  ) { }

  public dismiss(state: boolean): void {
    this.dialogRef.close(state);
  }

}
