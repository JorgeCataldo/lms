import { Component } from '@angular/core';
import { MatDialogRef } from '@angular/material';

@Component({
  selector: 'app-created-event-dialog',
  template: `
    <div class="created-event" >
      <img src="./assets/img/module-created.png" />
      <h2>Sucesso ao<br>salvar Evento!</h2>
    </div>
    <p (click)="dismiss()" >OK</p>`,
  styleUrls: ['./created-event.dialog.scss']
})
export class CreatedEventDialogComponent {

  constructor(
    public dialogRef: MatDialogRef<CreatedEventDialogComponent>
  ) { }

  public dismiss(): void {
    this.dialogRef.close();
  }

}
