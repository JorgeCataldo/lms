import { Component, OnInit, ViewChild, OnDestroy } from '@angular/core';
import { Location as NgLocation } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { ExamQuestionComponent } from './exam-question/exam-question.component';
import { ContentExamService } from '../_services/exam.service';
import { SharedService } from '../../shared/services/shared.service';
import { UserService } from '../_services/user.service';
import { MatSnackBar } from '@angular/material';
import { NotificationClass } from '../../shared/classes/notification';
import { ContentModulesService } from '../_services/modules.service';
import { Module } from '../../models/module.model';
import { Subject } from '../../models/subject.model';
import { Answer, Question } from 'src/app/models/question.model';
import { Content } from 'src/app/models/content.model';
import { AnalyticsService } from 'src/app/shared/services/analytics.service';
import { ActionInfoPageEnum, ActionInfoTypeEnum, ActionInfo } from 'src/app/shared/directives/save-action/action-info.interface';
import { Level } from 'src/app/models/shared/level.interface';
// ROLLBACK BDQ
@Component({
  selector: 'app-exam',
  templateUrl: './exam.component.html',
  styleUrls: ['./exam.component.scss']
})
export class ExamComponent extends NotificationClass implements OnInit, OnDestroy {

  @ViewChild('questions') questionsComponent: ExamQuestionComponent;

  public conceptContents: Array<Content> = [];
  public currentQuestion: Question;
  public canMoveToNextQuestion: boolean = false;
  public examFinished: boolean = false;
  public reviewingConcept: any;
  public moduleId: string;

  public questionWidth: number = 50;
  public reviewWidth: number = 50;
  private _questionDiff: number = 0;
  private _reviewDiff: number = 0;
  private subjectId: string;
  private levels: Array<Level>;
  public levelDict: {};
  public userProgress: any;
  private module: Module;
  public subject: Subject;
  public answerResult: Answer;
  private nextQuestion: any;
  private _confirmingAnswer: boolean = false;

  private actionId: string;
  private questionActionId: string;
  private reviewActionId: string;
  private _examStarted: boolean = false;

  constructor(
    protected _snackBar: MatSnackBar,
    private _examService: ContentExamService,
    private _sharedService: SharedService,
    private _modulesService: ContentModulesService,
    private _userService: UserService,
    private _activatedRoute: ActivatedRoute,
    private _router: Router,
    private _analyticsService: AnalyticsService
  ) {
    super(_snackBar);
  }

  ngOnInit() {
    this.moduleId = this._activatedRoute.snapshot.paramMap.get('moduleId');
    this.subjectId = this._activatedRoute.snapshot.paramMap.get('subjectId');
    this._setExamStartAction();
    this._loadLevels();
    this._loadModule(this.moduleId);
    this._loadUserSubjectProgress(this.moduleId, this.subjectId);
  }

  public startExam(): void {
    if (!this._examStarted) {
      this._examStarted = true;
      this._examService.startExam(this.moduleId, this.subjectId).subscribe(response => {
        this.currentQuestion = response.data;
        this._setExamQuestionStartAction();
        this.canMoveToNextQuestion = false;
        this.examFinished = false;
        this._examStarted = false;
      }, (error) => {
        this.notify(
          this.getErrorNotification(error)
        );
      });
    }
  }

  public goToNextQuestion(): void {
    !this.nextQuestion ?
      this._finishExam() :
      this._continueExam();
  }

  public confirmAnswer(answer: Answer): void {
    if (!this._confirmingAnswer) {
      this._confirmingAnswer = true;
      const concepts = answer.concepts ? answer.concepts.map(c => c.concept) : [];

      this._examService.answerQuestion(
        this.moduleId, this.subjectId, this.currentQuestion.id, answer.id,
        this.module.title, concepts
      ).subscribe(response => {
        this._setExamQuestionFinishAction();

        answer.isRight = response.data.hasAnsweredCorrectly;
        this.answerResult = answer;
        this.nextQuestion = response.data.nextQuestion;

        if (response.data.hasAchievedNewLevel) {
          this.userProgress.level = response.data.levelAchieved +
            (response.data.progress === 1 && response.data.levelAchieved === 3 ? 1 : 0);
          this.nextQuestion = null;
          this.answerResult = null;
          this._setAchievedNewLevelAction();
          this._finishExam();
        }

        this.userProgress.progress = response.data.progress;
        // this.userProgress.passPercentage = response.data.passPercentage;
        this.canMoveToNextQuestion = true;
        this._confirmingAnswer = false;
      }, err => {
        /*const errString = this.getErrorNotification(err);
        this.notify(errString);
        if (errString === 'Não foi possível obter nenhuma questão para o nível atual' ||
          errString === 'Não há a possibilidade de progredir no nivel atual') {
          this._router.navigate(['/modulo/' + this.moduleId]);
        } else {
          this._confirmingAnswer = false;
        }*/
        this.notify(this.getErrorNotification(err));
        this._confirmingAnswer = false;
      });
    }
  }

  public openReview(concept: string): void {
    if (this.subject && this.subject.contents && concept !== this.reviewingConcept) {
      this.subject.contents.forEach(subjectContent => {
        if (subjectContent.concepts && subjectContent.concepts.find(x => x.name === concept))
          this.conceptContents.push(subjectContent);
      });
      this.reviewingConcept = concept;
      this._setReviewStartAction(concept);
    }
  }

  public closeReview(): void {
    this.reviewingConcept = null;
    this.conceptContents = [];
    this._setReviewFinishAction();
  }

  public resizeWindows(offset: number): void {
    const totalWidth = window.innerWidth > 1200 ? 1200 : window.innerWidth;
    const halfWidth = totalWidth / 2;

    let questionWidth = (50 * (halfWidth - offset)) / halfWidth;
    let reviewWidth = (50 * (halfWidth + offset)) / halfWidth;

    questionWidth = questionWidth + this._questionDiff;
    reviewWidth = reviewWidth + this._reviewDiff;

    if (questionWidth < 30) {
      this.questionWidth = 30;
      this.reviewWidth = 70;
    } else if (reviewWidth < 30) {
      this.questionWidth = 70;
      this.reviewWidth = 30;
    } else {
      this.questionWidth = questionWidth;
      this.reviewWidth = reviewWidth;
    }
  }

  public setFinalOffset(): void {
    this._questionDiff = this.questionWidth - 50;
    this._reviewDiff = this.reviewWidth - 50;
  }

  public backToModule() {
    this._router.navigate(['/modulo/' + this.moduleId]);
  }

  public finish() {
    this._router.navigate(['/modulo/' + this.moduleId]);
  }

  private _finishExam(): void {
    this.examFinished = true;
    this._setExamFinishAction();
  }

  private _continueExam(): void {
    this.currentQuestion = this.nextQuestion;
    this.nextQuestion = null;
    this.answerResult = null;
    this.canMoveToNextQuestion = false;
    this._setExamQuestionStartAction();

    if (this.questionsComponent)
      this.questionsComponent.resetAnswer();
  }

  private _loadModule(moduleId: any): any {
    this._modulesService.getModuleById(moduleId).subscribe((response) => {
      this.module = response.data;
      this.subject = this.module.subjects.find(x => x.id === this.subjectId);
    }, () => {
      this.notify('Ocorreu um erro, por favor tente novamente mais tarde');
    });
  }

  private _loadLevels(): void {
    this._sharedService.getLevels(true).subscribe((response) => {
      this.levels = response.data;
      this.levelDict = {};
      this.levels.forEach(level => {
        this.levelDict[level.id] = level.description;
      });
    }, () => this.notify('Ocorreu um erro, por favor tente novamente mais tarde'));
  }

  private _loadUserSubjectProgress(moduleId, subjectId) {
    this._userService.getUserSubjectProgress(moduleId, subjectId).subscribe((response) => {
      this.userProgress = response.data;
      if (this.userProgress.level === 4) {
        this.notify('Você já atingiu o nível máximo neste assunto.');
        this.finish();
      }
    }, () => this.notify('Ocorreu um erro, por favor tente novamente mais tarde'));
  }

  /* Actions Methods */
  private _setExamStartAction(): void {
    const actionInfo = this._getActionInfo('exam-start', ActionInfoTypeEnum.Access);
    this._analyticsService.saveAction(actionInfo).subscribe(() => {
      const waitingAction = this._getActionInfo('exam-leave', ActionInfoTypeEnum.CloseTab);
      this.actionId = this._analyticsService.setWaitingAction(waitingAction);
    });
  }

  private _setExamFinishAction(): void {
    const actionInfo = this._getActionInfo('exam-finish', ActionInfoTypeEnum.Finish);
    this._analyticsService.saveAction(actionInfo).subscribe(() => {
      this._analyticsService.clearWaitingAction(this.actionId);
      this.actionId = null;
    });
  }

  private _setExamQuestionStartAction(): void {
    const actionInfo = this._getActionInfo('exam-question-start', ActionInfoTypeEnum.Access);
    actionInfo.questionId = this.currentQuestion.id;

    this._analyticsService.saveAction(actionInfo).subscribe(() => {
      const waitingAction = this._getActionInfo('exam-question-leave', ActionInfoTypeEnum.CloseTab);
      waitingAction.questionId = this.currentQuestion.id;
      this.questionActionId = this._analyticsService.setWaitingAction(waitingAction);
    });
  }

  private _setExamQuestionFinishAction(): void {
    const actionInfo = this._getActionInfo('exam-question-finish', ActionInfoTypeEnum.Finish);
    actionInfo.questionId = this.currentQuestion.id;

    this._analyticsService.saveAction(actionInfo).subscribe(() => {
      this._analyticsService.clearWaitingAction(this.questionActionId);
      this.questionActionId = null;
    });
  }

  private _setReviewStartAction(concept: string): void {
    const actionInfo = this._getActionInfo('exam-concept-review-start', ActionInfoTypeEnum.Access);
    actionInfo.concept = concept;
    actionInfo.questionId = this.currentQuestion.id;

    this._analyticsService.saveAction(actionInfo).subscribe(() => {
      const waitingAction = this._getActionInfo('exam-concept-review-leave', ActionInfoTypeEnum.CloseTab);
      waitingAction.concept = concept;
      waitingAction.questionId = this.currentQuestion.id;
      this.reviewActionId = this._analyticsService.setWaitingAction(waitingAction);
    });
  }

  private _setReviewFinishAction(): void {
    const actionInfo = this._getActionInfo('exam-concept-review-finish', ActionInfoTypeEnum.Finish);
    this._analyticsService.saveAction(actionInfo).subscribe(() => {
      this._analyticsService.clearWaitingAction(this.reviewActionId);
      this.reviewActionId = null;
    });
  }

  private _setAchievedNewLevelAction(): void {
    const actionInfo = this._getActionInfo('achieved-new-level', ActionInfoTypeEnum.LevelUp);
    actionInfo.page = ActionInfoPageEnum.Subject;
    actionInfo.questionId = this.currentQuestion.id;

    this._analyticsService.saveAction(actionInfo).subscribe(() => {
      this._analyticsService.clearWaitingAction(this.questionActionId);
      this.questionActionId = null;
    });
  }

  private _getActionInfo(description: string, type: ActionInfoTypeEnum): ActionInfo {
    return {
      'page': ActionInfoPageEnum.Exam,
      'description': description,
      'type': type,
      'moduleId': this.moduleId,
      'subjectId': this.subjectId
    };
  }

  ngOnDestroy() {
    if (this.actionId)
      this._saveWaitingAction(this.actionId);
    if (this.questionActionId)
      this._saveWaitingAction(this.questionActionId);
    if (this.reviewActionId)
      this._saveWaitingAction(this.reviewActionId);
  }

  private _saveWaitingAction(actionId: string): void {
    const action = this._analyticsService.getStorageWaitingActionById(actionId);
    if (action) {
      this._analyticsService.saveAction(action).subscribe();
      this._analyticsService.clearWaitingAction(actionId);
    }
  }

}
