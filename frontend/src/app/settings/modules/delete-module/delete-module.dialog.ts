import { Component, Inject } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material';

@Component({
  selector: 'app-delete-module-dialog',
  template: `
    <div class="created-module" >
      <h2 *ngIf="hasUsersProgress">
        Este módulo possui alunos associados a ele.
        <br>
        Deseja deletar mesmo assim?
      </h2>
      <h2 *ngIf="!hasUsersProgress">
        Ter certeza que deseja deletar este módulo?
      </h2>
    </div>
    <p style="padding-bottom: 20px;" (click)="dismiss(true)">OK</p>
    <p (click)="dismiss(false)" >Cancelar</p>`,
  styleUrls: ['./delete-module.dialog.scss']
})
export class DeleteModuleDialogComponent {

  constructor(
    public dialogRef: MatDialogRef<DeleteModuleDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public hasUsersProgress: boolean
  ) { }

  public dismiss(state: boolean): void {
    this.dialogRef.close(state);
  }

}
