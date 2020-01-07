import { Component, Input, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { TrackPreview } from '../../../models/previews/track.interface';
import { UserProgress } from 'src/app/models/subject.model';
import { UserService } from 'src/app/pages/_services/user.service';
import { UtilService } from '../../services/util.service';
import { AuthService } from '../../services/auth.service';
import { Recommendations } from 'src/app/settings/users/user-models/user-track';
import { SettingsUsersService } from 'src/app/settings/_services/users.service';
import { MatSnackBar } from '@angular/material';
import { NotificationClass } from '../../classes/notification';
import { TrackHeightChangeEnum } from 'src/app/models/enums/track-height-change.enum';

@Component({
  selector: 'app-track-card',
  template: `
    <div class="track-card" [style.height]="height + 'px'" (click)="goToTrack($event)">
      <!--<app-card-tag *ngIf="track.recommended"
        [text]="'RECOMENDADO'"
      ></app-card-tag >-->

      <app-card-tag
        *ngIf="track.recommended && showRecommended && !track.instructor"
        [ngClass]="{ 'attending': track.recommended && showRecommended }"
        [text]="'Cursando'"
      ></app-card-tag >

      <!--<app-card-tag
        *ngIf="track.instructor"
        [ngClass]="{ 'responsible': track.recommended }"
        [text]="'INSTRUTOR'"
      ></app-card-tag >

      <img class="icon" [src]="userService.getCompletedLevelImage(progress?.level, progress?.progress)" />-->

      <img class="course-img" [src]="track.imageUrl" />
      <!--<div><p *ngIf="track.recommended && showRecommended">Cursando</p></div>-->

      <!--<app-progress-bar
        [completedPercentage]="progress?.progress*100"
        [height]="11"
      ></app-progress-bar>-->

      <div class="content" >
        <p class="title"
          *ngIf="showText" >
          {{ track.title }}
        </p>
        <div class="modules" *ngIf="showModule || showEvents">
          <div class="all-track">
            <div class="showModule" *ngIf="showModule">
              <p>{{ track.moduleCount }}</p>
              <span> {{ track.moduleCount > 1 ? 'módulos' : 'módulo' }}&nbsp;</span>
            </div>
            <div class="showModule" *ngIf="showModule && showEvents">
              /
            </div>
            <div class="showEvents" *ngIf="showEvents">
              <p>&nbsp;{{ track.eventCount }} </p>
              <span> {{ track.eventCount > 1 ? 'eventos' : 'evento' }} </span>
            </div>
          </div>
          <div class="hours" *ngIf="hours">
            <p>{{ getDuration() }}</p>
            <span>hrs</span>
          </div>
        </div>

        <p class="excerpt" *ngIf="showContent">
          {{ getExcerptContent() }}
        </p>

        <div class="button-store" *ngIf="showButtonKnowMore">
          <div></div>
          <button (click)="goToPageStoreUrl($event)">
            Saiba Mais
          </button>
        </div>

        <div class="card-footer">
          <!--<div>
            <p *ngIf="track.recommended && showRecommended">Cursando</p>
          </div>-->
          <div class="button-subscribe" *ngIf="!track?.recommended && showRecommended && !track.instructor">
            <p *ngIf="track.ecommerceProducts && track.ecommerceProducts[0] && track.ecommerceProducts[0].price && showPrice">
              {{ track.ecommerceProducts[0].price | currency:'BRL' }}
            </p>
            <button *ngIf="track.ecommerceProducts && track.ecommerceProducts[0]
              && track.ecommerceProducts[0].price && showPrice && showButtonSubscribe"
             (click)="goToPageEcommerceUrl($event)">
              Matricular
            </button>
          </div>
          <div class="button-subscribe-off"
              *ngIf="!track?.recommended && track?.ecommerceProducts && track?.ecommerceProducts[0]
              && track?.ecommerceProducts[0].disableFreeTrial && showButtonSubscribe">
            <p>GRÁTIS</p>
            <button (click)="goToPageEcommerceUrl($event)">
              Matricular
            </button>
          </div>
        <!--<p class="duration" >
          {{ track.moduleCount }} módulos / {{ track.eventCount }} eventos
          <span>{{ getDuration() }} h</span>
        </p>-->
      </div>
    </div>`,
  styleUrls: ['./track-card.component.scss']
})
export class TrackCardComponent extends NotificationClass implements OnInit {

  @Input() track: TrackPreview;
  @Input() progress: UserProgress;
  @Input() showModule: boolean = false;
  @Input() showEvents: boolean = false;
  @Input() showRecommended: boolean = false;
  @Input() showText: boolean = false;
  @Input() showContent: boolean = false;
  @Input() hours: boolean = false;
  @Input() showButtonKnowMore: boolean = false;
  @Input() showPrice: boolean = false;
  @Input() showButtonSubscribe: boolean = false;
  @Input() redirectWhenRecommended: boolean = true;

  public height: number = 170;

  constructor(
    private _router: Router,
    public userService: UserService,
    private _usersService: SettingsUsersService,
    protected _snackBar: MatSnackBar,
    private _utilService: UtilService,
    private _authService: AuthService
  ) {
    super(_snackBar);
    }

  ngOnInit() {
    this.changeHeight();
  }

  public getDuration(): string {
    return this._utilService.formatSecondsToHour( this.track.duration );
  }

  public goToTrack(event) {

    event.stopPropagation();

    if (!this.redirectWhenRecommended && this.track.recommended) {
      return false;
    }

    if ( this._authService.isAdmin() ) {
      this._router.navigate([ 'configuracoes/trilha-de-curso/' + this.track.id ]);
    } else if (this.track.subordinate || this.track.instructor) {
      localStorage.setItem('track-responsible', JSON.stringify(this.track));
      this._router.navigate([ 'configuracoes/trilha-de-curso/' + this.track.id ]);
    } else {
      this.track.recommended ?
        this._router.navigate([ 'trilha-de-curso/' + this.track.id ]) :
        this._router.navigate([ 'trilha/' + this.track.id ]);
    }
  }

  public goToPageEcommerceUrl(event) {

    event.stopPropagation();

    if (this.track.ecommerceProducts === null) {
      this.notify('As matrículas não foram aberta');
    }
    if (this.track.ecommerceProducts[0].disableFreeTrial) {
      const recommendations: Array<Recommendations> = this._setRecommendations();
      this._usersService.updateUserRecommendations(
        recommendations
      ).subscribe(() => {
        this.track.recommended = true;
        this.notify('Inscrição feita com sucesso');
      }, () => this.notify('Ocorreu um erro ao tentar se matricular'));
    } else {
      window.open(this.track.ecommerceProducts[0].linkEcommerce, '_blank');
    }
  }

  private _setRecommendations(): Array<Recommendations> {
    const recommendations: Array<Recommendations> = [];
    const user = JSON.parse(localStorage.getItem('auth_data'));
      const recommendation: Recommendations = {
        userId: user.user_id,
        tracks: [
          {
          id: this.track.id, name: this.track.title, level: 0, percentage: 0
          }],
        modules: [],
        events: []
      };
      recommendations.push(recommendation);

    return recommendations;
  }

  public goToPageStoreUrl(event) {

    event.stopPropagation();

    if (this.track.ecommerceProducts === null || this.track.ecommerceProducts[0].linkProduct === null) {
      this.notify('Essa trilha ainda não tem detalhes');
    } else {
      window.open(this.track.ecommerceProducts[0].linkProduct, '_blank');
    }
  }

  public getExcerptContent(): string {
    if (!this.track) return '';

    if (this.track) {
      return this.track.description.substring(0, 130) + '...';
    }
  }

  public changeHeight() {
    if (this.showText) {
      this.height += TrackHeightChangeEnum.showText;
    }
    if (this.showModule) {
     this.height += TrackHeightChangeEnum.showModule;
    }
    if (this.showEvents) {
      this.height += TrackHeightChangeEnum.showEvents;
     }
    if (this.showContent) {
     this.height += TrackHeightChangeEnum.showContent;
    }
    if (this.hours) {
     this.height += TrackHeightChangeEnum.hours;
    }
    if (this.showButtonKnowMore) {
     this.height += TrackHeightChangeEnum.showButtonKnowMore;
    }
    if (this.showPrice) {
     this.height += TrackHeightChangeEnum.showPrice;
    }
    if (this.showButtonSubscribe) {
     this.height += TrackHeightChangeEnum.showButtonSubscribe;
    }
 }

}
