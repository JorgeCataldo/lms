import { Component, Output, EventEmitter, Input, OnInit } from '@angular/core';
import { Content } from '../../../../models/content.model';
import { UtilService } from 'src/app/shared/services/util.service';
import { Level } from 'src/app/models/shared/level.interface';
import { ActivatedRoute, Router } from '@angular/router';
import { identifierModuleUrl } from '@angular/compiler';
import { ActionInfoTypeEnum, ActionInfo, ActionInfoPageEnum } from 'src/app/shared/directives/save-action/action-info.interface';
import { AnalyticsService } from 'src/app/shared/services/analytics.service';
import { ModuleConfiguration } from 'src/app/models/module-configuration';
import { ContentModulesService } from 'src/app/pages/_services/modules.service';

@Component({
  selector: 'app-content-menu',
  template: `
    <div class="contents-menu" >
      <div class="arrow" >
        <span>CONTEÚDO</span>
        <img src="./assets/img/expand-arrow.png" (click)="closeMenu()" />
      </div>
      <div *ngIf="menuContents.modules">
        <div class="module-item"
          *ngFor="let mod of menuContents.modules;"
          [ngClass]="{ 'active': mod.selected === true }"
          (click)="toggleSelection($event, mod)">
          <div class="module-title">{{ mod.title }}</div>

          <div class="subject-item"
            *ngFor="let subject of mod.subjects;"
            [ngClass]="{ 'active': subject.selected === true }"
            (click)="toggleSelection($event, subject)">

            <div class="subject-title">{{ subject.title }}</div>

            <div class="content content-item"
              *ngFor="let content of subject.contents; let index = index"
              [ngClass]="{ 'active': index === currentIndex }"
              (click)="goToNewContent($event, mod.id, subject.id, index)">
              <div class="content-title">
                <mat-checkbox
                  [(ngModel)]="content.watched"
                  (ngModelChange)="finishContent(mod.id, subject.id, content)"
                  (click)="$event.stopPropagation()"
                ></mat-checkbox>
                {{ content.title }}
              </div>
              <br>
              <div class="progress" >
                <span>{{ utilService.formatSecondsToMinutes(content.duration) }} min</span>
              </div>
            </div>
          </div>
        </div>
      </div>

      <div *ngIf="!menuContents.modules">
        <div class="subject-item"
          *ngFor="let subject of menuContents.subjects; let subjectIndex = index"
          [ngClass]="{ 'active': subject.selected === true, 'no-border': subjectIndex === menuContents.subjects.length - 1 }"
          (click)="toggleSelection($event, subject)">

          <div class="subject-title">{{ subject.title }}</div>
          <ng-container *ngIf="subject.selected === true">
            <div class="content content-item"
              *ngFor="let content of subject.contents; let contentIndex = index"
              [ngClass]="{ 'active': contentIndex === currentIndex, 'no-border': contentIndex === subject.contents.length - 1 }"
              (click)="goToNewContent($event, moduleId, subject.id, index)">
              <div class="content-title">
                <mat-checkbox
                  [(ngModel)]="content.watched"
                  (ngModelChange)="finishContent(moduleId, subject.id, content)"
                  (click)="$event.stopPropagation()"
                ></mat-checkbox>
                {{ content.title }}
              </div>
              <br>
              <div class="progress" >
                <span>{{ utilService.formatSecondsToMinutes(content.duration) }} min</span>
              </div>
            </div>
          </ng-container>
        </div>
      </div>

      <p class="section" >AVALIAÇÃO</p>
      <div class="rating" >
        <app-progress-bar
          [completedPercentage]="(subjectProgress ? subjectProgress.progress : 0) | asPercentage"
          [height]="8"
          [roundedBorder]="true"
        ></app-progress-bar>
        <span>
          {{ getLevelDescription(subjectProgress ? subjectProgress.level : null) }}
          {{ (subjectProgress ? subjectProgress.progress : 0) | asPercentage }}%
        </span>

        <!-- p class="objective" >
          Objetivo<br>
          INTERMEDIÁRIO 80%
        </p -->
      </div>

      <p class="section" >NÍVEIS CONQUISTADOS</p>
      <div class="badges" >
        <img [src]="getBadgesProgressImageSrc(subjectProgress ? subjectProgress.level : null)" />
        {{ getLevelDescription(subjectProgress ? subjectProgress.level : null) }}
      </div>
    </div>
  `,
  styleUrls: ['./menu.component.scss']
})
export class ContentMenuComponent implements OnInit {

  @Input() readonly contents: Array<Content> = [];
  @Input() readonly menuContents: any = '';
  @Input() readonly currentIndex: number = 0;
  @Input() readonly subjectProgress;
  @Input() readonly levels: Array<Level> = [];

  @Output() toggleMenu = new EventEmitter();
  @Output() goToContent = new EventEmitter<any>();
  @Output() toggleItemClick = new EventEmitter();

  public moduleId: string = '';
  public subjectId: string = '';

  constructor(
    public utilService: UtilService,
    private _analyticsService: AnalyticsService,
    private _activatedRoute: ActivatedRoute
  ) { }

  ngOnInit() {
    this.moduleId = this._activatedRoute.snapshot.paramMap.get('moduleId');
    this.subjectId = this._activatedRoute.snapshot.paramMap.get('subjectId');

    if (this.menuContents.modules) {
      for (let index = 0; index < this.menuContents.modules.length; index++) {
        const element = this.menuContents.modules[index];
        if (element.id === this.moduleId) {
          element.selected = true;

          for (let sIndex = 0; sIndex < this.menuContents.modules[index].subjects.length; sIndex++) {
            const sub = this.menuContents.modules[index].subjects[sIndex];
            if (sub.id === this.subjectId) {
              sub.selected = true;
            } else {
              sub.selected = false;
            }
          }
        } else {
          element.selected = false;
        }
      }
    } else {
      for (let sIndex = 0; sIndex < this.menuContents.subjects.length; sIndex++) {
        const sub = this.menuContents.subjects[sIndex];
        if (sub.id === this.subjectId) {
          sub.selected = true;
        } else {
          sub.selected = false;
        }
      }
    }
  }

  public closeMenu() {
    this.toggleMenu.emit();
  }
  public getLevelDescription(levelId: number): string {
    if (!this.levels || this.levels.length === 0 || this._hasNoProgress())
      return 'Não Iniciado';

    const level = this.levels.find(lev => lev.id === levelId);
    return level ? level.description : '';
  }

  public getBadgesProgressImageSrc(levelId: number): string {
    if (this._hasNoProgress())
      return './assets/img/empty-badge.png';

    switch (levelId) {
      case 1:
        return './assets/img/pencil-icon-shadow.png';
      case 2:
        return './assets/img/glasses-icon-shadow.png';
      case 3:
        return './assets/img/brain-icon-shadow.png';
      case 4:
        return './assets/img/brain-dark-icon-shadow.png';
      case 0:
      default:
        return './assets/img/empty-badge.png';
    }
  }

  private _hasNoProgress(): boolean {
    return !this.subjectProgress || (
      this.subjectProgress.level === 0 &&
      this.subjectProgress.progress === 0
    );
  }

  public finishContent( moduleId: string, subjectId: string, content: any): void {
    event.stopPropagation();
    if (content.watched) {
      const actionInfo = this._getActionInfo('content-video', moduleId, subjectId, content.id, ActionInfoTypeEnum.Finish);
      this._analyticsService.saveAction(actionInfo).subscribe();
    } else {
      const actionInfo = this._getActionInfo('content-video', moduleId, subjectId, content.id, ActionInfoTypeEnum.Finish);
      this._analyticsService.removeAction(actionInfo).subscribe();
    }
  }

  private _getActionInfo(description: string, moduleId: string, subjectId: string,
    contentId: string, type: ActionInfoTypeEnum): ActionInfo {
    return {
      'page': ActionInfoPageEnum.Content,
      'description': description,
      'type': type,
      'moduleId': moduleId,
      'subjectId': subjectId,
      'contentId': contentId
    };
  }

  public toggleSelection(event: Event, content: any): void {
    event.stopPropagation();
    content.selected = !content.selected;
  }

  public goToNewContent(event: Event, moduleId: string, subjectId: string, index: number): void {
    event.stopPropagation();
    const data = {
        'moduleId': moduleId,
        'subjectId': subjectId,
        'index': index
    };

    this.goToContent.emit(data);
  }

}
