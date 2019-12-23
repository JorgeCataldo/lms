import { Component, Inject } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material';

@Component({
  selector: 'app-delete-event-dialog',
  template: `
    <div class="created-event" >
      <!--img src="./assets/img/event-created.png" /-->
      <h2 *ngIf="data">
        Este evento possui alunos associados a ele.
        <br>
        Deseja deletar mesmo assim?
      </h2>
      <h2 *ngIf="!data">
        Ter certeza que deseja deletar este evento?
      </h2>
    </div>
    <p style="padding-bottom: 20px;" (click)="dismiss(true)">OK</p>
    <p (click)="dismiss(false)" >Cancelar</p>`,
  styleUrls: ['./delete-event.dialog.scss']
})
export class DeleteEventDialogComponent {

  constructor(
    public dialogRef: MatDialogRef<DeleteEventDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: boolean
  ) { }

  public dismiss(state: boolean): void {
    this.dialogRef.close(state);
  }

}
