import { Component, OnInit, Output, EventEmitter, Input } from '@angular/core';
import { MatDialog } from '@angular/material';
import { Event } from '../../../../../models/event.model';
import { ConfirmDialogComponent } from '../../../../../shared/dialogs/confirm/confirm.dialog';
import { PrepQuizQuestions } from 'src/app/prepQuizQuestions.interface';

@Component({
  selector: 'app-new-event-questions',
  templateUrl: './questions.component.html',
  styleUrls: ['../new-event-steps.scss', './questions.component.scss']
})
export class NewEventQuestionsComponent implements OnInit {

  @Input() readonly event: Event;
  @Output() addEventQuestions = new EventEmitter<Array<PrepQuizQuestions>>();

  public questions: Array<PrepQuizQuestions> = [];

  constructor(
    private _dialog: MatDialog
  ) { }

  ngOnInit() {
    if (this.event && this.event.prepQuizQuestionList)
      this.questions = this.event.prepQuizQuestionList;
  }

  public addQuestion(): void {
    this.questions.push(new PrepQuizQuestions());
  }

  public confirmRemoveQuestion(index: number): void {
    const dialogRef = this._dialog.open(ConfirmDialogComponent, {
      width: '400px',
      data: { message: 'Tem certeza que deseja remover esta pergunta?' }
    });

    dialogRef.afterClosed().subscribe((result: boolean) => {
      if (result)
        this._removeQuestion(index);
    });
  }

  public trackByFunc(index: any) {
    return index;
 }

  public nextStep(): void {
    this.addEventQuestions.emit( this.questions );
  }

  private _removeQuestion(index: number): void {
    this.questions.splice(index, 1);
  }

}
