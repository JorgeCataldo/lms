import { Component, OnInit } from '@angular/core';
import { NotificationClass } from '../../shared/classes/notification';
import { MatSnackBar, MatDialog } from '@angular/material';
import { ActivatedRoute, Router } from '@angular/router';
import { ContentEventForumService } from '../_services/event-forum.service';
import { EventForum, EventForumQuestion } from 'src/app/models/event-forum.model';
import { ConfirmDialogComponent } from 'src/app/shared/dialogs/confirm/confirm.dialog';

@Component({
  selector: 'app-event-forum',
  templateUrl: './event-forum.component.html',
  styleUrls: ['./event-forum.component.scss']
})
export class EventForumComponent extends NotificationClass implements OnInit {

  public forum: EventForum;
  public itemsCount: number = 0;
  public searchValue: string = '';

  private _eventScheduleId: string;
  private _currentPage: number = 1;

  constructor(
    protected _snackBar: MatSnackBar,
    private _activatedRoute: ActivatedRoute,
    private _router: Router,
    private _forumService: ContentEventForumService,
    private _dialog: MatDialog
  ) {
    super(_snackBar);
  }

  ngOnInit() {
    this._eventScheduleId = this._activatedRoute.snapshot.paramMap.get('eventScheduleId');
    this._loadQuestions( this._eventScheduleId, this._currentPage );
  }

  public triggerSearch(searchValue: string) {
    this.searchValue = searchValue;
    this._loadQuestions(
      this._eventScheduleId,
      this._currentPage,
      this.searchValue
    );
  }

  public goToQuestion(question: EventForumQuestion): void {
    this._router.navigate([
      '/forum-evento/' + this.forum.eventId + '/' + this._eventScheduleId + '/' + question.id
    ]);
  }

  public goToPage(page: number) {
    if (page !== this._currentPage) {
      this._currentPage = page;
      this._loadQuestions(
        this._eventScheduleId,
        this._currentPage,
        this.searchValue
      );
    }
  }

  public manageLike(question: EventForumQuestion): void {
    this._forumService.manageEventForumQuestionLike(
      question.id, question.liked
    ).subscribe(
      () => { this._loadQuestions( this._eventScheduleId, this._currentPage, this.searchValue ); },
      () => { this.notify('Ocorreu um erro, por favor tente novamente mais tarde'); }
    );
  }

  public confirmRemoveQuestion(question: EventForumQuestion): void {
    const dialogRef = this._dialog.open(ConfirmDialogComponent, {
      width: '400px',
      data: { message: 'Tem certeza que deseja remover esta pergunta?' }
    });

    dialogRef.afterClosed().subscribe((result: boolean) => {
      if (result)
        this._removeQuestion(question);
    });
  }

  private _removeQuestion(question: EventForumQuestion): void {
    this._forumService.removeEventForumQuestion(
      question.id, this._eventScheduleId
    ).subscribe(() => {
      this._currentPage = 1;
      this._loadQuestions(
        this._eventScheduleId,
        this._currentPage
      );
      this.notify('Pergunta removida com sucesso');

    }, (error) => {
      this.notify(
        this.getErrorNotification(error)
      );
    });
  }

  private _loadQuestions(eventScheduleId: string, page: number, searchValue: string = ''): void {
    this._forumService.getEventForumByEventSchedule(
      eventScheduleId, page, 10, searchValue
    ).subscribe((response) => {

      this.forum = response.data;
      this.itemsCount = response.data.itemsCount;
    }, () => {
      this.notify('Ocorreu um erro, por favor tente novamente mais tarde');
    });
  }
}
