import { Component, OnInit } from '@angular/core';
import { MatSnackBar } from '@angular/material';
import { ActivatedRoute } from '@angular/router';
import { NotificationClass } from '../../shared/classes/notification';
import { EventApplication } from '../../models/event-application.model';
import { SettingsEventsService } from '../_services/events.service';
import { ApplicationStatus } from 'src/app/models/enums/application-status';
import { UrlResolver } from '@angular/compiler';
import { ExcelService } from 'src/app/shared/services/excel.service';

@Component({
  selector: 'app-settings-events-applications',
  templateUrl: './events-applications.component.html',
  styleUrls: ['./events-applications.component.scss']
})
export class SettingsEventsApplicationsComponent extends NotificationClass implements OnInit {

  public pendingApplications: Array<EventApplication> = [];
  public approvedApplications: Array<EventApplication> = [];
  public deniedApplications: Array<EventApplication> = [];
  public eventInfo: any = {};

  private _applications: Array<EventApplication> = [];

  constructor(
    protected _snackBar: MatSnackBar,
    private _eventsService: SettingsEventsService,
    private _excelService: ExcelService,
    private _activatedRoute: ActivatedRoute
  ) {
    super(_snackBar);
  }

  ngOnInit() {
    const eventId = this._activatedRoute.snapshot.paramMap.get('eventId');
    const scheduleId = this._activatedRoute.snapshot.paramMap.get('scheduleId');
    this._loadApplications(eventId, scheduleId);
  }

  private _loadApplications(eventId: string, scheduleId: string): void {
    this._eventsService.getEventsApplicationsByEventId(eventId, scheduleId).subscribe((response) => {
      const event = response.data.event;
      const schedule = event.schedules.find(x => x.id === scheduleId);
      this.eventInfo.eventTitle = event.title;
      this.eventInfo.eventDate = schedule.eventDate;
      this.eventInfo.scheduleStartDate = schedule.subscriptionStartDate;
      this.eventInfo.scheduleEndDate = schedule.subscriptionEndDate;
      this.eventInfo.percentage = (
        (new Date().getTime() - new Date(schedule.subscriptionStartDate).getTime()) /
        (new Date(schedule.subscriptionEndDate).getTime() - new Date(schedule.subscriptionStartDate).getTime())
      ) * 100;
      this._applications = response.data.applications;
      this._setApplicationsByStatus(this._applications);
    }, () => this.notify('Ocorreu um erro, por favor tente novamente mais tarde'));
  }

  private _setApplicationsByStatus(applications: Array<EventApplication>): void {
    this.pendingApplications = applications.filter(a => a.applicationStatus === ApplicationStatus.Pending);
    this.approvedApplications = applications.filter(a => a.applicationStatus === ApplicationStatus.Approved);
    this.deniedApplications = applications.filter(a => a.applicationStatus === ApplicationStatus.Rejected);
  }

  public exportAnswers() {
    const excelModule = [];
    for (let useridx = 0; useridx < this._applications.length; useridx++) {
      const user = this._applications[useridx];
      if (user.prepQuizAnswers && user.prepQuizAnswers.length > 0) {
        for (let idx = 0; idx < user.prepQuizAnswers.length; idx++) {
          const answer = user.prepQuizAnswers[idx];
          excelModule.push({
            user: user.user.name, question: user.prepQuiz.questions[idx],
            answer: answer, applicationStatus: user.applicationStatus
          });
        }
      }
    }
    this._excelService.exportAsExcelFile(excelModule, 'Respostas');
  }

  public approveUser(userId: string) {
    const pendingApplications = this.pendingApplications.slice();
    const approvedApplications = this.approvedApplications.slice();
    const deniedApplications = this.deniedApplications.slice();
    let index = pendingApplications.findIndex(x => x.user.id === userId);
    if (index !== -1) {
      const removedItem = pendingApplications.splice(index, 1)[0];
      removedItem.applicationStatus = ApplicationStatus.Approved;
      this.pendingApplications = pendingApplications;
      approvedApplications.push(removedItem);
      this.approvedApplications = approvedApplications;
    } else {
      index = deniedApplications.findIndex(x => x.user.id === userId);
      if (index !== -1) {
        const removedItem = deniedApplications.splice(index, 1)[0];
        removedItem.applicationStatus = ApplicationStatus.Approved;
        this.deniedApplications = deniedApplications;
        approvedApplications.push(removedItem);
        this.approvedApplications = approvedApplications;
      }
    }
  }

  public denyUser(userId: string) {
    const pendingApplications = this.pendingApplications.slice();
    const approvedApplications = this.approvedApplications.slice();
    const deniedApplications = this.deniedApplications.slice();
    let index = pendingApplications.findIndex(x => x.user.id === userId);
    if (index !== -1) {
      const removedItem = pendingApplications.splice(index, 1)[0];
      removedItem.applicationStatus = ApplicationStatus.Rejected;
      this.pendingApplications = pendingApplications;
      deniedApplications.push(removedItem);
      this.deniedApplications = deniedApplications;
    } else {
      index = approvedApplications.findIndex(x => x.user.id === userId);
      if (index !== -1) {
        const removedItem = approvedApplications.splice(index, 1)[0];
        removedItem.applicationStatus = ApplicationStatus.Rejected;
        this.approvedApplications = approvedApplications;
        deniedApplications.push(removedItem);
        this.deniedApplications = deniedApplications;
      }
    }
  }
}
