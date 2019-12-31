import { Component, Inject, OnInit } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA, MatSnackBar, MatDialog } from '@angular/material';
import { Question, Answer, ConceptAnswer } from '../../../../../../models/question.model';
import { UtilService } from '../../../../../../shared/services/util.service';
import { NotificationClass } from '../../../../../../shared/classes/notification';
import { Level } from '../../../../../../models/shared/level.interface';
import * as Editor from 'tui-editor';

@Component({
  selector: 'app-new-question-dialog',
  templateUrl: './new-question.dialog.html',
  styleUrls: ['./new-question.dialog.scss']
})
export class NewQuestionDialogComponent extends NotificationClass implements OnInit {

  public concepts: Array<string> = [];
  public selectedConcepts: Array<string> = [];
  public editor: Editor;

  constructor(
    protected _snackBar: MatSnackBar,
    public dialogRef: MatDialogRef<NewQuestionDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: {
      question: Question, concepts: Array<string>, levels: Array<Level>
    },
    private _utilService: UtilService,
    private _dialog: MatDialog
  ) {
    super(_snackBar);
    this.concepts = this.data.concepts;
    this.selectedConcepts = this.data.concepts.filter(
      concept => this.data.question.concepts.some(
        questionConcept => questionConcept === concept
      )
    );
  }

  ngOnInit() {
    this._configureEditor();
  }

  public addAnswer(): void {
    this.data.question.answers.push(
      new Answer( this.data.question.id, this.selectedConcepts )
    );
  }

  public removeAnswer(index: number): void {
    this.data.question.answers.splice(index, 1);
  }

  public getAnswerIconSrc(answer: Answer, concept: string): string {
    return answer.concepts.some(c => c.concept === concept && c.isRight) ?
      './assets/img/right-answer-full.png' :
      './assets/img/wrong-answer-full.png';
  }

  public toggleAnswer(answer: Answer, concept: string): void {
    const answerConcept = answer.concepts.find(c => c.concept === concept);
    answerConcept.isRight = !answerConcept.isRight;
  }

  public updateConcepts(concepts: Array<string>) {
    this.selectedConcepts = concepts;
    this.data.question.concepts = concepts;
    this.data.question.answers.forEach((answer) => {
      answer.concepts = concepts.map(concept => new ConceptAnswer(concept));
    });
  }

  public isString(duration): boolean {
    return !duration || typeof duration === 'string';
  }

  public getFormattedByDuration(duration: number): string {
    return this._utilService.formatDurationToHour(duration);
  }

  public dismiss(): void {
    this.dialogRef.close();
  }

  public save(): void {
    this.data.question.text = this.editor.getMarkdown();
    if (!this._validateQuestion()) return;
    this._saveQuestion();
  }

  private _configureEditor(): void {
    this.editor = new Editor({
      el: document.querySelector('#htmlEditor'),
      initialEditType: 'markdown',
      previewStyle: 'vertical',
      height: '200px'
    });

    this.editor.setMarkdown(
      this.data.question.text
    );
  }

  private _validateQuestion(): boolean {
    if (!this.data.question.text ||
        this.data.question.text.trim() === '' ||
        this.data.question.level == null) {
      this.notify('Preencha todos os campos obrigatórios para adicionar a questão');
      return false;
    }

    if (this.selectedConcepts.length === 0) {
      this.notify('Selecione pelo menos um conceito para ser avaliado pelas perguntas');
      return false;
    }

    if (!this.data.question.answers || this.data.question.answers.length < 2) {
      this.notify('Defina pelo menos duas respostas para adicionar a questão');
      return false;
    }

    const countAnswers = this.data.question.answers.filter(a => a.description.length <= 0);
    if (countAnswers.length > 1) {
      this.notify('Todas respostas devem ter uma descrição');
      return false;
    }

    const countRight = this.data.question.answers.filter(a => a.concepts.every(c => c.isRight));
    if (countRight.length > 1) {
      this.notify('Apenas uma resposta deve ter todos os conceitos certos');
      return false;
    } else if (countRight.length === 0) {
      this.notify('A questão deve ter pelo menos uma resposta com todos os conceitos certos');
      return false;
    }

    for (let index = 0; index < this.data.question.answers.length; index++) {
      const answer = this.data.question.answers[index];
      const rightCount = answer.concepts.reduce((count, concept) => {
        return concept.isRight ? (count + 1) : count;
      }, 0);

      const pointsCorrect = this._checkAnswerPoints( rightCount, answer.points );
      if (!pointsCorrect) { return false; }
    }

    return true;
  }

  private _saveQuestion() {
    this.data.question.duration = this._utilService.getDurationFromFormattedHour(
      (this.data.question.duration as any)
    );
    this.dialogRef.close( this.data.question );
  }

  private _checkAnswerPoints(rightCount: number, points: number): boolean  {
    switch (rightCount) {
      case 0:
        if (points !== -1) {
          this.notify('Uma resposta com todos os conceitos errado deve pontuar -1');
          return false;
        }
        break;
      case this.selectedConcepts.length:
        if (points !== 2) {
          this.notify('Uma resposta com todos os conceitos certos deve pontuar 2');
          return false;
        }
        break;
      default:
        if (points < 0 || points > 1) {
          this.notify('Uma resposta com conceitos certos e errados deve pontuar entre 0 e 1');
          return false;
        }
        break;
    }
    return true;
  }

}
