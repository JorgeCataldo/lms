import { Component, Inject, OnInit } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA, MatSnackBar } from '@angular/material';
import { ProfileTestQuestion, ProfileTestQuestionOption, ProfileTestTypeEnum } from 'src/app/models/profile-test.interface';
import * as Editor from 'tui-editor';
import { NotificationClass } from 'src/app/shared/classes/notification';

@Component({
  selector: 'app-manage-profile-question-dialog',
  templateUrl: './manage-question.dialog.html',
  styleUrls: ['./manage-question.dialog.scss']
})
export class ManageProfileQuestionDialogComponent extends NotificationClass implements OnInit {

  public editor: Editor;

  constructor(
    protected _snackBar: MatSnackBar,
    public dialogRef: MatDialogRef<ManageProfileQuestionDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: ProfileTestQuestion
  ) {
    super(_snackBar);
  }

  ngOnInit() {
    this._configureEditor();
  }

  public addAnswer(): void {
    this.data.options = this.data.options || [];
    this.data.options.push( new ProfileTestQuestionOption() );
  }

  public removeAnswer(index: number): void {
    this.data.options.splice(index, 1);
  }

  public getAnswerIconSrc(answer: ProfileTestQuestionOption): string {
    return answer.correct ?
      './assets/img/right-answer-full.png' :
      './assets/img/wrong-answer-full.png';
  }

  public toggleAnswer(answer: ProfileTestQuestionOption): void {
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
    if (this.data.type === ProfileTestTypeEnum.MultipleChoice) {
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
