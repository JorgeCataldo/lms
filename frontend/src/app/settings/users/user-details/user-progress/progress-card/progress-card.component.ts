import { Component, Input, OnInit } from '@angular/core';
import { SharedService } from 'src/app/shared/services/shared.service';
import { UserService } from 'src/app/pages/_services/user.service';
import { NotificationClass } from 'src/app/shared/classes/notification';
import { MatSnackBar } from '@angular/material';
import { AuthService, LoginResponse } from 'src/app/shared/services/auth.service';
import { ActivatedRoute, Router } from '@angular/router';

@Component({
  selector: 'app-user-progress-card',
  template: `
    <div class="progress-card" >
      <img class="main-img" (click)="getRouterLink()" [src]="progressItem.imageUrl" />
      <div class="gradient-bar" ></div>
      <img class="badge" [src]="userService.getCompletedLevelImage(progressItem?.level, progressItem?.percentage)" />
      <ng-container *ngIf="hasGeneralPermission && progressItem?.canBlock">
        <img *ngIf="progressItem?.blocked" (click)="blockUserMaterial()" class="delete" src="./assets/img/lock-user-red.png" />
        <img *ngIf="!progressItem?.blocked" (click)="blockUserMaterial()" class="delete" src="./assets/img/unlock-user-black.png" />
      </ng-container>
      <p class="title" (click)="getRouterLink()">
        {{ progressItem.title }}<br>
        <small *ngIf="isTrack" >
          {{ progressItem.classRank > 0 ? 'Ranking na Turma: #' + progressItem.classRank : '' }}
        </small>
      </p>
      <ng-container *ngIf="showProgress">
        <p class="level" *ngIf="levelDict" >
          <span>{{ levelDict[progressItem.level] }}</span>
          <ng-container *ngIf="progressItem.level !== 4">
            {{ progressItem.percentage * 100 | number:'1.0-0' }}%
          </ng-container>
          <ng-container *ngIf="progressItem.level === 4">
            Expert
          </ng-container>
        </p>
        <app-progress-bar
          [completedPercentage]="progressItem.percentage*100"
          [height]="16"
        ></app-progress-bar>
      </ng-container>
    </div>`,
  styleUrls: ['./progress-card.component.scss']
})
export class UserProgressCardComponent extends NotificationClass implements OnInit {

  @Input() progressItem: any = {};
  @Input() showProgress: boolean = true;
  @Input() isTrack: boolean = false;
  @Input() isEvent: boolean = false;
  @Input() isModule: boolean = false;

  public levelDict: any;
  public hasGeneralPermission: boolean;
  public user: LoginResponse;

  constructor(
    protected _snackBar: MatSnackBar,
    private _sharedService: SharedService,
    private _authService: AuthService,
    private _activatedRoute: ActivatedRoute,
    public userService: UserService,
    private router: Router) {
      super(_snackBar);
      this.user = this._authService.getLoggedUser();
      this.hasGeneralPermission = this.user.role === 'Admin' || this.user.role === 'HumanResources';
  }

  ngOnInit() {
    this._loadLevels();
  }
  private _loadLevels(): void {
    this._sharedService.getLevels(true).subscribe((response) => {
      this.levelDict = {};
      response.data.forEach(level => {
        this.levelDict[level.id] = level.description;
      });
    }, () => this.notify('Ocorreu um erro, por favor tente novamente mais tarde'));
  }

  public getRouterLink() {
    if (this.isEvent) {
      this.router.navigate(['evento/' + this.progressItem.id + '/' + this.progressItem.scheduleId]);
    } else if (this.isTrack) {
      this.router.navigate(['configuracoes/trilha-de-curso/' + this.progressItem.id + '/' + this.user.user_id]);
    } else {
      this.router.navigate(['home/modulo/' + this.progressItem.id]);
    }
  }

  public blockUserMaterial() {
    const userId = this._activatedRoute.snapshot.paramMap.get('userId');
    if (this.isEvent) {
      this.userService.blockUserMaterial(userId, '', '', this.progressItem.scheduleId).subscribe(() => {
        this.progressItem.blocked = !this.progressItem.blocked;
        this.progressItem.blocked ? this.notify('Evento bloqueado com sucesso') :
        this.notify('Evento desbloqueado com sucesso');
      }, err => {
        this.notify('ocorreu um erro em mudar o status de bloqueio deste evento');
      });
    } else if (this.isTrack) {
      this.userService.blockUserMaterial(userId, this.progressItem.id, '', '').subscribe(() => {
        this.progressItem.blocked = !this.progressItem.blocked;
        this.progressItem.blocked ? this.notify('Trilha bloqueada com sucesso') :
        this.notify('Trilha desbloqueada com sucesso');
      }, err => {
        this.notify('ocorreu um erro em mudar o status de bloqueio desta trilha');
      });
    } else if (this.isModule) {
      this.userService.blockUserMaterial(userId, '', this.progressItem.id, '').subscribe(() => {
        this.progressItem.blocked = !this.progressItem.blocked;
        this.progressItem.blocked ? this.notify('Módulo bloqueado com sucesso') :
        this.notify('Módulo desbloqueado com sucesso');
      }, err => {
        this.notify('ocorreu um erro em mudar o status de bloqueio deste módulo');
      });
    }
  }

}
