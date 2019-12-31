import { Injectable } from '@angular/core';
import { BackendService } from '@tg4/http-infrastructure/dist/src';
import { Observable } from 'rxjs';
import { Event } from 'src/app/models/event.model';
import { EventSchedule } from 'src/app/models/event-schedule.model';
import { SupportMaterial } from 'src/app/models/support-material.interface';
import { Requirement } from '../modules/new-module/models/new-requirement.model';

@Injectable()
export class SettingsEventsDraftsService {

  constructor(private _httpService: BackendService) { }

  public getPagedEventsAndDrafts(
    page: number = 1, pageSize: number = 10, searchTerm: string = ''
  ): Observable<any> {
    return this._httpService.post('getPagedEventsAndDrafts', {
      'page': page,
      'pageSize': pageSize,
      'filters': {
        'term': searchTerm
      }
    });
  }

  public getEventDraftById(eventId: string): Observable<any> {
    return this._httpService.get('getEventDraftById', [], [{
      'name': 'id', 'value': eventId
    }]);
  }

  public addNewEventDraft(event: Event): Observable<any> {
    return this._httpService.post('addNewEventDraft', event);
  }

  public updateEventDraft(event: Event): Observable<any> {
    return this._httpService.put('addNewEventDraft', event);
  }

  public manageEventDraftSchedule(eventSchedule: EventSchedule): Observable<any> {
    return this._httpService.post('manageEventDraftSchedule', eventSchedule);
  }

  public manageEventDraftSupportMaterials(eventId: string, materials: Array<SupportMaterial>): Observable<any> {
    return this._httpService.post('manageEventDraftSupportMaterials', {
      'eventId': eventId,
      'deleteNonExistent': true,
      'supportMaterials': materials
    });
  }

  public manageEventDraftRequirements(eventId: string, requirements: Array<Requirement>): Observable<any> {
    return this._httpService.post('manageEventDraftRequirements', {
      'eventId': eventId,
      'requirements': requirements
    });
  }

  public publishEventDraft(eventId: string): Observable<any> {
    return this._httpService.get('publishEventDraft', [], [
      { 'name': 'eventId', 'value': eventId }
    ]);
  }

  public rejectEventDraft(eventId: string): Observable<any> {
    return this._httpService.post('rejectEventDraft', {
      'eventId': eventId
    });
  }
}
