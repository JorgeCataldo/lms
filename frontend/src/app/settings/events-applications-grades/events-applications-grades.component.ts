import { Component, OnInit, OnDestroy } from '@angular/core';
import { MatSnackBar, Sort, MatDialog, MatDialogConfig } from '@angular/material';
import { NotificationClass } from '../../shared/classes/notification';
import { ActivatedRoute, Router } from '@angular/router';
import { SettingsEventsService } from '../_services/events.service';
import { EventApplication } from 'src/app/models/event-application.model';
import { trigger, state, transition, style, animate } from '@angular/animations';
import { Event } from '../../models/event.model';
import { EventSchedule } from 'src/app/models/event-schedule.model';
import { ContentEventForumService } from 'src/app/pages/_services/event-forum.service';
import { EventForumParticipationDialogComponent } from 'src/app/shared/dialogs/event-forum-participation/event-forum-participation.dialog';
import { UtilService } from 'src/app/shared/services/util.service';
import { TrackOverview } from 'src/app/models/track-overview.interface';
import { EventsChangeDialogComponent } from './events-chage-date-dialog/events-change-dialog.component';
import { EventPreview } from 'src/app/models/previews/event.interface';
import { ContentEventsService } from 'src/app/pages/_services/events.service';
import { AuthService } from 'src/app/shared/services/auth.service';
import { EventApplicationNoteDialogComponent } from './event-application-note/event-application-note.dialog';
import { ExcelService } from 'src/app/shared/services/excel.service';

 @Component({
   selector: 'app-settings-events-applications-grades',
   templateUrl: './events-applications-grades.component.html',
   styleUrls: ['./events-applications-grades.component.scss'],
   animations: [
     trigger('detailExpand', [
       state('collapsed', style({height: '0px', minHeight: '0', display: 'none'})),
       state('expanded', style({height: '*'})),
       transition('expanded <=> collapsed', animate('225ms cubic-bezier(0.4, 0.0, 0.2, 1)')),
     ]),
   ],
 })
export class SettingsEventsApplicationsGradesComponent extends NotificationClass implements OnInit, OnDestroy {

  public eventId: string = '';
  public events: Array<EventPreview> = [];
  public scheduleId: string = '';
  public event: Event;
  public schedule: EventSchedule;
  public users: EventApplication[] = [];
  private _startUsers: EventApplication[] = [];
  public eventsInfo: Array<Event>;
  public readonly displayedColumns: string[] = [
    'logo', 'name', 'requirements', 'questions', 'rated', 'presence', 'event-date', 'note', 'grade', 'forumGrade'
  ];
  public expandedElement: EventApplication;
  public expandedForumElement: EventApplication;
  public isFinished: boolean = false;
  public pendingPresence: number = 0;
  public fromTrack: boolean = false;
  public noActions: boolean = false;
  public track: TrackOverview;
  public schedules: any[] = [];
  public customGrades: boolean = false;

  constructor(
    protected _snackBar: MatSnackBar,
    private _eventsService: SettingsEventsService,
    private _eventsContentService: ContentEventsService,
    private _eventForumService: ContentEventForumService,
    private _activatedRoute: ActivatedRoute,
    private _utilService: UtilService,
    private _dialog: MatDialog,
    private _router: Router,
    private _authService: AuthService,
    private _excelService: ExcelService
  ) {
    super(_snackBar);
  }

  ngOnInit() {
    this.eventId = this._activatedRoute.snapshot.paramMap.get('eventId');
    this.scheduleId = this._activatedRoute.snapshot.paramMap.get('scheduleId');
    this._loadTrackInfo();
    this._loadApplications();
    this._loadSchedules();
    this._loadEvents();
    this._loadAllEventsByUser();
  }

  ngOnDestroy() {
    localStorage.removeItem('fromTrack');
  }

  private _loadTrackInfo() {
    const trackStr = localStorage.getItem('fromTrack');
    if (trackStr) {
      this.track = JSON.parse(trackStr);
      if (this.track.eventsConfiguration &&
        this.track.eventsConfiguration.length > 0 &&
        this.track.eventsConfiguration.find(x => x.eventId === this.eventId && x.eventScheduleId === this.scheduleId)
      ) {
        this.fromTrack = true;
      }
    }
  }

  public getFormattedHour(): string {
    if (!this.track || !this.track.duration) return '--';
    return this._utilService.formatSecondsToHourMinute(this.track.duration);
  }

  private _loadApplications(): void {
    this._eventsService.getEventsApplicationsByEventId(
      this.eventId, this.scheduleId
    ).subscribe((response) => {
      console.log('response - _loadApplications -> ', response);
      this.users = response.data.applications;
      this.pendingPresence = (this.users.filter(x => x.userPresence === null)).length;
      if (this.users && this.users.length > 0 &&
        this.users[0].customEventGradeValues && this.users[0].customEventGradeValues.length > 0) {
        this.customGrades = true;
      }
      this.event = response.data.event;
      if (this.event.schedules && this.event.schedules.length > 0) {
        this.schedule = this.event.schedules.find(x => x.id === this.scheduleId);
        this.isFinished = this.schedule.finishedAt ? true : false;
      }
      this._startUsers = this.users.slice();
    }, () => this.notify('Ocorreu um erro, por favor tente novamente mais tarde'));

    const trackPreview = localStorage.getItem('track-responsible');
    if (trackPreview) {
      this.noActions = true;
    }
  }

  private _loadSchedules(): void {
    this._eventsService.getEventSchedulesByEventId(
      this.eventId
    ).subscribe((response) => {
      this.schedules = response.data;
    }, () => this.notify('Ocorreu um erro, por favor tente novamente mais tarde'));
  }

  public changeScheduleId() {
    this._loadApplications();
  }

  public getPresenceImgSrc(application: EventApplication, presence: boolean): string {
    if (application.userPresence == null)
      return presence ? './assets/img/approved-disabled.png' : './assets/img/denied-disabled.png';

    return application.userPresence ?
      (presence ? './assets/img/approved.png' : './assets/img/denied-disabled.png') :
      (presence ? './assets/img/approved-disabled.png' : './assets/img/denied.png');
  }

  public getCheckImgSrc(check: boolean): string {
    return check ? './assets/img/approved.png' : './assets/img/denied.png';
  }

  public canFinishClass(): boolean {
    if (this.noActions) {
      return this.noActions;
    } else if (this.isFinished) {
      return this.isFinished;
    } else {
      if (this.customGrades) {
        return this.users.some(uAp =>
          uAp.userPresence === null ||
          (
            uAp.userPresence === true &&
            (
              uAp.customEventGradeValues.some(x => x.grade === null) ||
              uAp.customEventGradeValues.some(x => x.grade < 0) ||
              uAp.customEventGradeValues.some(x => x.grade > 10)
            )
          )
        );
      } else {
        return this.users.some(uAp =>
          uAp.userPresence === null ||
          (
            uAp.userPresence === true &&
            (
              (uAp.organicGrade == null || uAp.organicGrade < 0) ||
              (uAp.inorganicGrade == null || uAp.inorganicGrade < 0)
            )
          )
        );
      }
    }
  }

  public finishClass(): void {
    this._eventsService.finishEvent(
      this.eventId, this.scheduleId, this.users.filter(x => x.userPresence).map(x => x.user.id)
    ).subscribe(
      () =>  {
        this.notify('Evento finalizado com sucesso');
        this._router.navigate([ '/configuracoes/gerenciar-eventos/' + this.eventId ]);
      },
      () => this.notify('Ocorreu um erro, por favor tente novamente mais tarde')
    );
  }

  public disableSendEmails() {
    if (this.noActions) {
      return this.noActions;
    } else if (this.schedule && this.schedule.sentReactionEvaluationEmails) {
      return true;
    } else {
      return this.users.some(x => x.userPresence === null);
    }
  }

  public sendReactionValuation(userId: string = '') {
    const usersId = userId ? [userId] : this.users.filter(x => x.userPresence).map(x => x.user.id);
    this._eventsService.sendEventEvaluationEmail(
      this.eventId, this.scheduleId, usersId
    ).subscribe(
      () => {
        this.notify('Enviado com sucesso');
        this.schedule.sentReactionEvaluationEmails = true;
      },
      () => this.notify('Ocorreu um erro, por favor tente novamente mais tarde')
    );
  }

  public changeGrade(application: EventApplication): void {
    application.grading = !application.grading;
    this.expandedElement = application;
    if (this.customGrades) {
      if (!application.grading && !application.customEventGradeValues.some(x => x.grade === null)) {
        application.customEventGradeValues.some(x => x.grade < 0) || application.customEventGradeValues.some(x => x.grade > 10) ?
          this.notify('As notas devem ter valores positivos entre 0 e 10') :
          this._updateUserGrade( application );
      }
    } else {
      if (!application.grading && application.organicGrade != null && application.inorganicGrade != null) {
        application.organicGrade < 0 || application.inorganicGrade < 0 ?
          this.notify('As notas devem ter valores positivos') :
          this._updateUserGrade( application );
      }
    }
  }

  public changeForumGrade(application: EventApplication): void {
    application.forumGrading = !application.forumGrading;
    this.expandedForumElement = application;

    if (!application.forumGrading && application.forumGrade != null ) {
      application.forumGrade < 0 ?
        this.notify('A nota deve ter valor positivo') :
        this._updateUserForumGrade( application );
    }
  }

  public viewUserForumParticipation(userId: string) {
    this._eventForumService.getUserEventForumByEventSchedule(this.scheduleId, userId).subscribe(res => {
      this._dialog.open(EventForumParticipationDialogComponent, {
        width: '1000px',
        data: res.data
      });
    }, err => {
      this.notify(this.getErrorNotification(err));
    });
  }

  public manageUserPresence(application: EventApplication, presence: boolean): void {
    application.userPresence = presence;
    this._eventsService.manageUserPresence(
      application.id, application.userPresence
    ).subscribe(
      () =>  this.notify('Presença atualizada com sucesso!'),
      () => this.notify('Ocorreu um erro, por favor tente novamente mais tarde')
    );
  }

  private _updateUserGrade(application: EventApplication): void {
    this._eventsService.updateEventUserGrade(
      application.id,
      application.organicGrade,
      application.inorganicGrade,
      application.customEventGradeValues
    ).subscribe(() => {
      this.notify('Aluno avaliado com sucesso!');
    }, () => {
      this.notify('Ocorreu um erro, por favor tente novamente mais tarde');
    });
  }

  private _updateUserForumGrade(application: EventApplication): void {
    this._eventsService.updateEventUserForumGrade(
      application.id,
      application.forumGrade,
    ).subscribe(() => {
      this.notify('Aluno avaliado com sucesso!');
    }, () => {
      this.notify('Ocorreu um erro, por favor tente novamente mais tarde');
    });
  }

  public triggerSearch(searchValue: string) {
    if (searchValue) {
      this.users = this._startUsers.filter(ap => {
        return ap.user.name.toLocaleLowerCase().includes(
          searchValue.toLowerCase()
        );
      });
    } else {
      this.users = this._startUsers.slice();
    }
  }

  public sortData(sort: Sort) {
    switch (sort.direction) {
      case 'asc':
          this.users = this._startUsers.sort(function(a, b) {
            const textA = a.user.name.toLowerCase();
            const textB = b.user.name.toLowerCase();
            return (textA < textB) ? -1 : (textA > textB) ? 1 : 0;
          });
          break;
      case 'desc':
          this.users = this._startUsers.sort(function(a, b) {
            const textA = a.user.name.toLowerCase();
            const textB = b.user.name.toLowerCase();
            return (textA > textB) ? -1 : (textA < textB) ? 1 : 0;
          });
          break;
      default:
        this.users = this._startUsers.slice();
    }
  }

  public hasEmptyCustomGrade(application: EventApplication): boolean {
    return application.customEventGradeValues.some(x => x.grade === null);
  }

  public sumGrades(application: EventApplication): number {
    let finalGrade = 0;
    const grades = application.customEventGradeValues.map(x => x.grade);
    grades.forEach(grade => {
      finalGrade += +grade;
    });
    return finalGrade / application.customEventGradeValues.length;
  }

  public changeEventDateDialog(userId: string) {
    const dialogConfig = new MatDialogConfig();

    dialogConfig.data = {
        allSchedules: this.eventsInfo,
        event: this.event,
        scheduleId: this.scheduleId
    };

    const dialogRef = this._dialog.open(EventsChangeDialogComponent, {
      width: '1000px',
      data: dialogConfig
    });

    dialogRef.afterClosed().subscribe((result: any) => {
      if (result) {
        this._eventsService.changeUserEventApplicationSchedule(userId, this.eventId,
          this.scheduleId, result)
        .subscribe(() => {
          this._loadApplications();
        }, () => this.notify('Ocorreu um erro, por favor tente novamente mais tarde'));
      }
    });
  }

  private _loadEvents(): void {
    const eventId = this._activatedRoute.snapshot.paramMap.get('eventId');
    this._eventsService.getEventById(eventId)
    .subscribe((response) => {
      this.events = response.data.events;
      console.log('this.events -> ', this.events);
    }, () => this.notify('Ocorreu um erro, por favor tente novamente mais tarde'));
  }

  private _loadAllEventsByUser(): void {
    this._eventsService.getEventSchedulesByEventId(this.eventId).subscribe((response) => {
      this.eventsInfo = response.data;
    }, () => this.notify('Ocorreu um erro, por favor tente novamente mais tarde'));
  }

  public canImportGrades(): boolean {
    const loggedUser = this._authService.getLoggedUser();
    return loggedUser && loggedUser.role === 'Admin';
  }

  public setDocumentFile(files: FileList) {
    const file = files.item(0);
    const callback = this._sendToServer.bind(this);
    const reader = new FileReader();
    reader.onloadend = function (e) {
      callback(
        this.result as string,
        file.name,
        file.name,
        file.name
      );
    };
    reader.readAsDataURL(file);
  }

  private _sendToServer(result: string) {
    this._eventsContentService.postEventUsersGrade(this.eventId, this.scheduleId, result).subscribe(() => {
      this.notify('Arquivo enviado com sucesso!');
    }, (err) => {
      this.notify(this.getErrorNotification(err));
    });
  }

  public openParticipation(eventApp: EventApplication) {
    this._dialog.open(EventApplicationNoteDialogComponent, {
      width: '1000px',
      data: {message: eventApp.transcribedParticipation}
    });
  }

  public generateStudentList() {
    this._eventsContentService.getEventStudentList(
      this.eventId, this.scheduleId
    ).subscribe((response) => {
      const studentList = [];

      for (let i = 0; i < response.data.length; i++) {

        studentList.push({
          'Id': response.data[i].userId,
          'Nome': response.data[i].name,
          'Email': response.data[i].email,
          'Data_Realização': response.data[i].eventDate,
          'Case': response.data[i].event,
          'Grupo': response.data[i].group,
        });

        for (let k = 0; k < response.data[i].gradeBaseValues.length; k++) {
          studentList[i][response.data[i].gradeBaseValues[k].key] = response.data[i].gradeBaseValues[k].value;
        }
      }

      this._excelService.exportAsExcelFile(studentList, 'Template_Notas_Evento_' + this.event.title);

    }, () => this.notify('Ocorreu um erro, por favor tente novamente mais tarde'));
  }
}
