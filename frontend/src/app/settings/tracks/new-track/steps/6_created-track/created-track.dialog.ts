import { Component } from '@angular/core';
import { MatDialogRef } from '@angular/material';

@Component({
  selector: 'app-created-track-dialog',
  template: `
    <div class="created-track" >
      <img src="./assets/img/module-created.png" />
      <h2>Sucesso ao<br>salvar Trilha!</h2>
    </div>
    <p (click)="dismiss()" >OK</p>`,
  styleUrls: ['./created-track.dialog.scss']
})
export class CreatedTrackDialogComponent {

  constructor(
    public dialogRef: MatDialogRef<CreatedTrackDialogComponent>
  ) { }

  public dismiss(): void {
    this.dialogRef.close();
  }

}
