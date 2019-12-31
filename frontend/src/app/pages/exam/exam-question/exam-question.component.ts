import { Component, Input, Output, EventEmitter, OnInit } from '@angular/core';
import { Question, Answer } from '../../../models/question.model';
import { Subject } from 'src/app/models/subject.model';

@Component({
  selector: 'app-exam-question',
  templateUrl: './exam-question.component.html',
  styleUrls: ['./exam-question.component.scss']
})
export class ExamQuestionComponent implements OnInit {

  @Input() readonly subject: Subject;
  @Input() readonly question: Question;
  @Input() readonly questionNumber: number;
  @Input() readonly last: boolean;
  @Input() readonly reviewingConcept: any;
  @Input() readonly levels: any;
  @Input() set evalAnswer(answer: Answer) {
    this.answered = answer ? true : false;
    this.selectedAnswer = answer;
  }

  @Output() confirmAnswer = new EventEmitter();
  @Output() goToNextQuestion = new EventEmitter();
  @Output() openReview = new EventEmitter<any>();

  public selectedAnswer: Answer;
  public answered: boolean = false;
  public selectedConcept: any;

  ngOnInit() {
    if (this.question != null && this.question.answers != null && this.question.answers.length > 0) {
      this.question.answers = this.shuffleArray(this.question.answers);
    }
  }

  private shuffleArray(array: any[]): any[] {
    let currentIndex = array.length, temporaryValue, randomIndex;

    while (0 !== currentIndex) {
      randomIndex = Math.floor(Math.random() * currentIndex);
      currentIndex -= 1;

      temporaryValue = array[currentIndex];
      array[currentIndex] = array[randomIndex];
      array[randomIndex] = temporaryValue;
    }
    return array;
  }

  public setAnswer(answer: Answer) {
    this.selectedAnswer = answer;
  }

  public confirm(): void {
    if (this.answered) {
      this.goToNextQuestion.emit();
      this.selectedAnswer = null;
    } else {
      this.confirmAnswer.emit(this.selectedAnswer);
    }
  }

  public resetAnswer(): void {
    this.answered = false;
    this.selectedAnswer = null;
  }

  public reviewConcept(concept): void {
    if (typeof concept === 'object') {
      concept = concept.concept;
    }
    this.openReview.emit(concept);
  }
}
