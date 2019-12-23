import { Component, Inject } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material';
import { InvalidSubjectItem } from 'src/app/models/question.model';

@Component({
  selector: 'app-missing-levels-dialog',
  template: `
    <div class="created-module" >
      <h2>Não foi possível completar o cadastro de perguntas do módulo!</h2>
      <p>É necessário que cada assunto possua no mínimo uma pergunta de cada nível.</p>
      <br>
      <p>Perguntas de níveis necessárias em cada assunto:</p>
      <ng-container *ngFor="let invalidSub of data">
        <p>
          - {{invalidSub.subjectTitle}} :
          <span *ngFor="let missingLevel of invalidSub.missingLevels; let i = index">
            {{missingLevel.description}}<span *ngIf="i < invalidSub.missingLevels.length - 1">,</span>
          </span>.
        </p>

      </ng-container>
    </div>
    <div class="actions" >
      <p class="btn" (click)="dismiss(false)" >
        voltar
      </p>
      <p class="btn" (click)="dismiss(true)" >
        finalizar mesmo assim
      </p>
    </div>`,
  styleUrls: ['./missing-levels.dialog.scss']
})
export class MissingLevelsDialogComponent {

  constructor(
    public dialogRef: MatDialogRef<MissingLevelsDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: Array<InvalidSubjectItem>,
  ) { }

  public dismiss(resolution: boolean): void {
    this.dialogRef.close( resolution );
  }

}
