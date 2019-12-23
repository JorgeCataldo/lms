import { Component, Input, Output, EventEmitter } from '@angular/core';
import { ForumQuestion } from 'src/app/models/forum.model';
import { AuthService } from 'src/app/shared/services/auth.service';

@Component({
  selector: 'app-forum-question-card',
  template: `
    <div class="forum-question-card" >
      <div class="question" >
        <p class="title" >
          <span (click)="goToQuestion.emit(question)" >
            {{ question.title }}
          </span>
          <img *ngIf="canRemoveQuestion()"
            (click)="removeQuestion.emit(question)"
            src="./assets/img/trash.png"
          />
        </p>
        <p class="description" (click)="goToQuestion.emit(question)"
          [innerHTML]="question.description | MarkdownToHtml"
        ></p>
      </div>
      <div class="footer" >
        <div class="tags" >
          <span *ngIf="question.subjectName && hasTags" >
            {{ question.subjectName }}
          </span>
          <span *ngIf="question.contentName && hasTags" >
            {{ question.contentName }}
          </span>
        </div>
        <div class="interactions" >
          <div class="likes" (click)="manageQuestionLike()">
            <img src="./assets/img/like.png" />
            {{ question.likedBy.length }} likes
          </div>
          <div class="answers" >
            <img src="./assets/img/comments.png" />
            {{ question.answers ? question.answers.length : '0' }} respostas
          </div>
        </div>
      </div>
    </div>`,
  styleUrls: [ './question-card.component.scss' ]
})
export class ForumQuestionCardComponent {

  @Input() question: ForumQuestion;
  @Input() isInstructor: boolean = false;
  @Input() hasTags: boolean = true;
  @Output() goToQuestion: EventEmitter<ForumQuestion> = new EventEmitter();
  @Output() manageLike: EventEmitter<ForumQuestion> = new EventEmitter();
  @Output() removeQuestion: EventEmitter<ForumQuestion> = new EventEmitter();

  constructor(
    private _authService: AuthService
  ) { }

  public canRemoveQuestion(): boolean {
    if (this.isInstructor) return true;

    const loggedUser = this._authService.getLoggedUser();
    return loggedUser && loggedUser.role !== 'Student';
  }

  public manageQuestionLike(): void {
    this.question.liked = !this.question.liked;
    this.manageLike.emit( this.question );
  }

}
