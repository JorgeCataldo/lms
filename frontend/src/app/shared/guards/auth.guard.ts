import { Injectable } from '@angular/core';
import { CanActivate, ActivatedRouteSnapshot, Router, RouterStateSnapshot } from '@angular/router';
import { tap } from 'rxjs/operators';
import { Observable } from 'rxjs';
import { AuthService } from '../services/auth.service';

@Injectable()
export class AuthGuard implements CanActivate {

  private emailRoutes: string[] = [
    'forum/:moduleName/:moduleId/:questionId',
    'configuracoes/avaliar-evento/:eventId/:scheduleId/:eventName/:eventDate',
    'configuracoes/avaliar-evento/:eventId/:scheduleId'
  ];

  constructor(
    private _authService: AuthService,
    private _router: Router
  ) { }

  public canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<boolean> {
    if (!localStorage.getItem('emailUrl') &&
      (this.emailRoutes.find(x => x === route.routeConfig.path) ||
        route.routeConfig.matcher)) {
      localStorage.setItem('emailUrl', state.url);
    }
    return this._authService.isLoggedIn().pipe(
      tap((isLogged: boolean) => {
        if (!isLogged)
          this._router.navigate(['']);
        return isLogged;
      })
    );
  }

}
