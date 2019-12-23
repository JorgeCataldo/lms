import { Component, Output, EventEmitter, OnInit, OnDestroy } from '@angular/core';
import { AuthService } from '../../services/auth.service';
import { Subscription } from 'rxjs';
import { SharedService } from '../../services/shared.service';
import { environment } from 'src/environments/environment';
import { UserNotification } from 'src/app/models/notification.interface';
import { Router } from '@angular/router';
import { UserService } from 'src/app/pages/_services/user.service';

@Component({
  selector: 'app-header',
  template: `
    <div id="Header">
      <div class="header-content">
        <mat-progress-bar *ngIf="showLoader"
          class="loader"
          mode="indeterminate"
        ></mat-progress-bar>
        <div>
          <a href="javascript:;" (click)="open()">
            <i class="icon icon-menu burguer-menu"></i>
          </a>

          <a routerLink="/">
          <img class="logo" style="height: 35px;" src="{{logo}}" />
          </a>
        </div>
        <div *ngIf="isLoggedIn" class="user-info">
          <img class="notification" title="Ver Como"
          *ngIf="canSeeHow()"
            [src]="getSeeHowImgSrc()"
            (click)="toggleSeeHowAction()"
          />
          <div class="notifications" *ngIf="toggleSeeHow" >
            <div class="arrow-up"></div>
            <ul>
              <li *ngFor="let type of types"
                (click)="seeHow(type.type)" >
                <div class="circle"></div>
                <div class="content" >
                  <p>
                    <b>{{ type.name }}</b>
                  </p>
                </div>
              </li>
            </ul>
          </div>
          <img class="notification" title="Notificações"
            *ngIf="notifications && notifications.length > 0"
            [ngClass]="{ 'read': hasNotification() }"
            [src]="getNotificationImgSrc()"
            (click)="toggleNotifications = !toggleNotifications"
          />
          <div class="notifications" *ngIf="toggleNotifications" >
            <div class="arrow-up"></div>
            <ul>
              <li *ngFor="let notification of notifications"
                (click)="goToNotification(notification)" >

                <div class="circle"
                  [ngClass]="{ 'read': notification.read }"
                ></div>
                <div class="content" >
                  <p>
                    <b>{{ notification.title }}</b><br>
                    {{ notification.text }}
                  </p>
                  <p>
                    <small>{{ notification.createdAt | date: 'dd/MM/yyyy' }}</small>
                  </p>
                </div>
              </li>
            </ul>
          </div>

          <img src="{{user.image_url ? user.image_url : './assets/img/user-image-placeholder.png'}}"
          *ngIf="user.role != 'Admin' && user.role != 'Secretary'" />
          <p class="user">
            <a class="user-name" href="javascript:;"
              (click)="goTouserInfo()" >
              {{ user.name }}
            </a><br>
            <a class="logout" (click)="logout()" >
              Sair
            </a>
            <!--a *ngIf="isImpersonating" class="impersonate-logout" (click)="impersonateLogout()" >
              Voltar
            </a-->
          </p>
        </div>
      </div>
      <div *ngIf="isImpersonating" class="impersonate-block">
        <p>PERSONIFICANDO: {{ user.name }}</p>
        <button class="btn-test" (click)="impersonateLogout()" >
          sair da personificação
        </button>
      </div>
    </div>`,
  styleUrls: ['./header.component.scss']
})
export class HeaderComponent implements OnInit, OnDestroy {

  @Output()
  public OpenMenu: EventEmitter<any> = new EventEmitter();

  public isLoggedIn: boolean;
  public isImpersonating: boolean = false;
  public user: any;
  public logo = environment.logo + '.png';

  public showLoader: boolean = false;
  public toggleNotifications: boolean = false;
  public toggleSeeHow: boolean = false;
  public notifications: Array<UserNotification> = [];
  public types: Array<any> = [
    {name: 'Aluno', type: 'Student'},
    {name: 'Secretaria', type: 'Secretary'},
    {name: 'RH', type: 'HumanResources'},
    {name: 'Autor', type: 'Author'}
  ];

  private _loaderSubscription: Subscription;

  constructor(
    private _authService: AuthService,
    private _sharedService: SharedService,
    private _userService: UserService,
    private _router: Router
  ) {
    _authService.isLoggedIn().subscribe((x) => {
      this.isLoggedIn = x;
      this.user = _authService.getLoggedUser();

      this.logo = './assets/img/logo-' + environment.logo + '.png';

      if (this.isLoggedIn) {
        this._loadNotifications();

        this.logo = localStorage.getItem('logo-url')
         ? localStorage.getItem('logo-url')
         : './assets/img/logo-' + environment.logo + '.png';
      }

      if (this._authService.getImpersonatedUser()) {
        this.isImpersonating = true;
      }
    });
  }

  ngOnInit() {
    this._loaderSubscription = this._sharedService.getLoaderSubject().subscribe(
      (value: boolean) => this.showLoader = value
    );
  }

  public open() {
    this.OpenMenu.emit();
  }

  public logout() {
    this._authService.logout();

    localStorage.removeItem('logo-url');
    this.logo = './assets/img/logo-' + environment.logo + '.png';
  }

  public impersonateLogout() {
    if (this._router.url === '/home') {
      location.reload();
      this._authService.clearImpersonationInfo();
    } else {
      this._router.navigate([ 'home' ]).then(() => {
        location.reload();
        this._authService.clearImpersonationInfo();
      });
    }
  }

  public toggleSeeHowAction() {
    this.toggleNotifications = false;
    if (this._authService.getUserRoleToSeeHow()) {
      this.seeHow(localStorage.getItem('LoggedUserRole'));
    } else {
      this.toggleSeeHow = !this.toggleSeeHow;
    }
  }

  public canSeeHow(): boolean {
    if (this.user.role === 'Admin' || this._authService.getUserRoleToSeeHow() ) {
      return true;
    } else {
      return false;
    }
  }

  public getSeeHowImgSrc(): string {
    const seeHow = this._authService.getUserRoleToSeeHow();
    if (seeHow) {
      return './assets/img/shared-doc.png';
    } else {
      return './assets/img/shared-content.png';
    }
  }

  public getNotificationImgSrc(): string {
    return this.hasNotification() ?
      './assets/img/notification-empty.png' :
      './assets/img/notification.png';
  }

  public hasNotification(): boolean {
    return this.notifications && this.notifications.every(n => n.read);
  }

  public goToNotification(notification: UserNotification): void {
    if (!notification.read)
      this._setNotificationRead( notification );

    this._router.navigate([ notification.redirectPath ]);
    this.toggleNotifications = false;
  }

  public seeHow(role: string): void {
    const seeHow = this._authService.getUserRoleToSeeHow();
    console.log('role -> ', role);
    if (seeHow) {
      this._userService.SeeHow(role).subscribe(() => {
        this._authService.clearUserRoleToSeeHow();
        if (this._router.url === '/home') {
          location.reload();
        } else {
          this._router.navigate([ 'home' ]).then(() => {
            location.reload();
          });
        }
      }, (error) => console.log(error));
    } else {
      this._authService.setUserRoleToSeeHow(role);
      this._userService.SeeHow(role).subscribe(() => {
        if (this._router.url === '/home') {
          location.reload();
        } else {
          this._router.navigate([ 'home' ]).then(() => {
            location.reload();
          });
        }
      }, (error) => {
        this._authService.clearUserRoleToSeeHow();
        console.log(error);
      });
    }
  }

  public goTouserInfo() {
    const role = this._authService.getLoggedUserRole();
    console.log('role -> ', role);
    if (role === 'Recruiter' || role === 'BusinessManager') {
      this._router.navigate([ '/configuracoes/identificacao-empresa' ]);
    } else {
      this._router.navigate([ '/configuracoes/detalhes-usuario/'  + this.user.user_id ]);
    }
  }

  private _loadNotifications(): void {
    this._sharedService.getNotifications().subscribe((response) => {
      this.notifications = response.data;

    }, (error) => console.log(error));
  }

  private _setNotificationRead(notification: UserNotification): void {
    notification.read = true;
    this._sharedService.manageNotification(
      notification.id, true
    ).subscribe();
  }

  ngOnDestroy() {
    this._loaderSubscription.unsubscribe();
  }
}
