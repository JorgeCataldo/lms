import { Injectable } from '@angular/core';
import { CanActivate, ActivatedRouteSnapshot, Router } from '@angular/router';
import { MatSnackBar } from '@angular/material';
import { Observable, of } from 'rxjs';
import { AuthService } from '../services/auth.service';
import { NotificationClass } from '../classes/notification';
import { TrackPreview } from 'src/app/models/previews/track.interface';

@Injectable()
export class RoleGuard extends NotificationClass implements CanActivate {

  private _responsibleTrackRoutes: string[] = [
    'configuracoes/trilha-de-curso/:trackId',
    'configuracoes/trilha-de-curso/:trackId/modulo/:moduleId'
  ];

  private _responsibleRoutes: string[] = [
    'configuracoes/testes-de-avaliacao',
    'configuracoes/testes-de-avaliacao/:testId',
    'configuracoes/testes-de-avaliacao/repostas/:testId',
    'configuracoes/testes-de-avaliacao/repostas/:testId/:responseId'
  ];

  private _secretaryRoutes: string[] = [
    'configuracoes/trilha',
    'configuracoes/trilhas',
    'configuracoes/recomendacoes-produtos',
    'configuracoes/gerenciar-equipe',
    'configuracoes/emails-enviados',
    'configuracoes/trilha-de-curso/:trackId',
    'configuracoes/trilha-de-curso/:trackId/modulo/:moduleId',
    'empenho-desempenho',
    'configuracoes/nps',
    'relatorios',
  ];

  constructor(
    protected _snackBar: MatSnackBar,
    private _authService: AuthService,
    private _router: Router
  ) {
    super(_snackBar);
  }

  public canActivate(route: ActivatedRouteSnapshot): Observable<boolean> {
    const user = this._authService.getLoggedUser();

    if (!user || !user.role || user.role.trim() === '')
      return this.preventAccess();

    if (this._authService.isAdmin())
      return of(true);

    if (this._secretaryRoutes.find(x => x === route.routeConfig.path)) {
      if (user.role !== 'Secretary' && user.role !== 'Author')
        return of(false);

      return of(true);
    }

    if (this._responsibleRoutes.find(x => x === route.routeConfig.path)) {
      if (!user.adminAccesses || !(user.adminAccesses instanceof Array))
      return of(false);

      return of(user.adminAccesses.some(access => access === 'Gestor'));
    }

    if (this._responsibleTrackRoutes.find(x => x === route.routeConfig.path)) {
      const trackPreview = localStorage.getItem('track-responsible');
      if (trackPreview) {
        const track: TrackPreview = JSON.parse(trackPreview);
        if (track && (track.subordinate || track.instructor)) {
          return of(true);
        }
      }
    }

    return this.preventAccess();
  }

  private preventAccess(): Observable<boolean> {
    this.notify('Acesso negado');
    this._router.navigate(['']);
    return of(false);
  }

}
