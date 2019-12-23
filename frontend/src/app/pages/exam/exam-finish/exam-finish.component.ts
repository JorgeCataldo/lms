import { Component, Input, EventEmitter, Output } from '@angular/core';

@Component({
  selector: 'app-exam-finish',
  templateUrl: './exam-finish.component.html',
  styleUrls: ['./exam-finish.component.scss']
})
export class ExamFinishComponent {

  @Input() readonly achievedLevel: number;
  @Input() readonly levels: any;
  @Output() continue = new EventEmitter();
  @Output() finish = new EventEmitter();

  public getBadgesProgressImageSrc(concludedSteps: number): string {
    switch (concludedSteps) {
      case 0:
        return './assets/img/progress-beginner.png';
      case 1:
        return './assets/img/progress-intermediary.png';
      case 2:
        return './assets/img/progress-expert.png';
      case 3:
      case 4:
        return './assets/img/progress-expert-final.png';
      default:
        return '';
    }
  }

}
