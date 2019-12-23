import { Component, Input, Output, EventEmitter, OnInit } from '@angular/core';
import { MatDialog, MatSnackBar } from '@angular/material';
import { SubscriptionDialogComponent } from '../subscription-dialog/subscription-dialog.component';
import { Event } from '../../../models/event.model';
import { UtilService } from '../../../shared/services/util.service';
import { EventSchedule } from '../../../models/event-schedule.model';
import { Router, ActivatedRoute } from '@angular/router';
import { NotificationClass } from 'src/app/shared/classes/notification';
import { EventApplication, PrepQuizAnswer } from 'src/app/models/event-application.model';
import { ContentEventsService } from '../../_services/events.service';
import { ApplicationStatus } from 'src/app/models/enums/application-status';
import { ModuleProgress } from 'src/app/models/previews/module-progress.interface';

@Component({
  selector: 'app-classroom-lesson-header',
  templateUrl: './lesson-header.component.html',
  styleUrls: ['./lesson-header.component.scss']
})
export class ClassroomLessonHeaderComponent extends NotificationClass {

  public disableApplication: boolean = true;

  @Input() progress?;
  @Input() set setEvent(event: Event) {
    this.event = event;
    if (this.event && this.event.schedules && this.event.schedules.length > 0) {
      const scheduleId = this._activatedRoute.snapshot.paramMap.get('scheduleId');

      this.currentSchedule = scheduleId ?
        this.event.schedules.find(x => x.id === scheduleId) :
        this.event.schedules[0];

      this.disableApplication = this._verifyDisableApplication();
    }
  }
  @Input() set setEventApplication(eventApplication: EventApplication) {
    this.eventApplication = new EventApplication();
    if (eventApplication && eventApplication.eventId != null) {
      this.eventApplication = eventApplication;
      switch (this.eventApplication.applicationStatus) {
        case ApplicationStatus.Pending:
          this.subscriptionStatus = 'Em Análise';
          break;
        case ApplicationStatus.Rejected:
          this.subscriptionStatus = 'Rejeitado';
          break;
        case ApplicationStatus.Approved:
          this.subscriptionStatus = 'Aprovado';
          break;
        case ApplicationStatus.Full:
          this.subscriptionStatus = 'Lotado';
          break;
        default:
          break;
      }
    }
  }
  @Input() isManagement?: boolean = false;
  @Output() showSubscriptionMessage = new EventEmitter();

  public event: Event;
  public eventApplication: EventApplication;
  public currentSchedule: EventSchedule;
  public subscriptionStatus: string;

  constructor(
    public _snackBar: MatSnackBar,
    private _dialog: MatDialog,
    private _eventsService: ContentEventsService,
    private _utilService: UtilService,
    private _activatedRoute: ActivatedRoute,
    private _router: Router
  ) {
    super(_snackBar);
  }

  public getDuration(): string {
    if (!this.currentSchedule) return '--';
    return this._utilService.formatSecondsToHourMinute(this.currentSchedule.duration);
  }

  public openSubscriptionDialog() {
    if (this.event.prepQuizQuestionList) {
      const Questionnaire = new Array<any>();
      this.event.prepQuizQuestionList.forEach(q => {
        Questionnaire.push({ question: q, answer: '' });
      });
      const dialogRef = this._dialog.open(SubscriptionDialogComponent, {
        width: '1000px',
        data: Questionnaire
      });

      dialogRef.afterClosed().subscribe(async (resolution: any) => {
        if (resolution) {
          this.eventApplication.eventId = this.event.id;
          this.eventApplication.scheduleId = this.currentSchedule.id;
          this.eventApplication.answers = resolution.map(x => x.answer);
          this.eventApplication.prepQuizAnswersList = new Array<PrepQuizAnswer>();
          for (let i = 0; i < resolution.length; i++) {
            this.eventApplication.prepQuizAnswersList.push({
              'answer': resolution[i].answer,
              'fileAsAnswer': resolution[i].fileAsAnswer || false
            });
          }

          await this._apply();
        }
      });
    }

    // if (this.event.prepQuiz) {
    //   const Questionnaire = new Array<any>();
    //   this.event.prepQuiz.forEach(q => {
    //     Questionnaire.push({ question: q, answer: '' });
    //   });
    //   const dialogRef = this._dialog.open(SubscriptionDialogComponent, {
    //     width: '1000px',
    //     data: Questionnaire
    //   });

    //   dialogRef.afterClosed().subscribe(async (resolution: any) => {
    //     if (resolution) {
    //       this.eventApplication.eventId = this.event.id;
    //       this.eventApplication.scheduleId = this.currentSchedule.id;
    //       this.eventApplication.answers = resolution.map(x => x.answer);
    //       await this._apply();
    //     }
    //   });
    // }


  }

  private _verifyDisableApplication(): boolean {
    if (!this.currentSchedule) return true;
    const subscriptionEnd = new Date(
      this.currentSchedule.subscriptionEndDate.toString()
    ).getTime();
    const currentTime = new Date().getTime();
    const adjustedLimit = subscriptionEnd + 86400000;

    if (adjustedLimit < currentTime)
      return true;

    if (this.event.requirements.length > 0) {
      if (!this.progress) { return true; }

      return !this.event.requirements.every(req => {
        if (req.optional) return true;

        const progress: ModuleProgress = this.progress[req.moduleId];
        return progress && progress.level > req.level;
      });
    }
    return false;
  }

  private _apply(): void {
    this._eventsService.applyToEvent(this.eventApplication).subscribe((response) => {
      if (response.success) {
        this.subscriptionStatus = 'Em Análise';
        this.showSubscriptionMessage.emit(true);
      }
    }, () => this.notify('Ocorreu um erro, por favor tente novamente mais tarde'));
  }

  public goEditEvent() {
    localStorage.setItem('editingEvent',
      JSON.stringify(this.event)
    );
    this._router.navigate(['/configuracoes/evento']);
  }
}
