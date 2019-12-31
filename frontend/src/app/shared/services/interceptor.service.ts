import { Injectable } from '@angular/core';
import { HttpEvent, HttpInterceptor, HttpHandler, HttpRequest, HttpResponse, HttpErrorResponse } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { SharedService } from './shared.service';
import { tap, catchError } from 'rxjs/operators';
import { Router } from '@angular/router';
import { AuthService } from './auth.service';
import { UserPreviewInterceptor } from '../interfaces/user-preview-interceptor.interface';

@Injectable()
export class LoaderInterceptor implements HttpInterceptor {

  private _requestsCount: number = 0;

  constructor(private _sharedService: SharedService, private router: Router,
    private _authService: AuthService) { }

  intercept(
    request: HttpRequest<any>, next: HttpHandler
  ): Observable<HttpEvent<any>> {

    this._sharedService.setLoaderValue(true);
    this._requestsCount++;

    const userRoleToSeeHow = this._authService.getUserRoleToSeeHow();
    if (userRoleToSeeHow) {
      request = request.clone({
        setHeaders: {
          'UserRoleToSeeHow': userRoleToSeeHow
        }
      });
    }

    const impersonatedUser: UserPreviewInterceptor | null = this._authService.getImpersonatedUser();
    if (impersonatedUser && impersonatedUser.userId) {
      request = request.clone({
        setHeaders: {
          'UserIdToImpersonate': impersonatedUser.userId,
          'UserRoleToImpersonate': impersonatedUser.userRole
        }
      });
    }

    return next.handle(request).pipe(tap((event: HttpEvent<any>) => {
      if (event instanceof HttpResponse || event instanceof HttpErrorResponse) {
        this._requestsCount--;
        if (this._requestsCount === 0)
          this._sharedService.setLoaderValue(false);
      }
      return event;
    }))
    .pipe(catchError((error, caught) => {
      // intercept the respons error and displace it to the console
      this._requestsCount--;
      this._sharedService.setLoaderValue(false);
      console.log(error);
      this.handleAuthError(error);
      throw error;
    }) as any);
  }

  private handleAuthError(err: HttpErrorResponse): Observable<any> {
    // handle your auth error or rethrow
    if (err.status === 401) {
      // navigate /delete cookies or whatever
      console.log('handled error ' + err.status);
      this._authService.logout();
      // if you've caught / handled the error, you don't want to
      // rethrow it unless you also want downstream consumers to have to handle it as well.
      return of(err.message);
    }
  }
}
