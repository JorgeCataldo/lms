import { Component, Input, EventEmitter, Output } from '@angular/core';
import { Location as NgLocation } from '@angular/common';
import { Exam } from '../../../models/exam.interface';

@Component({
  selector: 'app-exam-start',
  templateUrl: './exam-start.component.html',
  styleUrls: ['./exam-start.component.scss']
})
export class ExamStartComponent {

  @Input() exam: Exam;
  @Input() levels: any;
  @Input() userProgress: any;
  @Output() goToNextQuestion = new EventEmitter();

  constructor(
    private _location: NgLocation
  ) { }

  public startExam(): void {
    this.goToNextQuestion.emit();
  }

  public goBack(): void {
    this._location.back();
  }

  public getBadgesProgressImageSrc(concludedSteps: number): string {
    switch (concludedSteps) {
      case 0:
        return './assets/img/progress-start.png';
      case 1:
        return './assets/img/progress-beginner.png';
      case 2:
        return './assets/img/progress-intermediary.png';
      case 3:
        return './assets/img/progress-expert.png';
      case 4:
        return './assets/img/progress-expert-final.png';
      default:
        return '';
    }
  }

}
