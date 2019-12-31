import { Component, Input, Output, EventEmitter } from '@angular/core';
import { ForumQuestion } from 'src/app/models/forum.model';

@Component({
  selector: 'app-forum-last-question-card',
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
          ver <i class="icon-seta_esquerda"></i>
        </p>
      </div>
    </div>`,
  styleUrls: [ './last-question-card.component.scss' ]
})
export class ForumLastQuestionCardComponent {

  @Input() question: ForumQuestion;

}
