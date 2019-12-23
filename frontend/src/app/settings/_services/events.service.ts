import { Injectable } from '@angular/core';
import { BackendService } from '@tg4/http-infrastructure/dist/src';
import { Observable, of } from 'rxjs';
import { Event } from '../../models/event.model';
import { SupportMaterial } from '../../models/support-material.interface';
import { Requirement } from '../modules/new-module/models/new-requirement.model';
import { EventSchedule } from '../../models/event-schedule.model';
import { EventReaction } from 'src/app/models/event-valuation.model';
import { CustomEventGradeValue } from 'src/app/models/event-application.model';

@Injectable()
export class SettingsEventsService {

  constructor(private _httpService: BackendService) { }

  public getPagedFilteredEventsList(
    page: number = 1, pageSize: number = 10, searchValue: string = '', withSchedule: boolean = false
  ): Observable<any> {
    return this._httpService.post('getPagedFilteredEventsList', {
      'page': page,
      'pageSize': pageSize,
      'filters': {
        'term': searchValue,
        'withSchedule': withSchedule
      }
    });
  }

  public getEventById(eventId: string, simpleQuery = 'false'): Observable<any> {
    return this._httpService.get('getEventById', [], [{
      'name': 'id', 'value': eventId
    }, {
      'name': 'simpleQuery', 'value': simpleQuery
    }]);
  }

  public manageEventInfo(event: Event): Observable<any> {
    return this._httpService.post('manageEvent', event);
  }

  public manageEventSchedule(eventSchedule: EventSchedule): Observable<any> {
    return this._httpService.post('manageEventSchedule', eventSchedule);
  }

  public manageSupportMaterials(eventId: string, materials: Array<SupportMaterial>): Observable<any> {
    return this._httpService.post('manageEventSupportMaterials', {
      'eventId': eventId,
      'deleteNonExistent': true,
      'supportMaterials': materials
    });
  }

  public manageRequirements(eventId: string, requirements: Array<Requirement>): Observable<any> {
    return this._httpService.post('manageEventRequirements', {
      'eventId': eventId,
      'requirements': requirements
    });
  }

  public getEventsApplicationsByEventId(eventId: string, scheduleId: string): Observable<any> {
    return this._httpService.post('getEventApplicationByEventId', {
      'eventId': eventId,
      'scheduleId': scheduleId
    });
  }

  public getEventSchedulesByEventId(eventId: string): Observable<any> {
    return this._httpService.post('getEventSchedulesByEventId', {
      'eventId': eventId
    });
  }

  public getEventsApplicationsAnswersByEventId(eventId: string, scheduleId: string): Observable<any> {
    return this._httpService.post('GetEventPrepAnswersByIdQuery', {
      'eventId': eventId,
      'scheduleId': scheduleId
    });
  }

  public updateEventUserApplicationStatus(eventId: string, scheduleId: string, userId: string, applicationStatus: number): Observable<any> {
    return this._httpService.post('changeEventUserApplicationStatus', {
      'eventId': eventId,
      'eventScheduleId': scheduleId,
      'userId': userId,
      'applicationStatus': applicationStatus
    });
  }

  public updateEventUserGrade(applicationId: string, organicGrade: number,
    inorganicGrade: number, customEventGradeValues: CustomEventGradeValue[]): Observable<any> {
    return this._httpService.post('changeEventUserGrade', {
      'eventApplicationId': applicationId,
      'organicGrade': organicGrade,
      'inorganicGrade': inorganicGrade,
      'customEventGradeValues': customEventGradeValues
    });
  }

  public updateEventUserForumGrade(applicationId: string, forumGrade: number): Observable<any> {
    return this._httpService.post('changeEventUserForumGrade', {
      'eventApplicationId': applicationId,
      'forumGrade': forumGrade
    });
  }

  public changeEventScheduleStatus(eventId: string, eventScheduleId: string): Observable<any> {
    return this._httpService.post('changeEventPublishedStatus', {
      'eventId': eventId,
      'EventScheduleId': eventScheduleId
    });
  }

  public deleteEventById(id: string): Observable<any> {
    return this._httpService.post('deleteEventById', {
      'eventId': id
    });
  }

  public addEventReaction(reaction: EventReaction): Observable<any> {
    return this._httpService.post('addEventReaction', {
      'eventId': reaction.eventId,
      'eventScheduleId': reaction.eventScheduleId,
      'didactic': reaction.didactic,
      'classroomContent': reaction.classroomContent,
      'studyContent': reaction.studyContent,
      'theoryAndPractice': reaction.theoryAndPractice,
      'usedResources': reaction.usedResources,
      'evaluationFormat': reaction.evaluationFormat,
      'expectation': reaction.expectation,
      'suggestions': reaction.suggestions
    });
  }

  public getEventReactions(
    eventId: string, scheduleId: string, page: number = 1, pageSize: number = 10
  ): Observable<any> {
    return this._httpService.get('getEventReactions', [], [
      { 'name': 'eventId', 'value': eventId },
      { 'name': 'eventScheduleId', 'value': scheduleId },
      { 'name': 'page', 'value': page.toString() },
      { 'name': 'pageSize', 'value': pageSize.toString() }
    ]);
  }

  public manageEventReaction(reactionId: string, approved: boolean) {
    return this._httpService.put('manageEventReaction', {
      'eventReactionId': reactionId,
      'approved': approved
    });
  }

  public manageUserPresence(applicationId: string, presence: boolean) {
    return this._httpService.put('manageUserPresence', {
      'eventApplicationId': applicationId,
      'presence': presence
    });
  }

  public finishEvent(eventId: string, eventScheduleId: string, usersId: string[]): Observable<any> {
    return this._httpService.post('finishEvent', {
      'EventId': eventId,
      'eventScheduleId': eventScheduleId,
      'usersId': usersId
    });
  }

  public getPastEvents(page: number = 1, searchTerm: string = ''): Observable<any> {
    return this._httpService.get('getPastEvents', [], [
      { 'name': 'page', 'value': page.toString() },
      { 'name': 'searchTerm', 'value': searchTerm }
    ]);
  }

  public getUserType(page: number = 1, searchTerm: string = ''): Observable<any> {
    return this._httpService.get('getUserType', [], [
      { 'name': 'page', 'value': page.toString() },
      { 'name': 'searchTerm', 'value': searchTerm }
    ]);
  }

  public sendEventEvaluationEmail(eventId: string, eventScheduleId: string, usersId: string[]): Observable<any> {
    return this._httpService.post('sendEventEvaluationEmail', {
      'EventId': eventId,
      'eventScheduleId': eventScheduleId,
      'usersId': usersId
    });
  }

  public getAllEventsByUser(): Observable<any> {
    return this._httpService.get('getAllEventsByUser');
  }

  public changeUserEventApplicationSchedule(userId: string, eventId: string,
    scheduleId: string, newScheduleId: string): Observable<any> {
    return this._httpService.post('changeUserEventApplicationSchedule', {
      'userId': userId,
      'eventId': eventId,
      'scheduleId': scheduleId,
      'newScheduleId': newScheduleId
    });
  }
}
