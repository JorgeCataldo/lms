import { Component, Input, Output, EventEmitter } from '@angular/core';
import { EventForumQuestion } from 'src/app/models/event-forum.model';

@Component({
  selector: 'app-event-forum-last-question-card',
  template: `
    <div class="forum-last-question-card" >
      <div class="question" >
        <p class="description"
          [innerHTML]="question.description | MarkdownToHtml"
        ></p>
      </div>
      <div class="footer" >
        <div class="answers" >
          <img src="./assets/img/comments.png" />
          {{ question.answers ? question.answers.length : '0' }} respostas
        </div>
        <p>
          ver <img src="./assets/img/chevron-right-black.png" />
        </p>
      </div>
    </div>`,
  styleUrls: [ './event-forum-last-question-card.component.scss' ]
})
export class EventForumLastQuestionCardComponent {

  @Input() question: EventForumQuestion;

}
