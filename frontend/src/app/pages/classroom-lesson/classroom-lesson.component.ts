import { Component, OnInit, Input } from '@angular/core';
import { NotificationClass } from '../../shared/classes/notification';
import { MatSnackBar } from '@angular/material';
import { ActivatedRoute, Router } from '@angular/router';
import { ContentEventsService } from '../_services/events.service';
import { Event } from '../../models/event.model';
import { Level } from '../../models/shared/level.interface';
import { SharedService } from '../../shared/services/shared.service';
import { EventApplication } from 'src/app/models/event-application.model';
import { UserService } from '../_services/user.service';
import { AnalyticsService } from 'src/app/shared/services/analytics.service';
import { ActionInfoPageEnum, ActionInfoTypeEnum } from 'src/app/shared/directives/save-action/action-info.interface';
import Player from '@vimeo/player';
import { ModuleProgress } from 'src/app/models/previews/module-progress.interface';
import { Requirement } from 'src/app/settings/modules/new-module/models/new-requirement.model';
import * as pdfform from 'pdfform.js/pdfform';
import { AuthService } from 'src/app/shared/services/auth.service';
import { UtilService } from 'src/app/shared/services/util.service';
import { HttpClient } from '@angular/common/http';
import { formatDate } from '@angular/common';
import { EventForumQuestion } from 'src/app/models/event-forum.model';

@Component({
  selector: 'app-classroom-lesson',
  templateUrl: './classroom-lesson.component.html',
  styleUrls: ['./classroom-lesson.component.scss']
})
export class ClassroomLessonComponent extends NotificationClass implements OnInit {

  public event: Event;
  public forumQuestionsPreview: EventForumQuestion[] = [];
  public eventApplication: EventApplication;
  public showSubscriptionMessage: boolean = false;
  public disabledQuestionBtn: boolean = false;
  public levels: Array<Level> = [];
  public levelDict: any = {};
  public moduleProgress: any;
  public player;
  private _watchedVideo: boolean = false;

  constructor(
    protected _snackBar: MatSnackBar,
    private _activatedRoute: ActivatedRoute,
    private _eventsService: ContentEventsService,
    private _sharedService: SharedService,
    private _userService: UserService,
    private _router: Router,
    private _analyticsService: AnalyticsService,
    private _authService: AuthService,
    private _utilService: UtilService,
    private _httpClient: HttpClient
  ) {
    super(_snackBar);
  }

  ngOnInit() {
    this._loadLevels();
    this._loadEvent();
    this._loadEventApplication();
    this._loadProgress();
  }

  public showWebinar(): boolean {
    if (!this.event || !this.event.schedules || this.event.schedules.length === 0)
      return false;

    const scheduleId = this._activatedRoute.snapshot.paramMap.get('scheduleId');
    const currentSchedule = scheduleId ?
      this.event.schedules.find(x => x.id === scheduleId) :
      this.event.schedules[0];

    if (!currentSchedule || !currentSchedule.webinarUrl || currentSchedule.webinarUrl.trim() === '')
      return false;

    const today = formatDate(new Date(), 'dd/MM/yyyy', 'pt-BR');
    const dayEvent = formatDate(currentSchedule.eventDate, 'dd/MM/yyyy', 'pt-BR');
    return today === dayEvent;
  }

  public goToEvent(): void {
    const webinarUrl = this.getWebinarUrl();
    const hasProtocol = webinarUrl.split('http').length > 1;
    const windowUrl = hasProtocol ? webinarUrl : '//' + webinarUrl;
    const newWindow = window.open(windowUrl, '_blank');
    newWindow.focus();
  }

  public getWebinarUrl(): string {
    const scheduleId = this._activatedRoute.snapshot.paramMap.get('scheduleId');
    const schedule = scheduleId ?
      this.event.schedules.find(x => x.id === scheduleId) :
      this.event.schedules[0];
    return !schedule ? null : schedule.webinarUrl;
  }

  public getVideoDuration(): number {
    if (!this.event || !this.event.videoDuration) return 0;
    return Math.ceil(this.event.videoDuration / 60);
  }

  public showNotification() {
    this.showSubscriptionMessage = true;
  }

  public dismissNotification() {
    this.showSubscriptionMessage = false;
  }

  public eventScheduleCompleted(eventApplication: EventApplication): boolean {
    if (eventApplication && eventApplication.scheduleId && this.event && this.event.schedules) {
      const schedule = this.event.schedules.find(s => s.id === eventApplication.scheduleId);
      return schedule && schedule.finishedAt != null && eventApplication.finalGrade != null;
    }
    return false;
  }

  public eventScheduleDate(scheduleId: string): boolean {
    if (!this.event || !this.event.schedules) return false;
    return true;
  }

  public getGradedSchedule(scheduleId: string): Date {
    if (!this.event || !this.event.schedules) return null;
    const schedule = this.event.schedules.find(s => s.id === scheduleId);
    return schedule ? schedule.eventDate : null;
  }

  private _loadEvent(): void {
    const eventId = this._activatedRoute.snapshot.paramMap.get('eventId');
    this._eventsService.getEventById(eventId).subscribe((response) => {
      this.event = response.data;
      this._setIntroductionVideo();
      this._saveEventAccessAction();
      const scheduleId = this._activatedRoute.snapshot.paramMap.get('scheduleId');
      this._checkForumDate(scheduleId);
      localStorage.setItem('eventForumQuestionDialog',
        JSON.stringify({
          eventId: this.event.id,
          eventScheduleId: scheduleId,
          eventName: this.event.title,
          position: '-'
        })
      );
      this.loadEventForumPreview(scheduleId);
    }, () => {
      this.notify('Ocorreu um erro, por favor tente novamente mais tarde');
    });
  }

  private _checkForumDate(scheduleId: string) {
    if (this.event) {
      const schedule = this.event.schedules.find(x => x.id === scheduleId);
      if (schedule) {
        const today = new Date();
        if (schedule.forumStartDate != null && schedule.forumEndDate != null) {
          if (new Date(schedule.forumStartDate) > today || new Date(schedule.forumEndDate) < today) {
            this.disabledQuestionBtn = true;
          }
        }
      }
    }
  }

  private _loadEventApplication(): void {
    const eventId = this._activatedRoute.snapshot.paramMap.get('eventId');
    const scheduleId = this._activatedRoute.snapshot.paramMap.get('scheduleId');
    this._eventsService.getEventApplication(eventId, scheduleId).subscribe((response) => {
      this.eventApplication = response.data.eventId ? response.data : null;
    }, () => {
      this.notify('Ocorreu um erro, por favor tente novamente mais tarde');
    });
  }

  private _loadLevels(): void {
    this._sharedService.getLevels(true).subscribe((response) => {
      this.levels = response.data;
      this.levelDict = {};
      response.data.forEach(level => {
        this.levelDict[level.id] = level.description;
      });

    }, () => {
      this.notify('Ocorreu um erro, por favor tente novamente mais tarde');
    });
  }

  private _loadProgress(): void {
    this._userService.getUserModulesProgress().subscribe((response) => {
      this.moduleProgress = {};
      response.data.forEach((mP: ModuleProgress) =>
        this.moduleProgress[mP.moduleId] = mP
      );

    }, () => {
      this.notify('Ocorreu um erro, por favor tente novamente mais tarde');
    });
  }

  public loadEventForumPreview(eventScheduleId: string): void {
    this._userService.getEventForumPreview(eventScheduleId, 2).subscribe((response) => {
      this.forumQuestionsPreview = response.data;
    }, () => {
      this.notify('Ocorreu um erro, por favor tente novamente mais tarde');
    });
  }

  private _saveEventAccessAction(): void {
    this._analyticsService.saveAction({
      'page': ActionInfoPageEnum.Event,
      'description': 'event-access',
      'type': ActionInfoTypeEnum.Access,
      'eventId': this.event.id
    }).subscribe();
  }

  private _setIntroductionVideo(): void {
    if (this.event.videoUrl && this.event.videoUrl !== '') {
      const options = {
        id: this.event.videoUrl,
        height: '470'
      };
      if (document.getElementById('videoContent')) {
        this.player = new Player('videoContent', options);
        this._handleVideoLoaded(this.player);
        this._handleVideoPlayed(this.player);
      }
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
          'page': ActionInfoPageEnum.Event,
          'description': 'introduction-video',
          'type': ActionInfoTypeEnum.VideoPlay,
          'eventId': this.event.id
        }).subscribe();
      }
    });
  }

  public goToModule(req: Requirement): void {
    this._router.navigate(['/modulo/' + req.moduleId]);
  }

  private getEventScheduleFinishedDate(scheduleId: string): Date {
    if (!this.event || !this.event.schedules) return null;
    const schedule = this.event.schedules.find(s => s.id === scheduleId);
    return schedule ? new Date(schedule.finishedAt) : null;
  }

  public generateCertificatePDF(): void {
    this._httpClient.get(
      this.event.certificateUrl, { responseType: 'arraybuffer' }
    ).subscribe(
      (response) => {
        const fields = {};
        fields['nome_evento'] = [this.event.title];
        fields['nome_evento_pequeno'] = [this.event.title];
        fields['nome_aluno'] = [this._authService.getLoggedUser().name];
        fields['data_conclusao'] = [this._utilService.formatDateToDDMMYYYY(
          this.getEventScheduleFinishedDate(this.eventApplication.scheduleId)
        )
        ];
        fields['data_conclusao_extenso'] = [this._utilService.formatDateToName(
          this.getEventScheduleFinishedDate(this.eventApplication.scheduleId)
        )
        ];
        const out_buf = pdfform().transform(response, fields);

        const blob = new Blob([out_buf], { type: 'application/pdf' });
        const fileURL = URL.createObjectURL(blob);
        window.open(fileURL);
      }, () => {
        this.notify('Ocorreu um erro ao carregar o certificado');
      });
  }
}
