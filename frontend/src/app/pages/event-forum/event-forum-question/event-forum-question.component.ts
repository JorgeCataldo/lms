import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ContentEventForumService } from '../../_services/event-forum.service';
import { EventForumQuestion, EventForumAnswer } from 'src/app/models/event-forum.model';
import { NotificationClass } from 'src/app/shared/classes/notification';
import { MatSnackBar, MatDialog } from '@angular/material';
import * as Editor from 'tui-editor';
import { ConfirmDialogComponent } from 'src/app/shared/dialogs/confirm/confirm.dialog';

@Component({
  selector: 'app-event-forum-question',
  templateUrl: './event-forum-question.component.html',
  styleUrls: [ './event-forum-question.component.scss' ]
})
export class EventForumQuestionComponent extends NotificationClass implements OnInit {

  public question: EventForumQuestion;
  public itemsCount: number = 0;
  public editor: Editor;

  private _eventId: string;
  private _eventScheduleId: string;
  private _questionId: string;
  private _currentPage: number = 1;

  constructor(
    protected _snackBar: MatSnackBar,
    private _router: Router,
    private _activatedRoute: ActivatedRoute,
    private _forumService: ContentEventForumService,
    private _dialog: MatDialog
  ) {
    super(_snackBar);
  }

  ngOnInit() {
    this._questionId = this._activatedRoute.snapshot.paramMap.get('questionId');
    this._eventId = this._activatedRoute.snapshot.paramMap.get('eventId');
    this._eventScheduleId = this._activatedRoute.snapshot.paramMap.get('eventScheduleId');
    this._loadAnswers( this._questionId );
    this._configureEditor();
    localStorage.removeItem('emailUrl');
  }

  public goToEvent(): void {
    this._router.navigate([ 'evento/' + this._eventId + '/' + this._eventScheduleId ]);
  }

  public saveAnswer(): void {
    const answer = this.editor.getMarkdown();
    if (answer && answer.trim() !== '') {

      this._forumService.saveEventForumQuestionAnswer({
        'questionId': this._questionId,
        'text': answer
      }).subscribe(() => {

        this.editor.setMarkdown('');
        this._loadAnswers( this._questionId );
      }, () => this.notify('Ocorreu um erro, por favor tente novamente mais tarde'));
    }
  }

  public manageAnswerLike(answer: EventForumAnswer): void {
    this._forumService.manageEventForumAnswerLike(
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

  public confirmRemoveAnswer(answer: EventForumAnswer): void {
    const dialogRef = this._dialog.open(ConfirmDialogComponent, {
      width: '400px',
      data: { message: 'Tem certeza que deseja remover esta resposta?' }
    });
    dialogRef.afterClosed().subscribe((result: boolean) => {
      if (result)
        this._removeAnswer(answer);
    });
  }

  private _removeAnswer(answer: EventForumAnswer): void {
    this._forumService.removeForumAnswer(
      answer.id, this._eventScheduleId
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
    this._forumService.getEventForumQuestionById(
      questionId, this._eventScheduleId, this._currentPage, 5
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
