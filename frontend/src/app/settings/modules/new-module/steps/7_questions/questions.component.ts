import { Component, Input, Output, EventEmitter, OnInit, ViewChild } from '@angular/core';
import { MatSnackBar, MatDialog, MatTabGroup } from '@angular/material';
import { NotificationClass } from '../../../../../shared/classes/notification';
import { Module } from '../../../../../models/module.model';
import { Question, InvalidSubjectItem, QuestionExcel } from '../../../../../models/question.model';
import { NewQuestionDialogComponent } from './new-question/new-question.dialog';
import { SettingsModulesService } from '../../../../_services/modules.service';
import { UtilService } from '../../../../../shared/services/util.service';
import { Level } from '../../../../../models/shared/level.interface';
import { ConfirmDialogComponent } from '../../../../../shared/dialogs/confirm/confirm.dialog';
import { Subject as rxjsSubject } from 'rxjs';
import { debounceTime } from 'rxjs/operators';
import { Subject } from '../../../../../models/subject.model';
import { UploadQuestionDatabaseDialogComponent } from './upload-qdb/upload-qdb.dialog';
import { MissingLevelsDialogComponent } from './missing-levels/missing-levels.dialog';
import { ExcelService } from 'src/app/shared/services/excel.service';
import { SettingsModulesDraftsService } from 'src/app/settings/_services/modules-drafts.service';
import { ModuleGradeTypeEnum } from 'src/app/models/enums/ModuleGradeTypeEnum';

@Component({
  selector: 'app-new-module-questions',
  templateUrl: './questions.component.html',
  styleUrls: ['../new-module-steps.scss', './questions.component.scss']
})
export class NewModuleQuestionsComponent extends NotificationClass implements OnInit {

  @ViewChild('tabGroup') tabGroup: MatTabGroup;

  @Input() readonly module: Module;
  @Input() readonly levels: Array<Level>;
  @Output() addQuestions = new EventEmitter();

  public questions: Array<Question> = [];
  public newQuestion: Question;
  public searchValue: string = '';
  public questionsCount: number = 0;
  public questionsLimit: number;
  public moduleGradeType: ModuleGradeTypeEnum;

  private _currentPage: number = 1;
  private _searchSubject: rxjsSubject<string> = new rxjsSubject();

  constructor(
    protected _snackBar: MatSnackBar,
    private _dialog: MatDialog,
    private _utilService: UtilService,
    private _excelService: ExcelService,
    private _draftsService: SettingsModulesDraftsService
  ) {
    super(_snackBar);
  }

  ngOnInit() {
    this.questionsLimit = this.module.questionsLimit;
    this.moduleGradeType = this.module.moduleGradeType ?
      this.module.moduleGradeType : ModuleGradeTypeEnum.SubjectsLevel;
    this.loadQuestions();
    this._setSearchSubscription();
  }

  public confirmRemoveQuestion(questionId: string): void {
    const dialogRef = this._dialog.open(ConfirmDialogComponent, {
      width: '400px',
      data: { message: 'Tem certeza que deseja remover esta questão?' }
    });

    dialogRef.afterClosed().subscribe((result: boolean) => {
      if (result)
        this._removeQuestion(questionId);
    });
  }

  public openQuestionDialog(subject: Subject) {
    const dialogRef = this._dialog.open(NewQuestionDialogComponent, {
      width: '1000px',
      data: {
        'question': new Question(subject.id, [], this.questions.length),
        'concepts': subject.concepts,
        'levels': this.levels
      }
    });

    dialogRef.afterClosed().subscribe((question: Question) => {
      if (question)
        this._addQuestion(question);
    });
  }

  public exportSubjectQuestions() {
    this._draftsService.getAllQuestionsDraft(this.module.id).subscribe(res => {
      const excelQuestions: QuestionExcel[] = [];
      for (let questionIndex = 0; questionIndex < res.data.length; questionIndex++) {
        const question = res.data[questionIndex];
        const newExcelLine = new QuestionExcel();
        newExcelLine.subjectId = question.subjectId;
        newExcelLine.subjectTitle = this.module.subjects.find( s => s.id === question.subjectId).title;
        newExcelLine.level = question.level;
        newExcelLine.text = question.text;
        newExcelLine.duration = question.duration;
        for (let answerIndex = 0; answerIndex < question.answers.length; answerIndex++) {
          const answer = question.answers[answerIndex];
          switch (answerIndex) {
            case 0:
              newExcelLine.answer1 = answer.description;
              newExcelLine.answer1Points = answer.points;
              newExcelLine.answer1Concepts = answer.concepts.filter(x => x.isRight).map(x => x.concept).join(',');
              break;
            case 1:
              newExcelLine.answer2 = answer.description;
              newExcelLine.answer2Points = answer.points;
              newExcelLine.answer2Concepts = answer.concepts.filter(x => x.isRight).map(x => x.concept).join(',');
              break;
            case 2:
              newExcelLine.answer3 = answer.description;
              newExcelLine.answer3Points = answer.points;
              newExcelLine.answer3Concepts = answer.concepts.filter(x => x.isRight).map(x => x.concept).join(',');
              break;
            case 3:
              newExcelLine.answer4 = answer.description;
              newExcelLine.answer4Points = answer.points;
              newExcelLine.answer4Concepts = answer.concepts.filter(x => x.isRight).map(x => x.concept).join(',');
              break;
            case 4:
              newExcelLine.answer5 = answer.description;
              newExcelLine.answer5Points = answer.points;
              newExcelLine.answer5Concepts = answer.concepts.filter(x => x.isRight).map(x => x.concept).join(',');
              break;
          }
        }
        excelQuestions.push(newExcelLine);
      }
      this._excelService.exportAsExcelFile(excelQuestions, 'BDQ - ' + this.module.title);
    });
  }

  public openUploadDialog(module: Module) {
    const dialogRef = this._dialog.open(UploadQuestionDatabaseDialogComponent, {
      width: '1000px',
      data: {
        'module': module,
      }
    });

    dialogRef.afterClosed().subscribe((question: Question) => {
      this.loadQuestions();
    });
  }

  public editQuestion(subject: Subject, dbQuestion: Question) {
    if (typeof dbQuestion.duration !== 'string') {
      dbQuestion.duration = (
        this._utilService.formatDurationToHour(dbQuestion.duration) as any
      );
    }

    const dialogRef = this._dialog.open(NewQuestionDialogComponent, {
      width: '1000px',
      data: {
        'question': dbQuestion,
        'concepts': subject.concepts,
        'levels': this.levels
      }
    });

    dialogRef.afterClosed().subscribe((question: Question) => {
      if (question)
        this._addQuestion(question);
    });
  }

  public getQuestionDuration(duration: number | string): string {
    if (typeof duration === 'string') {
      const durStr = duration.split(':').join('');
      return durStr.length > 5 ?
        durStr.substring(2, 4) + ':' + durStr.substring(4, 6) : duration;
    }
    return this._utilService.formatSecondsToMinutes(duration);
  }

  public getLevelDescription(levelId: number): string {
    if (!this.levels) return '';
    const level = this.levels.find(lev => lev.id === levelId);
    return level ? level.description : '';
  }

  public updateSearch(searchTextValue: string) {
    this._searchSubject.next(searchTextValue);
  }

  public goToPage(page: number) {
    if (page !== this._currentPage) {
      this._currentPage = page;
      this.loadQuestions();
    }
  }

  public nextStep(): void {
    /*this._modulesService.checkModuleQuestions(this.module.id, this.module.subjects.map(x => x.id)).subscribe((response) => {
      const invalidSubject: Array<InvalidSubjectItem> = response.data;
      if (invalidSubject.length === 0) {
        this.addQuestions.emit({
          'questions': this.questions,
          'questionsLimit': this.questionsLimit,
          'moduleGradeType': this.moduleGradeType
        });
      } else {
        invalidSubject.forEach(invSub => {
          invSub.subjectTitle = this.module.subjects.find(x => x.id === invSub.subjectId).title;
        });
        const dialogRef = this._dialog.open(MissingLevelsDialogComponent, {
          width: '1000px',
          data: invalidSubject
        });
        dialogRef.afterClosed().subscribe((result: boolean) => {
          if (result) {
            this.addQuestions.emit({
              'questions': this.questions,
              'questionsLimit': this.questionsLimit,
              'moduleGradeType': this.moduleGradeType
            });
          }
        });
      }
    }, () => this.notify('Ocorreu um erro, por favor tente novamente mais tarde') );*/
    this.addQuestions.emit({
      'questions': this.questions,
      'questionsLimit': this.questionsLimit,
      'moduleGradeType': this.moduleGradeType
    });
  }

  private _addQuestion(question: Question): void {
    this._draftsService.manageQuestionDraft(this.module.id, question).subscribe(() => {
      this.loadQuestions();

    }, () => this.notify('Ocorreu um erro, por favor tente novamente mais tarde'));
  }

  private _removeQuestion(questionId: string): void {
    this._draftsService.removeQuestionDraft(questionId).subscribe(() => {
      this.loadQuestions();

    }, () => this.notify('Ocorreu um erro, por favor tente novamente mais tarde'));
  }

  public loadQuestions(): void {
    if (this.module.subjects) {
      const subject = this.module.subjects[this.tabGroup.selectedIndex || 0];
      if (subject && subject.id) {

        this._draftsService.getPagedQuestionsDraft(
          subject.id, this.module.id, this._currentPage, 10, this.searchValue
        ).subscribe((response) => {
          this.questions = response.data.questions;
          this.questionsCount = response.data.itemsCount;

        }, () => this.notify('Ocorreu um erro, por favor tente novamente mais tarde'));
      }
    }
  }

  private _setSearchSubscription() {
    this._searchSubject.pipe(
      debounceTime(500)
    ).subscribe((searchValue: string) => {
      this.searchValue = searchValue;
      this.loadQuestions();
    });
  }
}
