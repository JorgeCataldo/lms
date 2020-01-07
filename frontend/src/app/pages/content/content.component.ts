import { Component, OnInit, ViewChild, OnDestroy } from '@angular/core';
import { Content } from '../../models/content.model';
import { ContentTypeEnum } from '../../models/enums/content-type.enum';
import { ActivatedRoute, Router } from '@angular/router';
import { PDFContentComponent } from './pdf/pdf.component';
import { ContentDescriptionComponent } from './common/description/description.component';
import { AnalyticsService } from 'src/app/shared/services/analytics.service';
import { ActionInfoPageEnum, ActionInfoTypeEnum, ActionInfo } from 'src/app/shared/directives/save-action/action-info.interface';
import { MatDialog, MatSnackBar } from '@angular/material';
import { ForumQuestionDialogComponent } from 'src/app/shared/dialogs/forum-question/forum-question.dialog';
import { SharedService } from 'src/app/shared/services/shared.service';
import { Level } from 'src/app/models/shared/level.interface';
import { SettingsUsersService } from 'src/app/settings/_services/users.service';
import { NotificationClass } from 'src/app/shared/classes/notification';
import { ContentModulesService } from '../_services/modules.service';

@Component({
  selector: 'app-content',
  templateUrl: './content.component.html',
  styleUrls: ['./content.component.scss']
})
export class ContentComponent extends NotificationClass implements OnInit, OnDestroy {

  @ViewChild('pdfContent') pdfContent: PDFContentComponent;
  @ViewChild('contentDescription') contentDescription: ContentDescriptionComponent;

  public contents: Array<Content>;
  public allContents: any = '';
  public index: number = 0;
  public contentTypeEnum = ContentTypeEnum;
  public showMenu: boolean = true;
  public conceptPosition: number = 0;
  public moduleId: string = '';
  public subjectId: string = '';
  public subjectProgress: any;
  public levels: Array<Level> = [];
  public hasQuestions: boolean = false;
  public hasFinishedRequirements: boolean = false;
  public userContentNote: string = '';

  private _actionId: string;

  constructor(
    protected _snackBar: MatSnackBar,
    private _activatedRoute: ActivatedRoute,
    private _router: Router,
    private _analyticsService: AnalyticsService,
    private _dialog: MatDialog,
    private _sharedService: SharedService,
    private _settingsUsersService: SettingsUsersService
  ) {
    super(_snackBar);
  }

  ngOnInit() {
    this._loadLevels();

    this.index = parseInt(
      this._activatedRoute.snapshot.paramMap.get('contentIndex'), 10
    );
    this.moduleId = this._activatedRoute.snapshot.paramMap.get('moduleId');
    this.subjectId = this._activatedRoute.snapshot.paramMap.get('subjectId');

    const contentStr = localStorage.getItem('contents');
    const allContentStr = localStorage.getItem('allContents');
    if (contentStr && contentStr.trim() !== '') {
      this.contents = JSON.parse(contentStr);
      this.allContents = JSON.parse(allContentStr);
      this.hasQuestions = JSON.parse(localStorage.getItem('contents-hasQuestions'));
      this._saveAccessAction( this.moduleId, this.subjectId, this.contents[this.index] );
    } else {

      // this._router.navigate([ 'home' ]);
      return;
    }

    const progressStr = localStorage.getItem('subjectProgress');
    if (progressStr && progressStr.trim() !== '')
      this.subjectProgress = JSON.parse(progressStr);

    const finishedReqStr = localStorage.getItem('hasFinishedRequirements');
    if (finishedReqStr && finishedReqStr.trim() !== '')
      this.hasFinishedRequirements = JSON.parse(finishedReqStr);

    this.loadUserContentNote();
  }

  ngOnDestroy() {
    localStorage.removeItem('contents');
    localStorage.removeItem('contents-hasQuestions');
  }

  public goBackToModule(indexOffset: number = 0): void {
    this._setContentFinishAction(this.moduleId, this.subjectId, this.contents[this.index + indexOffset] );
    const moduleId = this._activatedRoute.snapshot.paramMap.get('moduleId');
    this._router.navigate(['/modulo/' + moduleId]);
  }

  public toggleMenu(): void {
    this.showMenu = !this.showMenu;
  }

  public goToPrevious(): void {
    this._setContentFinishAction(this.moduleId, this.subjectId, this.contents[this.index] );
    this.index--;
    window.scroll({ top: 0, behavior: 'smooth' });
    this.contentDescription.showContent = true;
    setTimeout(() => { if (this.contentDescription) this.contentDescription.setDescriptionContentHeight(); });
    this._saveAccessAction(this.moduleId, this.subjectId, this.contents[this.index] );
  }

  public goToNext(): void {
    this.index++;
    if (this.index >= this.contents.length)
      this.goBackToModule(-1);
    else {
      this._setContentFinishAction( this.moduleId, this.subjectId, this.contents[this.index - 1] );
      this.contentDescription.showContent = true;
      window.scroll({ top: 0, behavior: 'smooth' });
      setTimeout(() => { if (this.contentDescription) this.contentDescription.setDescriptionContentHeight(); });
      this._saveAccessAction(this.moduleId, this.subjectId, this.contents[this.index] );
    }
  }

  public goToContent(data: any): void {

    if(!this.contents[data.index] || this.contents[data.index] === undefined){
      console.log('this.contents[data.index] -> ', this.contents[data.index]);
      console.log('[data] -> ', data);
    }
    const contentStr = localStorage.getItem('contents');
    const allContentStr = localStorage.getItem('allContents');

    if (allContentStr && allContentStr.trim() !== '') {
      this.contents = JSON.parse(contentStr);
      this.allContents = JSON.parse(allContentStr);

      this.moduleId = data.moduleId;
      this.subjectId = data.subjectId;

      if (this.allContents.modules) {
        for (let index = 0; index < this.allContents.modules.length; index++) {
          const element = this.allContents.modules[index];
          if (element.id === this.moduleId) {
            element.selected = true;

            for (let sIndex = 0; sIndex < this.allContents.modules[index].subjects.length; sIndex++) {
              const sub = this.allContents.modules[index].subjects[sIndex];
              if (sub.id === this.subjectId) {
                sub.selected = true;
              } else {
                sub.selected = false;
              }
            }
          } else {
            element.selected = false;
          }
        }

        const currentModule = this.allContents.modules.find(x => x.id === data.moduleId);
        const currentSubject = currentModule.subjects.find(x => x.id === data.subjectId);
        this.contents = currentSubject.contents;
      } else {
        for (let sIndex = 0; sIndex < this.allContents.subjects.length; sIndex++) {
          const sub = this.allContents.subjects[sIndex];
          if (sub.id === this.subjectId) {
            sub.selected = true;
            const currentSubject = sub;
            this.contents = currentSubject.contents;
          } else {
            sub.selected = false;
          }
        }
      }

    } else {
      if (contentStr && contentStr.trim() !== '') {
        this.contents = JSON.parse(contentStr);
      }
    }


    this._setContentFinishAction(this.moduleId, this.subjectId, this.contents[data.index] );

    this.index = data.index;
    this.contentDescription.showContent = true;
    setTimeout(() => { if (this.contentDescription) this.contentDescription.setDescriptionContentHeight(); });
    this._saveAccessAction(data.moduleId, data.subjectId, this.contents[this.index] );
    this._router.navigate(['/modulo/', data.moduleId, data.subjectId, this.index]);
  }

  public goToPosition(position: number): void {
    if (this.pdfContent) {
      this.pdfContent.goToPage(position);
    }
    this.conceptPosition = position;
  }

  public goToAnchor(anchor: string): void {
    const anchorElement: any = document.querySelector('a[href=\'' + anchor + '\']');
    document.querySelector('.htmlContent').scrollTop = anchorElement.offsetTop - 50;
  }

  public saveVideoPlayedAction(moduleId: string, subjectId: string, contentId: string): void {
    const actionInfo = this._getActionInfo('content-video', moduleId, subjectId, contentId, ActionInfoTypeEnum.VideoPlay);
    this._analyticsService.saveAction(actionInfo).subscribe();
  }

  public savePdfFinishedAction(moduleId: string, subjectId: string, contentId: string): void {
    const actionInfo = this._getActionInfo('content-pdf', moduleId, subjectId, contentId, ActionInfoTypeEnum.Finish);
    this._analyticsService.saveAction(actionInfo).subscribe();
  }

  public saveTextFinishedAction(moduleId: string, subjectId: string, contentId: string): void {
    const actionInfo = this._getActionInfo('content-text', moduleId, subjectId, contentId, ActionInfoTypeEnum.Finish);
    this._analyticsService.saveAction(actionInfo).subscribe();
  }

  public saveVideoFinishedAction(moduleId: string, subjectId: string, contentId: string): void {
    const actionInfo = this._getActionInfo('content-video', moduleId, subjectId, contentId, ActionInfoTypeEnum.Finish);
    this._analyticsService.saveAction(actionInfo).subscribe();
  }

  public saveConceptViewAction(moduleId: string, subjectId: string, concept: string): void {
    const actionInfo = this._getActionInfo(
      'content-view', moduleId, subjectId, this.contents[ this.index ].id, ActionInfoTypeEnum.Click
    );
    actionInfo.concept = concept;
    this._analyticsService.saveAction(actionInfo).subscribe();
  }

  public reachedMaxLevel(): boolean {
    return this.levels && this.subjectProgress && this.levels[this.subjectProgress.level] === undefined;
  }

  private _saveAccessAction(moduleId: string, subjectId: string, content: Content): void {
    if (content && !content.accessed) {
      content.accessed = true;

      const actionInfo = this._getActionInfo('content-access', moduleId, subjectId, content.id, ActionInfoTypeEnum.Access);
      this._analyticsService.saveAction(actionInfo).subscribe(() =>  {

          const waitingAction = this._getActionInfo('content-leave', moduleId, subjectId, content.id, ActionInfoTypeEnum.CloseTab);
          this._actionId = this._analyticsService.setWaitingAction(waitingAction);
        },
        (error) => { console.error(error); }
      );
    }
  }

  private _setContentFinishAction(moduleId: string, subjectId: string, content: Content): void {
    const actionInfo = this._getActionInfo('content-finish', moduleId, subjectId, content.id, ActionInfoTypeEnum.Finish);
    this._analyticsService.saveAction(actionInfo).subscribe(() => {
        this._analyticsService.clearWaitingAction( this._actionId );
        this._actionId = null;
      },
      (error) => { console.error(error); }
    );
  }

  private _getActionInfo(description: string, moduleId: string, subjectId: string,
    contentId: string, type: ActionInfoTypeEnum): ActionInfo {
    return {
      'page': ActionInfoPageEnum.Content,
      'description': description,
      'type': type,
      'moduleId': moduleId,
      'subjectId': subjectId,
      'contentId': contentId
    };
  }

  public openForumQuestionModal() {
    this._sharedService.forumQuestionResponse.subscribe(data => {
      const localForumQuestion = JSON.parse(localStorage.getItem('forumQuestionDialog'));
      localForumQuestion.position = data;
      localStorage.setItem('forumQuestionDialog', JSON.stringify(localForumQuestion));
      this._dialog.open(ForumQuestionDialogComponent, {
        width: '1000px'
      });
    });
    this._sharedService.forumQuestion.next();
  }

  public loadUserContentNote() {
    if (this.contents && this.contents.length > 0 && this.contents[this.index]) {
      const contentId = this.contents[this.index].id;
      this._settingsUsersService.getUserContentNote(this.moduleId, this.subjectId, contentId).subscribe(res => {
        this.userContentNote = res.data.note;
      });
    }
  }

  public updateUserContentNote(note: string) {
    const contentId = this.contents[this.index].id;
    this._settingsUsersService.updateUserContentNote(this.moduleId, this.subjectId,
      contentId, note).subscribe(() => {
        this.userContentNote = note;
        this.notify('Anotação salva com sucesso');
      }, err => {
        this.notify(this.getErrorNotification(err));
      });
  }

  private _loadLevels(): void {
    this._sharedService.getLevels(true).subscribe((response) => {
      this.levels = response.data;
    }, () => { });
  }
}
