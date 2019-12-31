import { Component } from '@angular/core';
import { MatDialogRef } from '@angular/material';

@Component({
  selector: 'app-created-module-dialog',
  template: `
    <div class="created-module" >
      <img src="./assets/img/module-created.png" />
      <h2>Sucesso ao<br>salvar Módulo!</h2>
    </div>
    <p (click)="dismiss()" >OK</p>`,
  styleUrls: ['./created-module.dialog.scss']
})
export class CreatedModuleDialogComponent {

  constructor(
    public dialogRef: MatDialogRef<CreatedModuleDialogComponent>
  ) { }

  public dismiss(): void {
    this.dialogRef.close();
  }

}
