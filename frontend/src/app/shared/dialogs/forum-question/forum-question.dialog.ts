import { Component, OnInit } from '@angular/core';
import { MatDialogRef, MatSnackBar } from '@angular/material';
import * as Editor from 'tui-editor';
import { NotificationClass } from '../../classes/notification';
import { Router } from '@angular/router';
import { ForumQuestion } from 'src/app/models/forum.model';
import { UserService } from 'src/app/pages/_services/user.service';
import { ContentForumService } from 'src/app/pages/_services/forum.service';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-forum-question-dialog',
  templateUrl: './forum-question.dialog.html',
  styleUrls: ['./forum-question.dialog.scss']
})
export class ForumQuestionDialogComponent extends NotificationClass implements OnInit {

  public forumQuestionsPreview: ForumQuestion[] = [];
  public editor: Editor;
  public newQuestion: any;

  constructor(
    protected _snackBar: MatSnackBar,
    public dialogRef: MatDialogRef<ForumQuestionDialogComponent>,
    private _forumService: ContentForumService,
    private _authService: AuthService,
    private _userService: UserService,
    private _router: Router
  ) {
    super(_snackBar);
    this.newQuestion = JSON.parse(localStorage.getItem('forumQuestionDialog'));
    this.newQuestion.subjectName = this.newQuestion.subjectName === '-' ? null : this.newQuestion.subjectName;
    this.newQuestion.contentName = this.newQuestion.contentName === '-' ? null : this.newQuestion.contentName;
    this.newQuestion.position = this.newQuestion.position === '-' ? null : this.newQuestion.position;
    this._loadModuleForumPreview(this.newQuestion.moduleId);
  }

  ngOnInit() {
    this._configureEditor();
  }

  private _loadModuleForumPreview(moduleId: string): void {
    this._userService.getModuleForumPreview(moduleId, 3).subscribe((response) => {
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

  public manageLike(question: ForumQuestion): void {
    this._forumService.manageQuestionLike(
      question.id, question.liked
    ).subscribe(
      () => {
        const user = this._authService.getLoggedUser();
        question.liked ? question.likedBy.push(user.user_id) : question.likedBy.pop();
      },
      () => { this.notify('Ocorreu um erro, por favor tente novamente mais tarde'); }
    );
  }

  public goToForum() {
    this.dialogRef.close();
    this._router.navigate([ '/forum/' + this.newQuestion.moduleId ]);
  }

  public sendNewQuestion() {
    this.newQuestion.description = this.editor.getMarkdown();
    if (this.newQuestion.description && this.newQuestion.title) {
      this._forumService.saveForumQuestion(this.newQuestion
      ).subscribe(
        () => {
          this.dialogRef.close(true);
          this.notify('Pergunta salva com sucesso');
        },
        () => { this.notify('Ocorreu um erro, por favor tente novamente mais tarde'); }
      );
    } else {
      this.notify('Preencha o título e a pergunta');
    }
  }

}
