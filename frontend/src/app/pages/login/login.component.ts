import { Component, OnInit } from '@angular/core';
import { FormGroup, FormControl, Validators } from '@angular/forms';
import { AuthService } from '../../shared/services/auth.service';
import { Router, ActivatedRoute } from '@angular/router';
import { NotificationClass } from '../../shared/classes/notification';
import { MatSnackBar, MatDialog } from '@angular/material';
import { BtgLoginComponent } from './btg-login/btg-login.component';
import { environment } from '../../../environments/environment';
import { ExternalService } from 'src/app/shared/services/external.service';
import { Credential } from 'src/app/models/login.model';
import Player from '@vimeo/player';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent extends NotificationClass implements OnInit {

  public loading: boolean;
  public formGroup: FormGroup;
  public firstAccessFormGroup: FormGroup;
  public hasSSO: boolean;
  public hasSignUp: boolean;
  public hasLoginLinkedin: boolean;
  public params: any;
  public isFirstAccess: boolean = false;
  public showLoader: boolean = false;
  public player;
  public apiUrl: string;
  private videoUrl: string = 'https://player.vimeo.com/video/308296447';
  public linkedIn: string = 'https://www.linkedin.com/oauth/v2/authorization' +
  '?response_type=code&client_id=' + environment.linkedInId +
  '&redirect_uri=' + environment.linkedInRedirect +
  '&scope=r_liteprofile%20r_emailaddress';

  constructor(
    protected _snackBar: MatSnackBar,
    private _authService: AuthService,
    private _externalService: ExternalService,
    private _dialog: MatDialog,
    private router: Router,
    private route: ActivatedRoute
  ) {
    super(_snackBar);
    this.formGroup = new FormGroup({
      'username': new FormControl('', [Validators.required]),
      'password': new FormControl('', [Validators.required])
    });
    this.hasSSO = environment.features.sso;
    this.hasSignUp = environment.features.registration;
    this.hasLoginLinkedin = environment.features.loginLinkedIn;
    this.apiUrl = environment.apiUrl;
    this.firstAccessFormGroup = new FormGroup({
      'fullName': new FormControl('', [Validators.required]),
      'phone': new FormControl('', [Validators.required]),
      'email': new FormControl('', [Validators.required]),
      'cpf': new FormControl('', [Validators.required])
    });

    this.route.queryParams.subscribe(params => {
      this.params = Object.assign({}, params);
    });
  }

  ngOnInit() {
    if (this._authService.hasToken())
      this._redirectUser();
    else if (this.params && this.params['ssoerror'])
      setTimeout(() => {
        this.notify(this.params['ssoerror']);
      });
    else if (this.params && this.params['ssosuccess']) {
      this._authService.ssoLogin(this.params);
      if (this.params.first_access && environment.features.firstLoginPersonalDataForm) {
        this.isFirstAccess = true;
        this._updateForm(this.params.user);
        // this._setIntroductionVideo();
      } else {
        this._redirectUser();
      }
    } else {
      this.route.queryParams.subscribe(params => {
        const linkedInCode = params['code'];
        if (linkedInCode) {
          this.showLoader = true;
          this._externalService.linkedInLogin(linkedInCode).subscribe(res => {
            this._authService.ssoLogin(res.data.tokenInfo);
            this._redirectUser();
            this.showLoader = false;
          }, err => {
            this.notify(this.getErrorNotification(err));
          });
        }
      });
    }
  }

  public openBtgDialog(): void {
    this._dialog.open(BtgLoginComponent, {
      width: '400px'
    });
  }

  public async forgotPass() {
    const credentials = this.formGroup.getRawValue();
    if (credentials.username) {
      this.loading = true;
      await this._authService.forgotPass(credentials.username).subscribe(() => {
        this.loading = false;
        this.notify('Foi enviado um email ao usuário com uma nova senha de acesso');
      }, () => {
        this.loading = false;
        this.notify('Usuário inválido');
      });
    } else {
      this.notify('Preencha o campo de usúario');
    }
  }

  public signUp() {
    this.router.navigate(['register']);
  }

  public async doLogin() {
    this.loading = true;
    const credentials: Credential = this.formGroup.getRawValue();
    try {
      const result = await this._authService.login(credentials.username, credentials.password);
      if (result.success) {
        this.loading = false;

        if (result.data.tokenInfo.first_access) {
          this._handleFirstAccess(result);
        } else if (!result.data.tokenInfo.email_verified) {
          this.router.navigate(['confirmacao-email']);
        } else {
          this._redirectUser();
        }

      } else {
        const error = result && result.errors && result.errors.length > 0 ?
          result.errors[0] : 'Houve um erro ao se comunicar com o servidor. Por favor tente mais tarde.';
        this.loading = false;
        this.notify(error);
      }
    } catch (err) {
      this.notify('Houve um erro ao se comunicar com o servidor. Por favor tente mais tarde.');
      this.loading = false;
    }
  }

  private _handleFirstAccess(result): void {
    const role = this._authService.getLoggedUserRole();
    if (role === 'Recruiter' || role === 'BusinessManager') {
      this.router.navigate(['configuracoes/identificacao-empresa']);
    } else if (environment.features.firstLoginPersonalDataForm) {
      this.isFirstAccess = true;
      this._updateForm(result.data.user);
      // this._setIntroductionVideo();
    } else {
      this._redirectUser();
    }
  }

  public async saveAndContinue() {
    this.loading = true;
    const credentials = this.firstAccessFormGroup.getRawValue();
    credentials.username = this.formGroup.getRawValue().username;
    try {
      const result = await this._authService.firstAccess(credentials);
      if (result.success) {
        this.loading = false;
        if (!result.data.email_verified) {
          this.router.navigate(['confirmacao-email']);
        }
        this._redirectUser();
      } else {
        const error = result && result.errors && result.errors.length > 0 ?
          result.errors[0] : 'Houve um erro ao se comunicar com o servidor. Por favor tente mais tarde.';
        this.loading = false;
        this.notify(error);
      }
    } catch (err) {
      this.notify('Houve um erro ao se comunicar com o servidor. Por favor tente mais tarde.');
      this.loading = false;
    }
  }

  private _updateForm(user) {
    if (!user) return;
    this.firstAccessFormGroup.get('fullName').setValue(user.name);
    this.firstAccessFormGroup.get('phone').setValue(user.phone);
    this.firstAccessFormGroup.get('email').setValue(user.email);
    this.firstAccessFormGroup.get('cpf').setValue(user.cpf);
  }

  private _redirectUser(): void {
    const emailUrl = localStorage.getItem('emailUrl');
    if (emailUrl) {
      this.router.navigate([emailUrl]);
    } else {
      const role = this._authService.getLoggedUserRole();
      const redirectUrl = this._getRedirectUrl(role);
      this.router.navigate([ redirectUrl ]);
    }
  }

  private _getRedirectUrl(userRole: string): string {
    switch (userRole) {
      case 'Secretary':
        return 'configuracoes/usuarios';
      case 'Recruiter':
        return 'configuracoes/vagas-empresa';
      default:
        return 'home';
    }
  }

  private _setIntroductionVideo(): void {
    if (this.videoUrl && this.videoUrl !== '') {
      const options = {
        id: this.videoUrl
      };
      setTimeout(() => {
        this.player = new Player('videoContent', options);
        this._handleVideoLoaded(this.player);
      }, 500);
    } else {
      const videoEl = document.getElementById('videoContent');
      if (videoEl)
        videoEl.innerHTML = '';
    }
  }

  private _handleVideoLoaded(player): void {
    player.on('loaded', () => {
      const frame = document.querySelector('iframe');
      if (frame) { frame.style.width = '100%'; }
      const divFrame = document.getElementById('videoContent');
      divFrame.style.visibility = 'initial';
    });
  }
}
