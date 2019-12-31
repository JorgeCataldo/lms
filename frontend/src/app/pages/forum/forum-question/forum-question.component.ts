import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ContentForumService } from '../../_services/forum.service';
import { ForumQuestion, ForumAnswer } from 'src/app/models/forum.model';
import { NotificationClass } from 'src/app/shared/classes/notification';
import { MatSnackBar, MatDialog } from '@angular/material';
import * as Editor from 'tui-editor';
import { ConfirmDialogComponent } from 'src/app/shared/dialogs/confirm/confirm.dialog';

@Component({
  selector: 'app-forum-question',
  templateUrl: './forum-question.component.html',
  styleUrls: [ './forum-question.component.scss' ]
})
export class ForumQuestionComponent extends NotificationClass implements OnInit {

  public question: ForumQuestion;
  public moduleName: string = '';
  public itemsCount: number = 0;
  public editor: Editor;

  private _moduleId: string;
  private _questionId: string;
  private _currentPage: number = 1;

  constructor(
    protected _snackBar: MatSnackBar,
    private _router: Router,
    private _activatedRoute: ActivatedRoute,
    private _forumService: ContentForumService,
    private _dialog: MatDialog
  ) {
    super(_snackBar);
  }

  ngOnInit() {
    this._questionId = this._activatedRoute.snapshot.paramMap.get('questionId');
    this._moduleId = this._activatedRoute.snapshot.paramMap.get('moduleId');
    this.moduleName = this._activatedRoute.snapshot.paramMap.get('moduleName');
    this._loadAnswers( this._questionId );
    this._configureEditor();
    localStorage.removeItem('emailUrl');
  }

  public goToModule(): void {
    this._router.navigate([ 'modulo/' + this._moduleId ]);
  }

  public goToContent(): void {
    this._router.navigate([ 'modulo/' + this._moduleId + '/' + this.question.subjectId + '/0' ]);
  }

  public getUserImg(question: ForumQuestion): string {
    return question && question.userImgUrl && question.userImgUrl !== '' ?
      question.userImgUrl : './assets/img/user-image-placeholder.png';
  }

  public saveAnswer(): void {
    const answer = this.editor.getMarkdown();
    if (answer && answer.trim() !== '') {

      this._forumService.saveForumQuestionAnswer({
        'questionId': this._questionId,
        'text': answer
      }).subscribe(() => {

        this.editor.setMarkdown('');
        this._loadAnswers( this._questionId );
      }, () => this.notify('Ocorreu um erro, por favor tente novamente mais tarde'));
    }
  }

  public manageAnswerLike(answer: ForumAnswer): void {
    this._forumService.manageAnswerLike(
      answer.id, answer.liked
    ).subscribe(() => {

      this._loadAnswers( this._questionId );
    }, () => this.notify('Ocorreu um erro, por favor tente novamente mais tarde'));
  }

  public goToPage(page: number): void {
    if (page !== this._currentPage) {
      this._currentPage = page;
      this._loadAnswers( this._questionId );
    }
  }

  public confirmRemoveAnswer(answer: ForumAnswer): void {
    const dialogRef = this._dialog.open(ConfirmDialogComponent, {
      width: '400px',
      data: { message: 'Tem certeza que deseja remover esta resposta?' }
    });

    dialogRef.afterClosed().subscribe((result: boolean) => {
      if (result)
        this._removeAnswer(answer);
    });
  }

  private _removeAnswer(answer: ForumAnswer): void {
    this._forumService.removeForumAnswer(
      answer.id, this._moduleId
    ).subscribe(() => {
      this._currentPage = 1;
      this._loadAnswers( this._questionId );
      this.notify('Resposta removida com sucesso');

    }, (error) => {
      this.notify(
        this.getErrorNotification(error)
      );
    });
  }

  private _loadAnswers(questionId: string): void {
    this._forumService.getForumQuestionById(
      questionId, this._moduleId, this._currentPage, 5
    ).subscribe((response) => {

      this.question = response.data;
      this.itemsCount = response.data.itemsCount;
    }, () => {
      this.notify('Ocorreu um erro, por favor tente novamente mais tarde');
    });
  }

  private _configureEditor(): void {
    this.editor = new Editor({
      el: document.querySelector('#htmlEditor'),
      initialEditType: 'markdown',
      previewStyle: 'wysiwyg',
      height: '150px',
      hideModeSwitch: true
  });
  }

}
