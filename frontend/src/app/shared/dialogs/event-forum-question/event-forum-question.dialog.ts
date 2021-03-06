import { Component, OnInit } from '@angular/core';
import { MatDialogRef, MatSnackBar } from '@angular/material';
import * as Editor from 'tui-editor';
import { NotificationClass } from '../../classes/notification';
import { Router } from '@angular/router';
import { EventForumQuestion } from 'src/app/models/event-forum.model';
import { UserService } from 'src/app/pages/_services/user.service';
import { ContentEventForumService } from 'src/app/pages/_services/event-forum.service';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-event-forum-question-dialog',
  templateUrl: './event-forum-question.dialog.html',
  styleUrls: ['./event-forum-question.dialog.scss']
})
export class EventForumQuestionDialogComponent extends NotificationClass implements OnInit {

  public forumQuestionsPreview: EventForumQuestion[] = [];
  public editor: Editor;
  public newQuestion: any;

  constructor(
    protected _snackBar: MatSnackBar,
    public dialogRef: MatDialogRef<EventForumQuestionDialogComponent>,
    private _forumService: ContentEventForumService,
    private _authService: AuthService,
    private _userService: UserService,
    private _router: Router
  ) {
    super(_snackBar);
    this.newQuestion = JSON.parse(localStorage.getItem('eventForumQuestionDialog'));
    this.newQuestion.subjectName = this.newQuestion.subjectName === '-' ? null : this.newQuestion.subjectName;
    this.newQuestion.contentName = this.newQuestion.contentName === '-' ? null : this.newQuestion.contentName;
    this.newQuestion.position = this.newQuestion.position === '-' ? null : this.newQuestion.position;
    this._loadEventForumPreview(this.newQuestion.eventScheduleId);
  }

  ngOnInit() {
    this._configureEditor();
  }

  private _loadEventForumPreview(eventScheduleId: string): void {
    this._userService.getEventForumPreview(eventScheduleId, 3).subscribe((response) => {
      this.forumQuestionsPreview = response.data;
    }, () => this.notify('Ocorreu um erro, por favor tente novamente mais tarde'));
  }

  public dismiss(): void {
    this.dialogRef.close();
  }

  private _configureEditor(): void {
    this.editor = new Editor({
      el: document.querySelector('#htmlEditor'),
      initialEditType: 'wysiwyg',
      hideModeSwitch: true,
      previewStyle: 'vertical',
      height: '200px'
    });

    this.editor.setMarkdown(
      this.newQuestion.description
    );
  }

  public manageLike(question: EventForumQuestion): void {
    this._forumService.manageEventForumQuestionLike(
      question.id, question.liked
    ).subscribe(
      () => {
        const user = this._authService.getLoggedUser();
        question.liked ? question.likedBy.push(user.user_id) : question.likedBy.pop();
      },
      (err) => { this.notify(this.getErrorNotification(err)); }
    );
  }

  public goToForum() {
    this.dialogRef.close();
    this._router.navigate([ '/forum-evento/' + this.newQuestion.eventId + '/' + this.newQuestion.eventScheduleId ]);
  }

  public sendNewQuestion() {
    this.newQuestion.description = this.editor.getMarkdown();
    if (this.newQuestion.description && this.newQuestion.title) {
      this._forumService.saveEventForumQuestion(this.newQuestion
      ).subscribe(
        () => {
          this.dialogRef.close(true);
          this.notify('Pergunta salva com sucesso');
        },
        (err) => { this.notify(this.getErrorNotification(err)); }
      );
    } else {
      this.notify('Preencha o título e a pergunta');
    }
  }

}
