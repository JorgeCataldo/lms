import { Component, Inject } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material';
import { PreparationQuestion } from '../../../../models/preparation-question.model';
import { EventApplication } from '../../../../models/event-application.model';

@Component({
  selector: 'app-answers-dialog',
  template: `
    <div class="answers-dialog" >
      <img class="close"
        src="./assets/img/close-colored.png"
        (click)="dismiss()"
      />
      <h2>PERGUNTAS DE PREPARAÇÃO</h2>

      <div class="user-info" >
        <div class="info" >
          <img [src]="eventApplication.user.imageUrl" />
          <p>
            {{ eventApplication.user.name }}<br>
            <small>{{ eventApplication.user.rank ? eventApplication.user.rank.name : '-'  }}</small>
          </p>
        </div>
        <p class="status" >
          APLICAÇÃO PENDENTE
        </p>
      </div>

      <div class="scrollable" >
        <div class="question-container"
          *ngFor="let answer of eventApplication.prepQuizAnswersList; let index = index" >
          <p class="number" >
            {{ index + 1 }}.
          </p>
          <div class="answer-container" >
            <p class="question" >
              {{ eventApplication.prepQuiz.questions[index] }}
            </p>
            <p class="answer" >
            <a href="{{answer.answer}}" *ngIf="answer.fileAsAnswer" download>
              <img src="./assets/img/download_azul.svg" *ngIf="answer.fileAsAnswer" >
            </a>
            {{ answer.fileAsAnswer ? '': answer.answer }}
            </p>
          </div>
        </div>
      </div>
    </div>`,
  styleUrls: ['./answers.dialog.scss']
})
export class AnswersDialogComponent {

  constructor(
    public dialogRef: MatDialogRef<AnswersDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public eventApplication: EventApplication
  ) {
  }

  public dismiss(): void {
    this.dialogRef.close();
  }

}
