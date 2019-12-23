import { Component, Input, EventEmitter, Output } from '@angular/core';
import { ForumQuestion, ForumAnswer } from 'src/app/models/forum.model';
import { AuthService } from 'src/app/shared/services/auth.service';

@Component({
  selector: 'app-forum-answer-card',
  template: `
    <div class="forum-answer-card" >
      <div class="answer" >
        <img class="userImg" [src]="getUserImg()" />
        <div>
          <p class="name" >
            <span>
              {{ answer.userName }} <small>{{ answer.createdAt | date : 'dd/MM/yyyy' }}</small>
            </span>
            <img *ngIf="canRemoveAnswer()"
              (click)="removeAnswer.emit(answer)"
              src="./assets/img/trash.png"
            />
          </p>
          <p class="text"
            [innerHTML]="answer.text | MarkdownToHtml"
          ></p>
          <p class="like" >
            <img
              [src]="getAnswerLikeIcon()"
              (click)="manageLike()"
            />
            <span>Curtir</span>
            {{ answer.likedBy.length }} {{ answer.likedBy.length === 1 ? 'like' : 'likes' }}
          </p>
        </div>
      </div>
    </div>`,
  styleUrls: [ './answer-card.component.scss' ]
})
export class ForumQuestionAnswerCardComponent {

  @Input() answer: ForumAnswer;
  @Input() isInstructor: boolean;
  @Output() manageAnswerLike: EventEmitter<ForumAnswer> = new EventEmitter();
  @Output() removeAnswer: EventEmitter<ForumAnswer> = new EventEmitter();

  constructor(
    private _authService: AuthService
  ) { }

  public canRemoveAnswer(): boolean {
    if (this.isInstructor) return true;

    const loggedUser = this._authService.getLoggedUser();
    return loggedUser && loggedUser.role !== 'Student';
  }

  public manageLike(): void {
    this.answer.liked = !this.answer.liked;
    this.manageAnswerLike.emit( this.answer );
  }

  public getUserImg(): string {
    return this.answer.userImgUrl && this.answer.userImgUrl !== '' ?
      this.answer.userImgUrl :
      './assets/img/user-image-placeholder.png';
  }

  public getAnswerLikeIcon(): string {
    return this.answer.liked ?
      './assets/img/like-filled-green.png' :
      './assets/img/like-green.png';
  }

}
