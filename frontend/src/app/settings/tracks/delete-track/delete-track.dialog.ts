import { Component } from '@angular/core';
import { MatDialogRef } from '@angular/material';

@Component({
  selector: 'app-delete-track-dialog',
  template: `
    <div class="created-module" >
      <!--img src="./assets/img/module-created.png" /-->
      <h2 >
        Ter certeza que deseja deletar esta trilha?
      </h2>
    </div>
    <p style="padding-bottom: 20px;" (click)="dismiss(true)">OK</p>
    <p (click)="dismiss(false)" >Cancelar</p>`,
  styleUrls: ['./delete-track.dialog.scss']
})
export class DeleteTrackDialogComponent {

  constructor(
    public dialogRef: MatDialogRef<DeleteTrackDialogComponent>
  ) { }

  public dismiss(state: boolean): void {
    this.dialogRef.close(state);
  }

}
