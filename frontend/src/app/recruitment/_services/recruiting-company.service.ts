import { Injectable } from '@angular/core';
import { BackendService } from '@tg4/http-infrastructure/dist/src';
import { Observable } from 'rxjs';
import { RecruiterCompany } from 'src/app/models/recruiter-company.model';
import { UserCategoryFilterSearchOption } from 'src/app/settings/manage-team/filters/filters.interface';
import { JobPosition } from 'src/app/models/previews/user-job-application.interface';
import { Employment } from 'src/app/models/employment.interface';

@Injectable()
export class RecruitingCompanyService {

  constructor(private _httpService: BackendService) { }

  public getRecruitingCompany(): Observable<any> {
    return this._httpService.get('getRecruitingCompany');
  }

  public manageRecruiterCompany(company: RecruiterCompany): Observable<any> {
    return this._httpService.post('manageRecruitingCompany', {
      'company': company
    });
  }

  public addRecruitmentFavorite(userId: string): Observable<any> {
    return this._httpService.post('addRecruitmentFavorite', {
      'userId': userId
    });
  }

  public removeRecruitmentFavorite(userId: string): Observable<any> {
    return this._httpService.delete('removeRecruitmentFavorite', [{
      'name': 'userId', 'value': userId
    }]);
  }

  public getTalentsList(
    page: number = 1, pageSize: number = 10, searchValue: string = '',
    categoryFilters: Array<UserCategoryFilterSearchOption> = [],
    sort: string = '', sortAscending: boolean = false,
    mandatoryFields: boolean = false, selectAllUsers: boolean = false
  ): Observable<any> {
    return this._httpService.post('getTalentsList', {
      'page': page,
      'pageSize': pageSize,
      'filters': {
        'term': searchValue,
        'categoryFilter': categoryFilters,
        'sortBy': sort,
        'isSortAscending': sortAscending
      },
      'mandatoryFields': mandatoryFields,
      'selectAllUsers': selectAllUsers
    });
  }

  public getAllJobPositions(): Observable<any> {
    return this._httpService.get('getJobPositions');
  }

  public getJobPosition(jobPositionId: string): Observable<any> {
    return this._httpService.get('getJobPositionById', [],
      [{ name: 'jobPositionId', value: jobPositionId }]
    );
  }

  public deleteCandidateJobPosition(jobPositionId: string, userId: string): Observable<any> {
    return this._httpService.delete('deleteCandidateJobPosition',
      [
        { name: 'jobPositionId', value: jobPositionId },
        { name: 'userId', value: userId }
      ]
    );
  }

  public addJobPosition(title: string, dueTo: Date, priority: number,
    employment: Employment): Observable<any> {
    return this._httpService.post('addJobPosition', {
      'title': title,
      'dueTo': dueTo,
      'priority': priority,
      'employment': employment
    });
  }

  public updateJobPosition(jobPositionId: string, title: string, dueTo: Date,
    priority: number, employment: Employment): Observable<any> {
    return this._httpService.put('updateJobPosition', {
      'jobPositionId': jobPositionId,
      'title': title,
      'dueTo': dueTo,
      'priority': priority,
      'employment': employment
    });
  }

  public updateJobPositionStatus(jobPosition: JobPosition): Observable<any> {
    return this._httpService.put('updateJobPositionStatus', {
      'jobPositionId': jobPosition.id,
      'status': jobPosition.status
    });
  }

  public addCandidateToJobPosition(candidates: any[], jobPositionId: string): Observable<any> {
    return this._httpService.post('addCandidatesJobPosition', {
      'candidates': candidates,
      'jobPositionId': jobPositionId
    });
  }

  public approveCandidateToJobPosition(userId: string, jobPositionId: string): Observable<any> {
    return this._httpService.post('approveCandidateToJobPosition', {
      'userId': userId,
      'jobPositionId': jobPositionId
    });
  }

  public rejectCandidateToJobPosition(userId: string, jobPositionId: string): Observable<any> {
    return this._httpService.post('rejectCandidateToJobPosition', {
      'userId': userId,
      'jobPositionId': jobPositionId
    });
  }

  public getAvailableCandidates(): Observable<any> {
    return this._httpService.get('getAvailableCandidates');
  }

  public getUserJobPosition(): Observable<any> {
    return this._httpService.get('getUserJobPosition');
  }

  public approveUserJobPosition(userId: string, jobPositionId: string): Observable<any> {
    return this._httpService.post('approveUserJobPosition', {
      'candidateId': userId,
      'jobPositionId': jobPositionId
    });
  }

  public getJobsList(
    page: number = 1, pageSize: number = 10, searchValue: string = '',
    categoryFilters: Array<UserCategoryFilterSearchOption> = []
  ): Observable<any> {
    return this._httpService.post('getJobsList', {
      'page': page,
      'pageSize': pageSize,
      'filters': {
        'name': searchValue,
        'categoryFilter': categoryFilters
      }
    });
  }

  public applyTojob(jobPositionId: string): Observable<any> {
    return this._httpService.post('applyTojob', {
      'jobPositionId': jobPositionId
    });
  }

  public getUserJobPositionId(jobPositionId: string): Observable<any> {
    return this._httpService.get('getUserJobPositionsById', [],
      [{ name: 'jobPositionId', value: jobPositionId }]
    );
  }

  public getUserJobNotifications(): Observable<any> {
    return this._httpService.get('getUserJobNotifications');
  }

}
