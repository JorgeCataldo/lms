import { Component, OnInit } from '@angular/core';
import { NotificationClass } from '../../shared/classes/notification';
import { MatSnackBar, MatDialog } from '@angular/material';
import { ActivatedRoute, Router } from '@angular/router';
import { ContentForumService } from '../_services/forum.service';
import { Forum, ForumQuestion } from 'src/app/models/forum.model';
import { SubjectPreview } from 'src/app/models/previews/subject.interface';
import { ContentPreview } from 'src/app/models/previews/content.interface';
import { ConfirmDialogComponent } from 'src/app/shared/dialogs/confirm/confirm.dialog';

@Component({
  selector: 'app-forum',
  templateUrl: './forum.component.html',
  styleUrls: ['./forum.component.scss']
})
export class ForumComponent extends NotificationClass implements OnInit {

  public forum: Forum;
  public itemsCount: number = 0;
  public selectedSubject: SubjectPreview;
  public selectedContent: ContentPreview;
  public searchValue: string = '';

  private _moduleId: string;
  private _currentPage: number = 1;

  constructor(
    protected _snackBar: MatSnackBar,
    private _activatedRoute: ActivatedRoute,
    private _router: Router,
    private _forumService: ContentForumService,
    private _dialog: MatDialog
  ) {
    super(_snackBar);
  }

  ngOnInit() {
    this._moduleId = this._activatedRoute.snapshot.paramMap.get('moduleId');
    this._loadQuestions( this._moduleId, this._currentPage );
  }

  public triggerSearch(searchValue: string) {
    this.searchValue = searchValue;
    this._loadQuestions(
      this._moduleId,
      this._currentPage,
      this.searchValue
    );
  }

  public goToQuestion(question: ForumQuestion): void {
    this._router.navigate([
      '/forum/' + this.forum.moduleName + '/' + this._moduleId + '/' + question.id
    ]);
  }

  public goToPage(page: number) {
    if (page !== this._currentPage) {
      this._currentPage = page;
      this._loadQuestions(
        this._moduleId,
        this._currentPage,
        this.searchValue
      );
    }
  }

  public manageLike(question: ForumQuestion): void {
    this._forumService.manageQuestionLike(
      question.id, question.liked
    ).subscribe(
      () => { this._loadQuestions( this._moduleId, this._currentPage, this.searchValue ); },
      () => { this.notify('Ocorreu um erro, por favor tente novamente mais tarde'); }
    );
  }

  public setSubjectFilter(subject: SubjectPreview): void {
    this.selectedSubject = subject;
    this._loadQuestions(
      this._moduleId,
      this._currentPage,
      this.searchValue
    );
  }

  public setContentFilter(content: ContentPreview): void {
    this.selectedContent = content;
    this._loadQuestions(
      this._moduleId,
      this._currentPage,
      this.searchValue
    );
  }

  public confirmRemoveQuestion(question: ForumQuestion): void {
    const dialogRef = this._dialog.open(ConfirmDialogComponent, {
      width: '400px',
      data: { message: 'Tem certeza que deseja remover esta pergunta?' }
    });

    dialogRef.afterClosed().subscribe((result: boolean) => {
      if (result)
        this._removeQuestion(question);
    });
  }

  private _removeQuestion(question: ForumQuestion): void {
    this._forumService.removeForumQuestion(
      question.id, this._moduleId
    ).subscribe(() => {
      this._currentPage = 1;
      this._loadQuestions(
        this._moduleId,
        this._currentPage
      );
      this.notify('Pergunta removida com sucesso');

    }, (error) => {
      this.notify(
        this.getErrorNotification(error)
      );
    });
  }

  private _loadQuestions(moduleId: string, page: number, searchValue: string = ''): void {
    this._forumService.getForumByModule(
      moduleId, page, 10, searchValue,
      this.selectedSubject ? this.selectedSubject.id : null,
      this.selectedContent ? this.selectedContent.id : null
    ).subscribe((response) => {

      this.forum = response.data;
      this.itemsCount = response.data.itemsCount;
    }, () => {
      this.notify('Ocorreu um erro, por favor tente novamente mais tarde');
    });
  }
}
