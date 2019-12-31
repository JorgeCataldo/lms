import { Injectable } from '@angular/core';
import { BackendService } from '@tg4/http-infrastructure/dist/src';
import { Observable } from 'rxjs';

@Injectable()
export class UserService {

  constructor(private _httpService: BackendService) { }

  public getUserSubjectProgress(moduleId, subjectId): Observable<any> {
    return this._httpService.get('getsubjectprogress', [], [
      { name: 'moduleId', value: moduleId },
      { name: 'subjectId', value: subjectId }]);
  }

  public getUserModuleProgress(moduleId): Observable<any> {
    return this._httpService.get('getmoduleprogress', [], [
      { name: 'moduleId', value: moduleId }]);
  }

  public getModuleForumPreview(moduleId: string, pageSize: number): Observable<any> {
    return this._httpService.post('getForumPreviewByModule', {
      'moduleId': moduleId,
      'pageSize': pageSize
    });
  }

  public getEventForumPreview(eventScheduleId: string, pageSize: number): Observable<any> {
    return this._httpService.post('getEventForumPreviewByEventSchedule', {
      'eventScheduleId': eventScheduleId,
      'pageSize': pageSize
    });
  }

  public getUserModulesProgress(): Observable<any> {
    return this._httpService.get('getmodulesprogress');
  }

  public getUserTracksProgress(): Observable<any> {
    return this._httpService.get('gettracksprogress');
  }

  public getUserTrackProgress(trackId: string): Observable<any> {
    return this._httpService.get('getTrackProgress', [], [
      { name: 'trackId', value: trackId }]);
  }

  public getContactAreas(): Observable<any> {
    return this._httpService.get('getContactAreas');
  }

  public sendMessage(areaId: string, title: string, text: string, fileUrl: string): Observable<any> {
    return this._httpService.post('sendMessage', {
      'areaId': areaId,
      'title': title,
      'text': text,
      'fileUrl': fileUrl
    });
  }

  public blockUserMaterial(userId: string, trackId: string, moduleId: string, eventScheduleId: string): Observable<any> {
    return this._httpService.post('blockUserMaterial', {
      'userId': userId,
      'trackId': trackId,
      'moduleId': moduleId,
      'eventScheduleId': eventScheduleId
    });
  }

  public SeeHow(userRoleToChange: string): Observable<any> {
    return this._httpService.post('seeHow', {
      'userRoleToChange': userRoleToChange
    });
  }

  public getBasicProgressInfo(entityId: string) {
    return this._httpService.get('getBasicProgressInfo', [], [
      { name: 'entityId', value: entityId }]);
  }

  public getLevelColor(level: number) {
    switch (level) {
      case 1:
        return '#0369DB';
      case 2:
        return '#6F5EED';
      case 3:
        return '#3C3C3C';
      case 0:
      default:
        return '#06E295';
    }
  }

  public getLevelClass(level: number) {
    switch (level) {
      case 1:
        return '#0369DB';
      case 2:
        return '#6F5EED';
      case 3:
        return '#3C3C3C';
      case 0:
      default:
        return '#06E295';
    }
  }

  public getLevelGreyImage(level: number): string {
    switch (level) {
      case 1:
        return './assets/img/glasses-icon-gray.png';
      case 2:
      case 3:
        return './assets/img/brain-icon-gray.png';
      case 0:
      default:
        return './assets/img/pencil-icon-gray.png';
    }
  }
  public getLevelImage(level: number): string {
    switch (level) {
      case 1:
        return './assets/img/glasses-icon.png';
      case 2:
        return './assets/img/brain-icon.png';
      case 3:
        return './assets/img/brain-dark-icon-shadow.png';
      case 0:
      default:
        return './assets/img/pencil-icon.png';
    }
  }
  public getCompletedLevelImage(level: number, progress: number) {
    switch (level) {
      case 1:
        return './assets/img/pencil-icon.png';
      case 2:
        return './assets/img/glasses-icon.png';
      case 3:
        return './assets/img/brain-icon.png';
      case 4:
        return './assets/img/brain-dark-icon-shadow.png';
      case 0:
      default:
        return './assets/img/empty-badge.png';
    }
  }

}
