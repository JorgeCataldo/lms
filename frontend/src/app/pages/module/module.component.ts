import { Component, OnInit } from '@angular/core';
import { ContentModulesService } from '../_services/modules.service';
import { NotificationClass } from '../../shared/classes/notification';
import { MatSnackBar } from '@angular/material';
import { ActivatedRoute, Router } from '@angular/router';
import { Module } from '../../models/module.model';
import { UtilService } from '../../shared/services/util.service';
import { SharedService } from '../../shared/services/shared.service';
import { Level } from '../../models/shared/level.interface';
import { UserService } from '../_services/user.service';
import Player from '@vimeo/player';
import { AnalyticsService } from 'src/app/shared/services/analytics.service';
import { ActionInfoPageEnum, ActionInfoTypeEnum } from 'src/app/shared/directives/save-action/action-info.interface';
import { ForumQuestion } from 'src/app/models/forum.model';
import { RequirementProgress } from 'src/app/settings/modules/new-module/models/new-requirement.model';
import { SettingsValuationTestsService } from 'src/app/settings/_services/valuation-tests.service';
import { ValuationTestTypeEnum, ValuationTestModuleTypeEnum } from 'src/app/models/enums/valuation-test-type-enum';
import { ValuationTest } from 'src/app/models/valuation-test.interface';
import { formatDate } from '@angular/common';

@Component({
  selector: 'app-module',
  templateUrl: './module.component.html',
  styleUrls: ['./module.component.scss']
})
export class ModuleComponent extends NotificationClass implements OnInit {

  public module: Module;
  public forumQuestionsPreview: ForumQuestion[] = [];
  public levels: Array<Level> = [];
  public userProgress: any;
  public userModuleInfo: any;
  public levelDict: any;
  public player;
  public moduleTestsResearch: any[] = [];
  public moduleTestsBefore: any[] = [];
  public moduleTestsAfter: any[] = [];
  public moduleTestsResearchExpanded: boolean = false;
  public moduleTestsBeforeExpanded: boolean = false;
  public moduleTestsAfterExpanded: boolean = false;
  public lockModule: boolean = false;
  private _watchedVideo: boolean = false;
  public moduleTests: any[] = [];
  public moduleTestsExpanded: boolean = false;

  constructor(
    protected _snackBar: MatSnackBar,
    private _modulesService: ContentModulesService,
    private _activatedRoute: ActivatedRoute,
    private _utilService: UtilService,
    private _sharedService: SharedService,
    private _userService: UserService,
    private _analyticsService: AnalyticsService,
    private _testsService: SettingsValuationTestsService,
    private _router: Router
  ) {
    super(_snackBar);
  }

  ngOnInit() {
    this._activatedRoute.params.subscribe(
      (params) => {
        this._modulesService.getModuleById(
          params.moduleId
        ).subscribe((response) => {
          this.module = response.data;
          this._loadUserSubjectProgress(params.moduleId);
          this._loadUserModuleInfo(params.moduleId);
          this._loadModuleTests(params.moduleId);

          localStorage.setItem('forumQuestionDialog',
            JSON.stringify({
              moduleId: this.module.id,
              moduleName: this.module.title,
              subjectId: '',
              subjectName: '-',
              contentId: '',
              contentName: '-',
              position: '-'
            })
          );
          this.module.duration = this.module.videoDuration +
            this.module.subjects.reduce((sum, subj) => {
              return sum + subj.contents.reduce((sumCont, cont) => sumCont + cont.duration, 0);
            }, 0);
          this._setIntroductionVideo();

        }, (error) => {
          this.notify(
            this.getErrorNotification(error)
          );
        });
        this._loadLevels();
        this.loadModuleForumPreview(params.moduleId);
    });

    window.scroll(0, 0);
  }

  public getVideoDurationFormatted(): string {
    return this._utilService.formatSecondsToMinutes(this.module.videoDuration);
  }

  public getProgressForSubject(subjectId: string) {
    return this.userProgress && this.userProgress.subjectsProgressDict[subjectId] ?
      this.userProgress.subjectsProgressDict[subjectId] :
      { level: 0, progress: 0 };
  }

  public checkRequirements(): boolean {
    if (!this.module) return false;
    if (this.module && (!this.module.requirements || this.module.requirements.length === 0))
      return true;

    return this.module.requirements.every(req => {
      return req.optional || (
        req.progress && req.progress.level > req.level
      );
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

  private _loadUserSubjectProgress(moduleId: string): void {
    this._userService.getUserModuleProgress(moduleId).subscribe((response) => {
      this.userProgress = response.data;
      this.userProgress.subjectsProgressDict = {};
      this.userProgress.subjectsProgress.forEach(sbp => {
        this.userProgress.subjectsProgressDict[sbp.subjectId] = sbp;
      });
      this.module = this._setRequirementsProgress(
        this.module, response.data.requirementsProgress
      );
    }, () => this.notify('Ocorreu um erro, por favor tente novamente mais tarde'));
  }

  private _loadUserModuleInfo(moduleId: string): void {
    this._userService.getBasicProgressInfo(moduleId).subscribe((response) => {
      console.log('_loadUserModuleInfo -> ', response.data);
      this.userModuleInfo = response.data;
      this.userModuleInfo.modulesInfo.forEach(element => {
        if (element.validFor && element.validFor > 0 && element.id === moduleId) {
          if (new Date(element.dueDate).getTime() < new Date().getTime()) {
            localStorage.setItem('expiredModule', 'Seu acesso ao módulo expirou');
            this._router.navigate([ 'home' ]);
          }
        }
      });
    }, () => this.notify('Ocorreu um erro, por favor tente novamente mais tarde'));
  }

  public loadModuleForumPreview(moduleId: string): void {
    this._userService.getModuleForumPreview(moduleId, 2).subscribe((response) => {
      this.forumQuestionsPreview = response.data;
    }, () => this.notify('Ocorreu um erro, por favor tente novamente mais tarde'));
  }

  private _setIntroductionVideo(): void {
    if (this.module.videoUrl && this.module.videoUrl !== '') {
      if (document.getElementById('videoContent')) {
        const w = window.innerWidth;
        if ( w <= 414) {
          const options = {
            id: this.module.videoUrl,
            height: '181'
          };
          this.player = new Player('videoContent', options);
          this._handleVideoLoaded( this.player );
          this._handleVideoPlayed( this.player );
        } else {
          const option = {
            id: this.module.videoUrl,
            height: '470'
          };
          this.player = new Player('videoContent', option);
        this._handleVideoLoaded( this.player );
        this._handleVideoPlayed( this.player );
        }
      }
    } else {
      const videoEl = document.getElementById('videoContent');
      if (videoEl)
        videoEl.innerHTML = '';
    }
  }

  private _handleVideoLoaded(player): void {
    player.on('loaded', () => {
      const frame = document.querySelector('iframe');
      if (frame) { frame.style.width = '100%'; }
      const divFrame = document.getElementById('videoContent');
      divFrame.style.visibility = 'initial';
    });
  }

  private _handleVideoPlayed(player) {
    player.on('play', () => {
      if (!this._watchedVideo) {
        this._watchedVideo = true;
        this._analyticsService.saveAction({
          'page': ActionInfoPageEnum.Module,
          'description': 'introduction-video',
          'type': ActionInfoTypeEnum.VideoPlay,
          'moduleId': this.module.id
        }).subscribe();
      }
    });
  }

  private _loadModuleTests(moduleId: string) {
    this._testsService.getModuleValuationTests(moduleId).subscribe(res => {
      if (res.data.some(x => x.type === ValuationTestTypeEnum.Percentile)) {
        this.moduleTestsResearch = res.data.filter(x => x.type === ValuationTestTypeEnum.Percentile);
      }
      if (res.data.some(x => x.type === ValuationTestTypeEnum.Free)) {
        const moduleTestsFree = res.data.filter(x => x.type === ValuationTestTypeEnum.Free);
        if (moduleTestsFree.some(x => x.testModules.some(y => y.type === ValuationTestModuleTypeEnum.BeforeModule))) {
          this.moduleTestsBefore = moduleTestsFree.filter(x =>
            x.testModules.some(y => y.type === ValuationTestModuleTypeEnum.BeforeModule)
          );
          this.lockModule = this.moduleTestsBefore.some(x => x.answered === false);
        }
        if (moduleTestsFree.some(x => x.testModules.some(y => y.type === ValuationTestModuleTypeEnum.AfterModule))) {
          this.moduleTestsAfter = moduleTestsFree.filter(x =>
            x.testModules.some(y => y.type === ValuationTestModuleTypeEnum.AfterModule)
          );
        }
      }
    }, err => {
      this.notify(this.getErrorNotification(err));
    });
  }

  public getModuleTestPercent(test: ValuationTest): number {
    return test.testModules.find(x => x.id === this.module.id).percent;
  }

  public disablePercentButtonLogic(answered: boolean, percent: number): boolean {
    if (answered) {
      return true;
    }
    if (percent > 1) {
      percent = percent / 100;
    }
    if (this.userProgress == null) {
      return true;
    } else {
      const progress = (this.userProgress.level + this.userProgress.progress) * 0.25;
      if (progress >= percent) {
        return false;
      } else {
        return true;
      }
    }
  }

  public disablePercentButtonLogicText(answered: boolean, percent: number): string {
    if (answered) {
      return 'Teste respondido';
    }
    if (percent > 1) {
      percent = percent / 100;
    }
    if (this.userProgress == null) {
      return 'Progresso necessário ' + (percent * 100).toString() + '%';
    } else {
      const progress = (this.userProgress.level + this.userProgress.progress) * 0.25;
      if (progress >= percent) {
        return 'Fazer o teste';
      } else {
        return 'Progresso necessário ' + (percent * 100).toString() + '%';
      }
    }
  }

  public goToTest(testId: string): void {
    this._router.navigate([ '/teste-de-avaliacao/' + testId ]);
  }

  public disableGrade(): boolean {
    if (this.userProgress == null) {
      return true;
    }

    if (this.userProgress.level !== 4) {
      return true;
    }

    if (this.moduleTestsBefore.some(x => x.answered === false) ||
    this.moduleTestsAfter.some(x => x.answered === false) ||
    this.moduleTestsResearch.some(x => x.answered === false)) {
      return true;
    }

    return false;
  }

  private _setRequirementsProgress(
    module: Module, requirementsProgress: Array<RequirementProgress>
  ): Module {
    if (module.requirements) {
      module.requirements.forEach(req => {
        const reqProgress = requirementsProgress.find(p => p.moduleId === req.moduleId);
        if (reqProgress) {
          req.progress = {
            'moduleId': reqProgress.moduleId,
            'level': reqProgress.level,
            'progress': reqProgress.progress
          };
        }
      });
    }

    return module;
  }

  public goToPageEcommerceUrl() {
    window.open(this.module.ecommerceUrl, '_blank');
  }

  public goToPageStoreUrl() {
    window.open(this.module.storeUrl, '_blank');
  }

}
