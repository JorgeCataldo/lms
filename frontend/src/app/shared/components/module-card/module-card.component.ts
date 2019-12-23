import { Component, Input, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { Module } from '../../../models/module.model';
import { UserProgress } from 'src/app/models/subject.model';
import { UserService } from 'src/app/pages/_services/user.service';
import { NotificationClass } from '../../classes/notification';
import { MatSnackBar } from '@angular/material';
import { SettingsUsersService } from 'src/app/settings/_services/users.service';
import { Recommendations } from 'src/app/settings/users/user-models/user-track';
import { UtilService } from '../../services/util.service';
import { ModuleHeightChangeEnum } from 'src/app/models/enums/module-height-change.enum';

@Component({
  selector: 'app-module-card',
  template: `
    <div class="suggested-module"
      (click)="goToModule($event)"
      [style.height]="height + 'px'"
      (mouseenter)="setHoveredValue(true)"
      (mouseleave)="setHoveredValue(false)"
      [ngClass]="{ 'list': viewType === 'list' }"
    >
      <app-card-tag
          *ngIf="module?.hasUserProgess && showRecommended"
          [ngClass]="{ 'attending': module?.hasUserProgess && showRecommended }"
          [text]="'Cursando'"
      ></app-card-tag >
      <img class="course-img"
        [src]="module.imageUrl"
      />

      <!--<img class="main-image" [src]="userService.getCompletedLevelImage(progress?.level, progress?.progress)" />-->
      <span class="requirement-level" *ngIf="module.requirements && module.requirements.length > 0">
        <span [ngSwitch]="module.requirements[0].requirementValue.level">
          <span *ngSwitchCase="1">INTERMEDIÁRIO</span>
          <span *ngSwitchCase="2">EXPERT</span>
          <span *ngSwitchDefault>INICIANTE</span>
        </span>
      </span>
      <span class="requirement-text" *ngIf="module.requirements && module.requirements.length > 0">
        Nivel Necessário <br>
        <b [ngSwitch]="module.requirements[0].requirementValue.level">
          <span *ngSwitchCase="1">INTERMEDIÁRIO</span>
          <span *ngSwitchCase="2">EXPERT</span>
          <span *ngSwitchDefault>INICIANTE</span>
        </b> <br>
        <b>{{module.requirements[0].requirementValue.percentage * 100}}%</b>
      </span>

      <div class="content"
         >
        <!--<div *ngIf="viewType !== 'list'" class="top-decoration" [class.completed]="completed"></div>-->
        <p class="title"
          *ngIf="showText">
          {{ module.title }}
        </p>
        <p class="instructor"
        *ngIf="showInstructor">
          Instrutor: {{ module.instructor }}
        </p>

        <p class="excerpt" *ngIf="showContent" >
          {{ getExcerptContent() }}
        </p>

        <!--<p class="level" >
          <app-progress-bar
            *ngIf="viewType === 'list'"
            [completedPercentage]="progress?.progress*100"
            [height]="16"
          ></app-progress-bar>
          <ng-container *ngIf="levels && progress && (progress.level !== null)" >
            <span>{{ levels[progress.level] }}</span>
            <ng-container *ngIf="((progress.level !== 4))">
              {{ progress.progress * 100 | number: '1.0-0' }}%
            </ng-container>
          </ng-container>
          <ng-container *ngIf="levels && progress && (progress.level === 4)" >
            <span style="color: #4A4A4A">EXPERT</span>
          </ng-container>
        </p>-->
      </div>

        <div class="button-module">
          <div class="hours" *ngIf="hours">
            <p>{{ getDuration() }}</p>
            <span>&nbsp;hrs</span>
          </div>
          <div *ngIf="!hours"></div>
          <button *ngIf="showButtonKnowMore" (click)="goToPageStoreUrl($event)">
            Saiba Mais
          </button>
        </div>

      <div class="button-subscribe" *ngIf="!module?.hasUserProgess && showRecommended">
        <p *ngIf="module?.ecommerceProducts && module?.ecommerceProducts[0] && module?.ecommerceProducts[0].price
          && !module?.hasUserProgess && showPrice">
          {{ module.ecommerceProducts[0].price | currency:'BRL' }}
        </p>
        <button *ngIf="module?.ecommerceProducts && module?.ecommerceProducts[0] && module?.ecommerceProducts[0].price
        && !module?.hasUserProgess && showPrice && showButtonSubscribe" (click)="goToPageEcommerceUrl($event)">
          Matricular
        </button>
      </div>
      <div class="button-subscribe-off"
        *ngIf="!module?.hasUserProgess && module?.ecommerceProducts && module?.ecommerceProducts[0]
          && !module?.ecommerceProducts[0].price && showButtonSubscribe">
        <p>GRÁTIS</p>
        <button (click)="goToPageEcommerceUrl($event)">
          Matricular
        </button>
      </div>

      <app-progress-bar class="progress-bar"
        *ngIf="viewType === 'cards'"
        [completedPercentage]="progress?.progress*100"
      ></app-progress-bar>
    </div>`,
  styleUrls: ['./module-card.component.scss']
})
export class ModuleCardComponent extends NotificationClass implements OnInit {

  @Input() module: Module;
  @Input() progress: UserProgress;
  @Input() levels: any;
  @Input() viewType?: string = 'cards';
  @Input() completed = false;
  @Input() showRecommended = false;
  @Input() showText: boolean = false;
  @Input() showInstructor: boolean = false;
  @Input() showContent: boolean = false;
  @Input() hours: boolean = true;
  @Input() showButtonKnowMore: boolean = false;
  @Input() showPrice = false;
  @Input() showButtonSubscribe: boolean = false;
  @Input() redirectWhenRecommended: boolean = true;
  public height: number = 170;

  constructor(
    private _router: Router,
    protected _snackBar: MatSnackBar,
    private _usersService: SettingsUsersService,
    private _utilService: UtilService,
    public userService: UserService) {
      super(_snackBar);
    }
  ngOnInit() {
    this.changeHeight();
  }

  public getExcerptContent(): string {
    if (!this.module) return '';

    if (this.module) {
      return this.module.excerpt.substring(0, 130) + '...';
    }
  }

  public setHoveredValue(value: boolean): void {
    if (!this.module) return;
    this.module.hovered = value;
  }

  public goToModule(event) {

    event.stopPropagation();

    if (!this.redirectWhenRecommended && this.module.hasUserProgess) {
      return false;
    }

    localStorage.removeItem('allContents');
    this._router.navigate(['modulo/' + this.module.id]);
  }

  public goToPageEcommerceUrl(event) {

    event.stopPropagation();

    if (this.module.ecommerceProducts === null) {
      this.notify('As matrículas não foram aberta');
    }
    if (this.module.ecommerceProducts[0].disableFreeTrial) {
      const recommendations: Array<Recommendations> = this._setRecommendations();
      this._usersService.updateUserRecommendations(
        recommendations
      ).subscribe(() => {
        this.module.hasUserProgess = true;
        this.notify('Inscrição feita com sucesso');
      }, () => this.notify('Ocorreu um erro ao tentar se matricular'));
    } else {
      window.open(this.module.ecommerceProducts[0].linkEcommerce, '_blank');
    }
  }

  private _setRecommendations(): Array<Recommendations> {
    const recommendations: Array<Recommendations> = [];
    const user = JSON.parse(localStorage.getItem('auth_data'));
      const recommendation: Recommendations = {
        userId: user.user_id,
        tracks: [], modules: [{id: this.module.id, name: this.module.title }], events: []
      };
      recommendations.push(recommendation);

    return recommendations;
  }

  public goToPageStoreUrl(event) {

    event.stopPropagation();

    if (this.module.ecommerceProducts === null || this.module.ecommerceProducts[0].linkProduct === null) {
      this.notify('Esse curso ainda não tem detalhes');
    } else {
      window.open(this.module.ecommerceProducts[0].linkProduct, '_blank');
    }
  }

  public getDuration(): string {
    return this._utilService.formatSecondsToHour( this.module.duration );
  }

  public changeHeight() {
     if (this.showText) {
       this.height += ModuleHeightChangeEnum.showText;
     }
     if (this.showInstructor) {
      this.height += ModuleHeightChangeEnum.showInstructor;
     }
     if (this.showContent) {
      this.height += ModuleHeightChangeEnum.showContent;
     }
     if (this.hours) {
      this.height += ModuleHeightChangeEnum.hours;
     }
     if (this.showButtonKnowMore) {
      this.height += ModuleHeightChangeEnum.showButtonKnowMore;
     }
     if (this.showPrice) {
      this.height += ModuleHeightChangeEnum.showPrice;
     }
     if (this.showButtonSubscribe) {
      this.height += ModuleHeightChangeEnum.showButtonSubscribe;
     }
  }
}
