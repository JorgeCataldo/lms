import { Injectable, EventEmitter } from '@angular/core';
import { BackendService } from '@tg4/http-infrastructure/dist/src';
import { Observable, BehaviorSubject } from 'rxjs';
import { User } from '../../models/user.model';
import { Router } from '@angular/router';
import { FirstAccessInfo } from 'src/app/models/login.model';
import { environment } from '../../../environments/environment';
import { UserPreview } from 'src/app/models/previews/user.interface';
import { UserPreviewInterceptor } from '../interfaces/user-preview-interceptor.interface';

export class Result<T> {
  public data: T;
  public success: boolean;
  public errors: string[];
}

export class LoginResponse {
  public access_token: string;
  public expires_in: number;
  public refresh_token: string;
  public name: string;
  public username: string;
  public user_id: string;
  public role: string;
  public completed_registration: boolean;
  public first_access: boolean;
  public email_verified: boolean;
  public adminAccesses?: Array<string>;
  public have_dependents?: boolean;
}

@Injectable()
export class AuthService {

  public personificationChanged$ = new EventEmitter<boolean>();

  private localStorageKey = 'auth_data';
  private emailVerificationKey = 'email_verification';
  private isLoginSubject: BehaviorSubject<boolean>;
  private setColorPalette: BehaviorSubject<boolean>;

  constructor(private _httpService: BackendService,
    private router: Router) {
    const hasAuthData = this.hasToken();
    this.isLoginSubject = new BehaviorSubject<boolean>(hasAuthData);
    this.setColorPalette = new BehaviorSubject<boolean>(hasAuthData);
  }

  public async login(username: string, password: string): Promise<any> {
    let result: any = { success: false };
    try {
      result =
        await this._httpService.post<LoginResponse>('login', {
          'UserName': username,
          'Password': password
        }).toPromise();
    } catch (err) {
      if (err.error) {
        result = err.error;
      }
    }
    if (result.success) {
      if (result.data.user.logoUrl && result.data.user.logoUrl.length > 0) {
        localStorage.setItem('logo-url', result.data.user.logoUrl);
      }
      if (result.data.user.colorBaseValues && result.data.user.colorBaseValues.length > 0) {
        localStorage.setItem('color-palette', JSON.stringify(result.data.user.colorBaseValues));
        this.setColorPalette.next(true);
      }
      if (environment.features.firstLoginPersonalDataForm) {
        if (!result.data.tokenInfo.first_access && result.data.tokenInfo.email_verified) {
          result.data.tokenInfo.access_token = 'Bearer ' + result.data.tokenInfo.access_token;
          localStorage.setItem(this.localStorageKey, JSON.stringify(result.data.tokenInfo));
          this.isLoginSubject.next(true);
        } else {
          localStorage.setItem(this.emailVerificationKey, JSON.stringify({
            userId: result.data.tokenInfo.user_id,
            email: result.data.tokenInfo.email
          }));
        }
      } else {
        if (result.data.tokenInfo.email_verified) {
          result.data.tokenInfo.access_token = 'Bearer ' + result.data.tokenInfo.access_token;
          localStorage.setItem(this.localStorageKey, JSON.stringify(result.data.tokenInfo));
          this.isLoginSubject.next(true);
        } else {
          localStorage.setItem(this.emailVerificationKey, JSON.stringify({
            userId: result.data.tokenInfo.user_id,
            email: result.data.tokenInfo.email
          }));
        }
      }
    }
    return result;
  }

  public async ssoLogin(tokenInfo: LoginResponse) {
    tokenInfo.access_token = 'Bearer ' + tokenInfo.access_token;
    if (!tokenInfo.first_access || !environment.features.firstLoginPersonalDataForm) {
      localStorage.setItem(this.localStorageKey, JSON.stringify(tokenInfo));
      this.isLoginSubject.next(true);
    }
  }

  public async firstAccess(credentials: FirstAccessInfo): Promise<any> {
    let result: any = { success: false };
    try {
      result =
        await this._httpService.post<LoginResponse>('firstAccess', credentials).toPromise();
    } catch (err) {
      if (err.error) {
        result = err.error;
      }
    }
    if (result.success) {
     if (result.data.email_verified) {
          result.data.access_token = 'Bearer ' + result.data.access_token;
          localStorage.setItem(this.localStorageKey, JSON.stringify(result.data));
          this.isLoginSubject.next(true);
        } else {
          localStorage.setItem(this.emailVerificationKey, JSON.stringify({
            userId: result.data.user_id,
            email: result.data.email
          }));
        }
    }
    return result;
  }

  public logout(): void {
    localStorage.clear();
    this.isLoginSubject.next(false);
    this.setColorPalette.next(false);
    this.router.navigate(['']);
  }

  public isLoggedIn(): Observable<boolean> {
    return this.isLoginSubject.asObservable();
  }

  public setUserColorPalette(): Observable<boolean> {
    return this.setColorPalette.asObservable();
  }

  public updateColorPallete() {
    this.setColorPalette.next(true);
  }

  public async register(newUser: User): Promise<any> {
    let result: any = { success: false };
    try {
      result =
        await this._httpService.post<LoginResponse>('register', newUser).toPromise();
    } catch (err) {
      if (err.error) {
        result = err.error;
      }
    }
    if (result.success) {
      localStorage.setItem(this.emailVerificationKey, JSON.stringify({
        userId: result.data.user_id,
        email: result.data.email
      }));
    }
    return result;
  }

  public async sendVerificationEmail(newEmail: boolean = false): Promise<any> {
    let result: any = { success: false };
    try {
      const userId = JSON.parse(localStorage.getItem(this.emailVerificationKey)).userId;
      result =
        await this._httpService.post<LoginResponse>('sendVerificationEmail',
        {
          'userId': userId,
          'newEmail': newEmail
        }).toPromise();
    } catch (err) {
      if (err.error) {
        result = err.error;
      }
    }
    return result;
  }

  public async verifyEmailCode(code: string): Promise<any> {
    let result: any = { success: false };
    try {
      const userId = JSON.parse(localStorage.getItem(this.emailVerificationKey)).userId;
      result =
        await this._httpService.post<LoginResponse>('verifyEmailCode',
        {
          'userId': userId,
          'code': code
        }).toPromise();
    } catch (err) {
      if (err.error) {
        result = err.error;
      }
    }
    if (result.success) {
      result.data.access_token = 'Bearer ' + result.data.access_token;
      localStorage.setItem(this.localStorageKey, JSON.stringify(result.data));
      this.isLoginSubject.next(true);
    }
    return result;
  }

  public getLoggedUser(): LoginResponse {
    const authData = localStorage.getItem(this.localStorageKey);
    return authData ? JSON.parse(authData) : null;
  }

  public getEmailVerification() {
    const authEmailData = localStorage.getItem(this.emailVerificationKey);
    return authEmailData ? JSON.parse(authEmailData) : null;
  }

  public getLoggedUserId(): string {
    const authData = this.getLoggedUser();
    return authData ? authData.user_id : null;
  }

  public getLoggedUserRole(): string {
    const authData = localStorage.getItem(this.localStorageKey);
    return authData ? JSON.parse(authData).role : null;
  }

  public isAdmin(): boolean {
    const user = this.getLoggedUser();
    return user && user.role && (
      user.role === 'Admin' || user.role === 'HumanResources'
    );
  }

  public hasToken() {
    return localStorage.getItem(this.localStorageKey) ? true : false;
  }

  public forgotPass(username: string): Observable<any> {
    return this._httpService.post('forgotPassword', { 'username': username });
  }

  public setUserRoleToSeeHow(userRole: string): void {
    localStorage.setItem('UserRoleToSeeHow', userRole);
    localStorage.setItem('LoggedUserRole', this.getLoggedUserRole());
    const authData = this.getLoggedUser();
    authData.role = userRole;
    localStorage.removeItem(this.localStorageKey);
    localStorage.setItem(this.localStorageKey, JSON.stringify(authData));
  }

  public getUserRoleToSeeHow(): string {
    const data = localStorage.getItem('UserRoleToSeeHow');
    return data ? data : null;
  }

  public clearUserRoleToSeeHow(): void {
    localStorage.removeItem('UserRoleToSeeHow');
    const loggedUserRole = localStorage.getItem('LoggedUserRole');
    localStorage.removeItem('LoggedUserRole');
    const authData = this.getLoggedUser();
    authData.role = loggedUserRole;
    localStorage.removeItem(this.localStorageKey);
    localStorage.setItem(this.localStorageKey, JSON.stringify(authData));
  }

  public setImpersonationInfo(user: UserPreviewInterceptor): void {
    localStorage.setItem(
      'ImpersonatedUser', JSON.stringify(user)
    );
    const currentAuthData = this.getLoggedUser();
    localStorage.setItem('loggedUser', JSON.stringify(currentAuthData));
    currentAuthData.user_id = user.userId;
    currentAuthData.role = user.userRole;
    currentAuthData.name = user.userName;
    currentAuthData.username = user.userName;
    localStorage.removeItem(this.localStorageKey);
    localStorage.setItem(this.localStorageKey, JSON.stringify(currentAuthData));
    this.isLoginSubject.next(true);
    this.personificationChanged$.next(true);
  }

  public getImpersonatedUser(): UserPreviewInterceptor {
    const data = localStorage.getItem('ImpersonatedUser');
    return data ? JSON.parse(data) : null;
  }

  public clearImpersonationInfo(): void {
    const loggedUser = localStorage.getItem('loggedUser');
    const authData = loggedUser ? loggedUser : null;
    localStorage.removeItem(this.localStorageKey);
    localStorage.removeItem('ImpersonatedUser');
    localStorage.removeItem('loggedUser');
    localStorage.setItem(this.localStorageKey, authData);
    this.isLoginSubject.next(true);
    this.personificationChanged$.next(false);
  }

}
