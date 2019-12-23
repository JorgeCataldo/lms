import { Injectable } from '@angular/core';
import { BackendService } from '@tg4/http-infrastructure/dist/src';
import { Observable, of } from 'rxjs';
import { User } from '../users/user-models/user';
import { Recommendations } from '../users/user-models/user-track';
import { UserCategoryFilterSearchOption } from '../manage-team/filters/filters.interface';
import { CategoryEnum } from 'src/app/models/enums/category.enum';
import { environment } from 'src/environments/environment';
import { UserFile } from 'src/app/models/user-file.interface';
import { UserCareer } from '../users/user-models/user-career';
import { ColorKey } from 'src/app/models/color-key';
import { ActivationTypeEnum } from 'src/app/models/enums/activation-status.enum';
import { Activation } from 'src/app/models/activation.model';

@Injectable()
export class SettingsUsersService {

  constructor(private _httpService: BackendService) { }

  public getPagedFilteredUsersList(page: number = 1, pageSize: number = 10,
    searchValue: string = '', blocked: boolean = false, sort: string = '', sortAscending: boolean = false,
    categoryFilters: Array<UserCategoryFilterSearchOption> = [], dependent: string = '' ): Observable<any> {
    return this._httpService.post('getPagedUser', {
      'page': page,
      'pageSize': pageSize,
      'filters': {
        'name': searchValue,
        'blocked': blocked,
        'sortBy': sort,
        'isSortAscending': sortAscending,
        'categoryFilter': categoryFilters,
        'dependent': dependent
      }
    });
  }

  public getFilteredUserToManage(
    page: number = 1, pageSize: number = 10, searchValue: string = '', createdSince: Date = null,
    categoryFilters: Array<UserCategoryFilterSearchOption> = [], dependent: string = '',
    sort: string = '', sortAscending: boolean = false, selectAllUsers: boolean = false
  ): Observable<any> {
    return this._httpService.post('getFilteredPagedUser', {
      'page': page,
      'pageSize': pageSize,
      'filters': {
        'term': searchValue,
        'categoryFilter': categoryFilters,
        'dependent': dependent,
        'createdSince': createdSince,
        'sortBy': sort,
        'isSortAscending': sortAscending
      },
      'selectAllUsers': selectAllUsers
    });
  }

  public getUserById(userId: string): Observable<any> {
    return this._httpService.post('getUserById', {
      'id': userId
    });
  }

  public getUserProfile(userId: string): Observable<any> {
    return this._httpService.post('getUserProfile', {
      'id': userId
    });
  }

  public getUserRecommendation(userId: string): Observable<any> {
    return this._httpService.get('getUserRecommendationBasicInfo', [], [
      { 'name': 'userId', 'value': userId }
    ]);
  }

  public createUpdateUser(user: User): Observable<any> {
    return this._httpService.post('addOrModifyUser', {
      'id': user.id,
      'role': user.role,
      'name': user.name,
      'registrationId': user.registrationId,
      'dateBorn': user.dateBorn,
      'cpf': user.cpf,
      'userName': user.userName,
      'password': user.password ? user.password : '',
      'responsible': user.responsibleId,
      'lineManager': user.lineManager,
      'linkedIn': user.linkedIn,
      'info': user.info,
      'imageUrl': user.imageUrl,
      'email': user.email,
      'phone': user.phone,
      'phone2': user.phone2,
      'businessGroup': user.businessGroup,
      'businessUnit': user.businessUnit,
      'country': user.country,
      'frontBack': user.frontBackOffice,
      'job': user.job,
      'location': user.location,
      'rank': user.rank,
      'sector': user.sectors,
      'segment': user.segment,
      'responsibleId': user.responsibleId,
      'autoGenerateRegistrationId': environment.features.autoGenerateRegistrationId,
      'address': user.address,
      'specialNeeds': user.specialNeeds,
      'specialNeedsDescription': user.specialNeedsDescription,
      'document': user.document,
      'documentNumber': user.documentNumber,
      'documentEmitter': user.documentEmitter,
      'emitDate': user.emitDate,
      'expirationDate': user.expirationDate,
      'forumActivities': user.forumActivities,
      'forumEmail': user.forumEmail
    });
  }

  public getUserCategory(
    category: CategoryEnum, searchTerm: string = '', page: number = 1, pageSize: number = 10
  ): Observable<any> {
    return this._httpService.get('getUserCategories', [], [
      { 'name': 'category', 'value': category.toString() },
      { 'name': 'searchTerm', 'value': searchTerm },
      { 'name': 'page', 'value': page.toString() },
      { 'name': 'pageSize', 'value': pageSize.toString() }
    ]);
  }

  public changeUserBlockedStatus(id: string): Observable<any> {
    return this._httpService.post('changeUserBlockedStatus', {'id': id});
  }

  public getPagedFilteredUsersSyncProcesses(page: number = 1, pageSize: number = 10,
     sort: string = '', sortAscending: boolean = false, fromDate: number = null, toDate: number = null): Observable<any> {
    return this._httpService.post('pagedUsersSyncProcesse', {
      'page': page,
      'pageSize': pageSize,
      'filters': {
        'fromDate': fromDate,
        'toDate': toDate,
        'sortBy': sort,
        'isSortAscending': sortAscending
      }
    });
  }

  public postUsersSyncProcess(fileToUpload: any): Observable<any> {
    return this._httpService.post('addUsersSyncProcess', fileToUpload);
  }

  public updateUserRecommendations(recommendations: Array<Recommendations>): Observable<any> {
    return this._httpService.post('updateUserRecommendations', {
      'recommendations': recommendations
    });
  }

  public changePassword(currentPass: string, newPass: string): Observable<any> {
    return this._httpService.post('changePassword', {
      'currentPassword': currentPass,
      'newPassword': newPass
    });
  }

  public adminChangePassword(userId: string, newPass: string): Observable<any> {
    return this._httpService.post('adminChangePassword', {
      'userId': userId,
      'newPassword': newPass
    });
  }

  public getProfessors(name: string): Observable<any> {
    return this._httpService.get('getProfessors', [], [
      { name: 'term', value: name }]);
  }

  public getUserArchive(userId: string): Observable<any> {
    return this._httpService.get('getUserFiles', [], [
      { 'name': 'filesUserId', 'value': userId }
    ]);
  }

  public manageUserFiles(filesUserId: string, userFiles: Array<UserFile>): Observable<any> {
    return this._httpService.put('manageUserFiles', {
      'filesUserId': filesUserId,
      'userFiles': userFiles
    });
  }

  public addUserFiles(filesUserId: string, userFiles: Array<UserFile>): Observable<any> {
    return this._httpService.post('addUserFiles', {
      'filesUserId': filesUserId,
      'userFiles': userFiles
    });
  }
  public addAssesmentUserFiles(filesUserId: string, userFiles: Array<UserFile>): Observable<any> {
    return this._httpService.post('addAssesmentUserFiles', {
      'filesUserId': filesUserId,
      'userFiles': userFiles
    });
  }

  public getPagedCustomEmails(page: number = 1, pageSize: number = 10,
    sort: string = '', sortAscending: boolean = false): Observable<any> {
   return this._httpService.post('pagedCustomEmails', {
     'page': page,
     'pageSize': pageSize,
     'filters': {
       'sortBy': sort,
       'isSortAscending': sortAscending
     }
   });
 }

  public sendCustomEmail(usersId: string[], title: string, text: string): Observable<any> {
    return this._httpService.post('sendCustomEmail', {
      'usersIds': usersId,
      'title': title,
      'text': text
    });
  }

  public getUserContentNote(moduleId: string, subjectId: string,
    contentId: string): Observable<any> {
    return this._httpService.get('getUserContentNote', [], [
      { name: 'moduleId', value: moduleId },
      { name: 'subjectId', value: subjectId },
      { name: 'contentId', value: contentId }
    ]);
  }

  public updateUserContentNote(moduleId: string, subjectId: string,
    contentId: string, note: string): Observable<any> {
    return this._httpService.post('updateUserContentNote', {
      'moduleId': moduleId,
      'subjectId': subjectId,
      'contentId': contentId,
      'note': note
    });
  }

  public getUserCareer(userId: string): Observable<any> {
    return this._httpService.get('getUserCareer', [], [
      { name: 'userId', value: userId }
    ]);
  }

  public updateUserCareer(userId: string, career: UserCareer): Observable<any> {
    return this._httpService.post('updateUserCareer', {
      'userId': userId,
      'career': career,
    });
  }

  public getUserInstitutions(name: string): Observable<any> {
    return this._httpService.get('getUserInstitutions', [], [
      { name: 'name', value: name }
    ]);
  }

  public exportUsersCareer(users: any[]): Observable<any> {
    return this._httpService.post('exportUsersCareer', {
      'users': users,
    });
  }

  public exportUsersGrade(userIds: string[]): Observable<any> {
    return this._httpService.post('exportUsersGrade', {
      'userIds': userIds,
    });
  }

  public exportUsersEffectiveness(users: any[]): Observable<any> {
    return this._httpService.post('exportUsersEffectivenessIndicators', {
      'users': users,
    });
  }

  public generateResponsibleTree(): Observable<any> {
    return this._httpService.post('createResponsibleTree', {});
  }

  public getUserSkills(userId: string): Observable<any> {
    return this._httpService.get('getUserSkills', [], [
      { 'name': 'studentId', 'value': userId }
    ]);
  }

  public allowRecommendation(userId: string, allow: boolean): Observable<any> {
    return this._httpService.put('allowRecommendation', {
      'allowRecommendation': allow,
      'userId': userId
    });
  }

  public allowSecretaryRecommendation(userId: string, allow: boolean): Observable<any> {
    return this._httpService.put('allowSecretaryRecommendation', {
      'allowRecommendation': allow,
      'userId': userId
    });
  }

  public getAllLocations(): Observable<any> {
    return this._httpService.get('getAllLocations');
  }
  public getUserColorPalette(): Observable<any> {
    return this._httpService.get('getUserColorPalette', [], []);
  }

  public updateUserColorPalette(colorValues: ColorKey[]): Observable<any> {
    return this._httpService.post('updateUserColorPalette', {
      'colorBaseValues': colorValues
    });
  }

  public getUserNpsInfos(page: number = 1, pageSize: number = 10, name: string = '',
    sort: string = '', sortAscending: boolean = false): Observable<any> {
    return this._httpService.post('getUserNpsInfos', {
      'page': page,
      'pageSize': pageSize,
      'filters': {
        'name': name,
        'sortBy': sort,
        'isSortAscending': sortAscending
      }
    });
 }

  public saveNpsValuation(grade: number): Observable<any> {
    return this._httpService.post('saveNpsValuation', {
      'grade': grade
    });
  }

  public getAllUserNpsInfos(): Observable<any> {
    return this._httpService.get('getAllNpsInfos', [], []);
  }

  public getActivationStatus(type: ActivationTypeEnum): Observable<any> {
    return this._httpService.get('getActivationStatus', [], [
      { name: 'type', value: type.toString() }
    ]);
  }

  public updateActivationStatus(active: Activation, type: ActivationTypeEnum): Observable<any> {
    return this._httpService.post('updateNpsActivation', {
      'type': type,
      'active': active.active,
      'title': active.title,
      'text': active.text,
      'percentage': active.percentage
    });
  }

  public getUserNpsAvailability(): Observable<any> {
    return this._httpService.get('getUserNpsAvailability', [], []);
  }

  public exportCareerUsers(): Observable<any> {
    return this._httpService.get('exportCareerUsers');
  }

  public getUserToImpersonate(userId: string): Observable<any> {
    return this._httpService.get('getUserToImpersonate', [], [
      { 'name': 'userId', 'value': userId }
    ]);
  }
}
