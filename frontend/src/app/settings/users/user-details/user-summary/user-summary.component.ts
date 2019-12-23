import { Component, Input, OnInit } from '@angular/core';
import { SettingsUsersService } from 'src/app/settings/_services/users.service';
import { Router, ActivatedRoute } from '@angular/router';
import { NotificationClass } from 'src/app/shared/classes/notification';
import { MatSnackBar } from '@angular/material';
import { UserSummary } from '../../user-models/user-summary';
import { UserService } from 'src/app/pages/_services/user.service';
import { AuthService } from 'src/app/shared/services/auth.service';
import { environment } from 'src/environments/environment';
import { ExternalService } from 'src/app/shared/services/external.service';

@Component({
  selector: 'app-user-details-summary',
  templateUrl: './user-summary.component.html',
  styleUrls: ['./user-summary.component.scss']
})
export class SettingsUserDetailsSummaryComponent extends NotificationClass implements OnInit {

  @Input() user: UserSummary = new UserSummary();


  public cacheUser: any;
  public showRecomendation = environment.features.recruitment;
  public showSendFile = environment.features.userUploadFilesToSecretary;
  public showAllTagUsers: boolean = false;

  public concepts = [];

  public hasLoginLinkedin: boolean;
  public linkedIn: string = 'https://www.linkedin.com/oauth/v2/authorization' +
  '?response_type=code&client_id=' + environment.linkedInId +
  '&redirect_uri=' + environment.linkedInRedirect +
  '&scope=r_liteprofile%20r_emailaddress';

  constructor(
    public progressService: UserService,
    protected _snackBar: MatSnackBar,
    private _usersService: SettingsUsersService,
    private _router: Router,
    private _externalService: ExternalService,
    private _route: ActivatedRoute,
    private _authService: AuthService
  ) {
    super(_snackBar);
    this._authService.isLoggedIn().subscribe(() => {
      this.cacheUser = _authService.getLoggedUser();
    });
    this.hasLoginLinkedin = environment.features.loginLinkedIn;
  }

  ngOnInit() {
    this._route.queryParams.subscribe(params => {
      const linkedInCode = params['code'];
      if (linkedInCode) {
        this._externalService.bindLinkedIn(linkedInCode, this.user.id).subscribe(res => {
          this.notify('Usuário associado com sucesso');
        }, err => {
          this.notify(this.getErrorNotification(err));
        });
      }
    });
  }

  public blockUser() {
    if (!this.user || !this.user.id) { return; }

    this._usersService.changeUserBlockedStatus(
      this.user.id.toString()
    ).subscribe(() => {
      this.user.isBlocked = !this.user.isBlocked;
      this.notify(`Usuario ${this.user.isBlocked ? 'bloqueado' : 'desbloqueado'} com sucesso`);

    }, (error) => this.notify(this.getErrorNotification(error)));
  }

  public manageUser(): void {
    if (this.user)
      this._router.navigate(['/configuracoes/usuarios/' + this.user.id]);
  }

  public editUserCareer() {
    if (this.user)
      this._router.navigate(['configuracoes/usuarios/carreira/' + this.user.id]);
  }

  public goToUserArchives() {
    if (this.user)
      this._router.navigate(['/configuracoes/detalhes-usuario/' + this.user.id + '/envio_arquivo']);
  }

  public goToRecomendation(): void {
    if (this.user)
      this._router.navigate(['/configuracoes/card-recomendacao/' + this.user.id]);
  }
}
