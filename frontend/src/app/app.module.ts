import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { NgModule, ErrorHandler, Injectable, Injector } from '@angular/core';
import { HTTP_INTERCEPTORS, HttpErrorResponse } from '@angular/common/http';
import { AuthService, AuthInterceptor } from '@tg4/http-infrastructure/dist/src';
import { CookieService } from 'ngx-cookie-service';

import { AppComponent } from './app.component';
import { PagesModule } from './pages/pages.module';
import { HeaderModule } from './shared/components/header/header.module';
import { SettingsModule } from './settings/settings.module';
import { NgxMaskModule } from 'ngx-mask';
import { AppRoutingModule } from './app.routing.module';
import { MatSidenavModule } from '@angular/material';
import { LoaderInterceptor } from './shared/services/interceptor.service';
import { ErrorsService } from './shared/services/error.service';

import * as Sentry from '@sentry/browser';
import * as LogRocket from 'logrocket';
import { environment } from 'src/environments/environment';
import { RecruitmentModule } from './recruitment/recruitment.module';
import { LOCALE_ID } from '@angular/core';

if (environment.production) {
  Sentry.init({
    dsn: 'https://d6084f6630ae435e81e34cf71a6870d3@sentry.io/1372093'
  });

  LogRocket.init('thm68q/btg-app');

  Sentry.configureScope((scope) => {
    const authData = localStorage.getItem('auth_data');
    if (authData) {
      const user = JSON.parse(authData);
      scope.setUser({
        'id': user.user_id,
        'username': user.username
      });
    }
  });
}

@Injectable()
export class SentryErrorHandler implements ErrorHandler {

  constructor(
    private _errorsService: ErrorsService
  ) { }

  handleError(error) {
    if (error instanceof HttpErrorResponse) { // Server Error
      if (!navigator.onLine)
        return this._errorsService.notify('Sem conexão com a internet');

      Sentry.captureException((error as any).originalError || error);
      console.error(error);

      return this._errorsService.notify(
        this._errorsService.getErrorNotification(error)
      );

    } else { // Client Error
      if (environment.production) {
        Sentry.captureException((error as any).originalError || error);
        const errorRoute = '/erro?url=' + window.location.href;
        window.location.href = window.location.protocol + '//' + window.location.host + errorRoute;
      } else {
        console.error(error);
      }
    }
  }
}

@NgModule({
  declarations: [
    AppComponent
  ],
  imports: [
    BrowserModule,
    BrowserAnimationsModule,
    AppRoutingModule,
    PagesModule,
    SettingsModule,
    RecruitmentModule,
    HeaderModule,
    NgxMaskModule.forRoot(),
    MatSidenavModule
  ],
  providers: [
    AuthService,
    ErrorsService,
    CookieService,
    { provide: HTTP_INTERCEPTORS, useClass: AuthInterceptor, multi: true },
    { provide: HTTP_INTERCEPTORS, useClass: LoaderInterceptor, multi: true },
    { provide: ErrorHandler, useClass: SentryErrorHandler },
    { provide: LOCALE_ID, useValue: 'pt' }
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
