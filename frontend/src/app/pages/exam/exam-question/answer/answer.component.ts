import { Component, Input, Output, EventEmitter } from '@angular/core';
import { Answer } from '../../../../models/question.model';

@Component({
  selector: 'app-exam-answer',
  templateUrl: './answer.component.html',
  styleUrls: ['./answer.component.scss']
})
export class ExamAnswerComponent {

  @Input() readonly answer: Answer;
  @Input() readonly selected: boolean = false;
  @Input() readonly confirmed: boolean = false;
  @Output() selectAnswer = new EventEmitter<Answer>();

  public select(): void {
    if (!this.confirmed)
      this.selectAnswer.emit( this.answer );
  }
}
