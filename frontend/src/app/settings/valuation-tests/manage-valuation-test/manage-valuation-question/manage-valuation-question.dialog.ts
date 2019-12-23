import { Component, Inject, OnInit } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA, MatSnackBar } from '@angular/material';
import { ValuationTestQuestion, ValuationTestQuestionOption, ValuationTestQuestionTypeEnum } from 'src/app/models/valuation-test.interface';
import * as Editor from 'tui-editor';
import { NotificationClass } from 'src/app/shared/classes/notification';

@Component({
  selector: 'app-manage-valuation-question-dialog',
  templateUrl: './manage-valuation-question.dialog.html',
  styleUrls: ['./manage-valuation-question.dialog.scss']
})
export class ManageValuationQuestionDialogComponent extends NotificationClass implements OnInit {

  public editor: Editor;

  constructor(
    protected _snackBar: MatSnackBar,
    public dialogRef: MatDialogRef<ManageValuationQuestionDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: ValuationTestQuestion
  ) {
    super(_snackBar);
  }

  ngOnInit() {
    this._configureEditor();
  }

  public addAnswer(): void {
    this.data.options = this.data.options || [];
    this.data.options.push( new ValuationTestQuestionOption() );
  }

  public removeAnswer(index: number): void {
    this.data.options.splice(index, 1);
  }

  public getAnswerIconSrc(answer: ValuationTestQuestionOption): string {
    return answer.correct ?
      './assets/img/right-answer-full.png' :
      './assets/img/wrong-answer-full.png';
  }

  public toggleAnswer(answer: ValuationTestQuestionOption): void {
    answer.correct = !answer.correct;
  }

  public dismiss(): void {
    this.dialogRef.close();
  }

  public save(): void {
    if (this._checkQuestion()) {
      this.data.title = this.editor.getMarkdown();
      this.dialogRef.close( this.data );
    }
  }

  private _configureEditor(): void {
    this.editor = new Editor({
      el: document.querySelector('#htmlEditor'),
      initialEditType: 'markdown',
      previewStyle: 'vertical',
      height: '200px'
    });

    this.editor.setMarkdown(
      this.data.title
    );
  }

  private _checkQuestion(): boolean {
    if (this.data.type === ValuationTestQuestionTypeEnum.MultipleChoice) {
      if (!this.data.options.some(o => o.correct)) {
        this.notify('Deve haver pelo menos uma resposta correta');
        return false;
      }
    }

    if (!this.data.percentage) {
      this.notify('O peso da questão deve ser informado');
      return false;
    } else if (this.data.percentage < 1 || this.data.percentage > 100) {
      this.notify('O peso da questão deve ser maior que 0 e menor ou igual a 100');
      return false;
    }

    return true;
  }

}
