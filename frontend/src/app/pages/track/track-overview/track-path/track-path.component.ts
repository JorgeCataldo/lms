import { Component, Input } from '@angular/core';
import { TrackModule } from 'src/app/models/track-module.model';
import { TrackEvent } from 'src/app/models/track-event.model';
import { Router } from '@angular/router';
import { TrackOverview, TrackStudentOverview } from 'src/app/models/track-overview.interface';
import { ExcelService } from 'src/app/shared/services/excel.service';
import { SettingsTracksService } from 'src/app/settings/_services/tracks.service';
import { NotificationClass } from 'src/app/shared/classes/notification';
import { MatSnackBar } from '@angular/material';
import { TrackPreview } from 'src/app/models/previews/track.interface';
import { AuthService } from 'src/app/shared/services/auth.service';
import { ValuationTest } from 'src/app/models/valuation-test.interface';

import * as moment from 'moment/moment';
import { Debug } from '@sentry/core/dist/integrations';

@Component({
  selector: 'app-track-overview-path',
  template: `
    <div class="track" >
      <div class="title">
        <p class="track-title" >
          TRILHA
        </p>
        <div class="title-buttons">
          <button *ngIf="canExport()"
            class="btn-test"
            (click)="exportGrades()" >
            Exportar Notas
          </button>
          <button *ngIf="canExport()"
            class="btn-test"
            (click)="exportStatus()" >
            Exportar Status
          </button>
          <button *ngIf="!allowOverview"
            class="btn-test"
            (click)="seeReportCard()" >
            Ver Boletim
          </button>
          <button *ngIf="!allowOverview"
            class="btn-test"
            (click)="seeStatus()" >
            Meu Desempenho
          </button>
        </div>
      </div>
      <div class="tests" *ngIf="trackTestsFree.length > 0">
        <div class="header">
          <p>
            <span class="valuation-index">0</span>
            Avaliações
          </p>
          <p class="arrow-box">
            <span *ngIf="!trackTestsFreeExpanded">
              <img (click)="trackTestsFreeExpanded = !trackTestsFreeExpanded" class="turn-up" src="./assets/img/arrow-back.png" />
            </span>
            <span *ngIf="trackTestsFreeExpanded">
              <img (click)="trackTestsFreeExpanded = !trackTestsFreeExpanded" class="turn-down" src="./assets/img/arrow-back.png" />
            </span>
          </p>
        </div>
        <div class="test-content" *ngIf="trackTestsFreeExpanded" >
          <div class="level" *ngFor="let test of trackTestsFree">
            <p>
              {{test.title}}
            </p>
            <button class="btn-test"
              (click)="goToTest(test.id)"
              [disabled]="disablePercentButtonLogic(test.answered, completedPercentage, getModuleTestPercent(test))" >
              {{
                disablePercentButtonLogicText(test.answered, completedPercentage, getModuleTestPercent(test))
              }}
            </button>
          </div>
        </div>
      </div>
      <ul><!--[ngClass]="{'disabled-item': item.blocked}"-->
        <li *ngFor="let item of getTrackCards(); let index = index" [ngClass]="item.blocked ? 'disabled-item' : ''" >
          <div class="list-content" (click)="goToModuleOverview(item)">
            <p class="index" >
              {{ index + 1 }}
            </p>
            <p class="title" >
              {{ item.title }}<br>
              <span>{{ item.isEvent ? 'EVENTO PRESENCIAL' : 'MÓDULO ONLINE' }}</span>
            </p>
            <div class="card-right">
              <div class="progress" *ngIf="(!item.isEvent || hasSubprogress) && !item.studentFinished" >
                <app-progress-bar
                  [completedPercentage]="getProgress(item)"
                  [height]="12"
                  [color]="checkLateEvent(item)"
                ></app-progress-bar>
                <span [ngStyle]="{'color': checkLateEvent(item)}">
                  {{ getProgress(item).toFixed(0) }}%
                </span>
              </div>
              <div class="finished" *ngIf="item.studentFinished" >
                Finalizado
                <img src="./assets/img/status-success.png" />
              </div>
              <p class="sub-progress" *ngIf="hasSubprogress" [ngStyle]="{'color': checkLateEvent(item)}">
                <ng-container *ngIf="item.isEvent && item.incompleteRequirementStudents" >
                  <b>{{track.studentsCount - item.incompleteRequirementStudents.length}}</b>/{{track.studentsCount}} pré-requisitos
                </ng-container>
                <ng-container *ngIf="!item.isEvent">
                  <b>{{item.completeStudents}}</b>/{{track.studentsCount}} finalizados
                </ng-container>
              </p>
            </div>
            <div class="late" *ngIf="!hasSubprogress && hasLateStudents()" >
              <p>alunos<br>atrasados</p>
              <img src="./assets/img/status-warning.png" />
            </div>
            <div class="card-right">
              <img class="go" src="./assets/img/chevron-right-black.png" />
            </div>
          </div>
          <div class="tests" *ngIf="checkEventItemValuation(item)">
            <div class="header">
              <p>
                <span class="valuation-index">{{ index + 1 }}</span>
                Avaliações de {{ item.title }}
              </p>
              <p class="arrow-box">
                <span *ngIf="!trackTestsOrderedExpanded[index]">
                  <img (click)="trackTestsOrderedExpanded[index] = !trackTestsOrderedExpanded[index]"
                  class="turn-up" src="./assets/img/arrow-back.png" />
                </span>
                <span *ngIf="trackTestsOrderedExpanded[index]">
                  <img (click)="trackTestsOrderedExpanded[index] = !trackTestsOrderedExpanded[index]"
                  class="turn-down" src="./assets/img/arrow-back.png" />
                </span>
              </p>
            </div>
            <div class="test-content" *ngIf="trackTestsOrderedExpanded[index]" >
              <div class="level" *ngFor="let test of listEventItemValuation(item)">
                <p>
                  {{test.title}}
                </p>
                <button class="btn-test"
                  (click)="goToTest(test.id)"
                  [disabled]="disablePercentButtonLogic(test.answered, item.studentFinished ? 100 : getProgress(item), 1)" >
                  {{
                    disablePercentButtonLogicText(test.answered, item.studentFinished ? 100 : getProgress(item), 1)
                  }}
                </button>
              </div>
            </div>
          </div>
        </li>
      </ul>
      <div class="tests" *ngIf="trackTestsResearch.length > 0">
        <div class="header">
          <p>
            <span class="valuation-index">{{getTrackCards().length + 1}}</span>
            Avaliação de Reação
          </p>
          <p class="arrow-box">
            <span *ngIf="!trackTestsResearchExpanded">
              <img (click)="trackTestsResearchExpanded = !trackTestsResearchExpanded" class="turn-up" src="./assets/img/arrow-back.png" />
            </span>
            <span *ngIf="trackTestsResearchExpanded">
              <img (click)="trackTestsResearchExpanded = !trackTestsResearchExpanded" class="turn-down" src="./assets/img/arrow-back.png" />
            </span>
          </p>
        </div>
        <div class="test-content" *ngIf="trackTestsResearchExpanded" >
          <div class="level" *ngFor="let test of trackTestsResearch">
            <p>
              {{test.title}}
            </p>
            <button class="btn-test"
              (click)="goToTest(test.id)"
              [disabled]="disablePercentButtonLogic(test.answered, completedPercentage, getModuleTestPercent(test))" >
              {{
                disablePercentButtonLogicText(test.answered, completedPercentage, getModuleTestPercent(test))
              }}
            </button>
          </div>
        </div>
      </div>
    </div>
  `,
  styleUrls: ['./track-path.component.scss']
})
export class TrackOverviewPathComponent extends NotificationClass {

  @Input() readonly track: TrackOverview | TrackStudentOverview;
  @Input() readonly completedPercentage: number;
  @Input() readonly lateStudents: boolean = false;
  @Input() readonly allowOverview: boolean = false;
  @Input() readonly hasSubprogress: boolean = false;
  @Input() readonly trackTestsResearch: any[] = [];
  @Input() readonly trackTestsFree: any[] = [];
  @Input() set setTrackTestsOrdered(tests: any[]) {
    this.trackTestsOrdered = tests;
    tests.forEach(() => {
      this.trackTestsOrderedExpanded.push(false);
    });
  }
  public trackTestsOrdered: any[] = [];
  public trackTestsResearchExpanded: boolean = false;
  public trackTestsFreeExpanded: boolean = false;
  public trackTestsOrderedExpanded: boolean[] = [];

  constructor(
    protected _snackBar: MatSnackBar,
    private _router: Router,
    private _excelService: ExcelService,
    private _tracksService: SettingsTracksService,
    private _authService: AuthService
  ) {
    super(_snackBar);
  }

  public getTrackCards(): Array<TrackModule | TrackEvent> {
    if (!this.track) return [];

    this.track.eventsConfiguration ?
      this.track.eventsConfiguration.forEach(e => e.isEvent = true) :
      this.track.eventsConfiguration = [];

    let modulesEvents = [... this.track.modulesConfiguration, ...this.track.eventsConfiguration];
    modulesEvents = modulesEvents.sort((a, b) => {
      return a.order - b.order;
    });
    return modulesEvents;
  }

  private _getLevelName(level: number): string {
    switch (level) {
      case 1:
        return 'Iniciante';
      case 2:
        return 'Intermediário';
      case 3:
        return 'Avançado';
      case 4:
        return 'Expert';
      default:
        return 'Sem Badge';
    }
  }

  public exportStatus() {
    const track = this.track as TrackOverview;

    if (track) {
      const excelModule = [];
      if (this.track.modulesConfiguration) {
        for (let idx = 0; idx < this.track.modulesConfiguration.length; idx++) {
          const mod = this.track.modulesConfiguration[idx];
          if (mod.students) {
            for (let useridx = 0; useridx < mod.students.length; useridx++) {
              const user = mod.students[useridx];

              excelModule.push({
                module: mod.title,
                user: user.userName,
                points: user.points,
                level: this._getLevelName(user.level),
                acheivedGoal: user.acheivedGoal,
                group: user.businessGroup,
                unit: user.businessUnit,
                segment: user.segment
              });
            }
          }
          if (track.topPerformants) {
            if (mod.students.length !==  track.topPerformants.length) {
              for (let topIndex = 0; topIndex < track.topPerformants.length; topIndex++) {
                const findIndex = mod.students.findIndex(x => x.userId === track.topPerformants[topIndex].id);
                if (findIndex === -1) {
                  const user = track.topPerformants[topIndex];
                  excelModule.push({
                    module: mod.title,
                    user: user.name,
                    points: 0,
                    level: 'Não Iniciado',
                    acheivedGoal: false,
                    group: user.businessGroup,
                    unit: user.businessUnit,
                    segment: user.segment
                  });
                }
              }
            }
          }
        }
      }
      if (track.topPerformants) {
        const notStarted = track.topPerformants.filter(x => x.points === 0);
        if (notStarted) {
          for (let i = 0; i < notStarted.length; i++) {
            const user = notStarted[i];
            excelModule.push({
              module: 'Não iniciado',
              user: user.name,
              points: user.points,
              level: '-',
              acheivedGoal: false,
              group: user.businessGroup,
              unit: user.businessUnit,
              segment: user.segment
            });
          }
        }
      }
      this._excelService.exportAsExcelFile(excelModule, 'Status');
    }
  }

  public canExport(): boolean {
    const trackPreview = localStorage.getItem('track-responsible');
    if (trackPreview) {
      const track: TrackPreview = JSON.parse(trackPreview);
      if (track && track.subordinate) {
        return !track.subordinate && this.allowOverview;
      }
    }
    return this.allowOverview;
  }

  public hasLateStudents(): boolean {
    if (!this.track) { return false; }
    const track = this.track as TrackOverview;
    return track.lateStudents && track.lateStudents.length > 0;
  }

  public getProgress(item: TrackModule | TrackEvent): number {
    if (item.isEvent) {
      const eventItem = item as TrackEvent;
      if (this.hasSubprogress) {
        return !eventItem.incompleteRequirementStudents || eventItem.incompleteRequirementStudents.length === 0 ?
          100 : Math.floor(100 - (eventItem.incompleteRequirementStudents.length / this.track.studentsCount) * 100);
      }

    } else {
      const moduleItem = item as TrackModule;
      if (this.hasSubprogress) {
        return moduleItem.completeStudents === this.track.studentsCount ?
          100 : Math.floor((moduleItem.completeStudents / this.track.studentsCount) * 100);
      } else
        return moduleItem.studentPercentage * 100;
    }
    return 0;
  }

  public checkLateEvent(item: TrackModule | TrackEvent): string {
    if (!item.isEvent) {
      return 'var(--primary-color)';
    } else if (item.isEvent && !this._hasLateStudents(item as TrackEvent)) {
      return 'var(--primary-color)';
    } else {
      return '#FF4376';
    }
  }

  private _checkModuleEventAvailability(item: TrackModule | TrackEvent): boolean {
    if (item.studentFinished ||
      item.alwaysAvailable === null ||
      item.alwaysAvailable === undefined ||
      item.alwaysAvailable ||
      (item.alwaysAvailable === false && (item.openDate === null || item.openDate === undefined))
    ) {
      return true;
    }

    const momentOpenDate = moment(item.openDate).startOf('day');
    const momentValuationDate = moment(item.valuationDate).startOf('day');
    const momentToday = moment().startOf('day');
    const diffOpen = moment.duration(momentOpenDate.diff(momentToday)).asDays();
    const diffValueation = moment.duration(momentValuationDate.diff(momentToday)).asDays();
    if (diffOpen <= 0 && diffValueation >= 0)
      return true;

    item.isEvent ? this.notify('Evento fora da data') :
      this.notify('Módulo fora da data');
    return false;
  }

  private _checkModuleEventAvailabilityAdmin(item: TrackModule | TrackEvent): boolean {
    if (item.alwaysAvailable === null ||
      item.alwaysAvailable === undefined ||
      item.alwaysAvailable ||
      (item.alwaysAvailable === false && (item.openDate === null || item.openDate === undefined))
    ) {
      return true;
    }

    const momentOpenDate = moment(item.openDate).startOf('day');
    const momentToday = moment().startOf('day');
    const diffOpen = moment.duration(momentOpenDate.diff(momentToday)).asDays();
    if (diffOpen <= 0)
      return true;

    item.isEvent ? this.notify('Este evento ainda não começou') :
      this.notify('Este módulo ainda não começou');
    return false;
  }

  public goToModuleOverview(item: TrackModule | TrackEvent): void {

    console.log('item.isEvent -> ', item.isEvent);
    console.log('item.blocked -> ', item.blocked);

    if (!item.isEvent && item.blocked) {
      this.notify('É necessário se aplicar ao case antes de visualizar o curso.');
      return;
    }


    const trackPreview = localStorage.getItem('track-responsible') !== null ||
      this._authService.getLoggedUserRole() === 'Secretary';
    if (this.allowOverview) {
      if (this._checkModuleEventAvailabilityAdmin(item)) {
        if (!item.isEvent) {
          this._router.navigate([
            'configuracoes/trilha-de-curso/' + this.track.id + '/modulo/' + (item as TrackModule).moduleId
          ]);
        } else if (item.isEvent && !trackPreview) {
          const eventItem = item as TrackEvent;
          localStorage.setItem('fromTrack', JSON.stringify(this.track));
          this._router.navigate(['configuracoes/gerenciar-eventos/' + eventItem.eventId]);
        } else if (item.isEvent && trackPreview) {
          const eventItem = item as TrackEvent;
          localStorage.setItem('fromTrack', JSON.stringify(this.track));
          this._router.navigate(['configuracoes/gerenciar-inscricoes-notas/' + eventItem.eventId + '/' + eventItem.eventScheduleId ]);
        }
      }
    } else {
      if (this._checkModuleEventAvailability(item)) {
        if (!item.isEvent) {
          this._router.navigate(['modulo/' + (item as TrackModule).moduleId]);
        } else {
          const eventItem = item as TrackEvent;
          this._router.navigate(['evento/' + eventItem.eventId + '/' + eventItem.eventScheduleId]);
        }
      }
    }
  }

  private _hasLateStudents(item: TrackEvent): boolean {
    return this.hasLateStudents() &&
      (this.track as TrackOverview).lateStudents.some(
        s => s.lateEvents.some(evId => evId === item.eventId)
      );
  }

  public seeStatus() {
    this._router.navigate(['configuracoes/trilha-de-curso/' + this.track.id + '/' + this._authService.getLoggedUser().user_id]);
  }

  public seeReportCard() {
    this._router.navigate(['configuracoes/trilha-de-curso/boletim/' + this.track.id + '/' + this._authService.getLoggedUser().user_id]);
  }

  public exportGrades() {
    this._tracksService.getTrackOverviewGrades(this.track.id).subscribe(res => {
      const exportArray: any[] = [];
      const moduleArray: any[] = [];
      const eventArray: any[] = [];
      const eventPresenceArray: any[] = [];
      for (let i = 0; i < res.data.length; i++) {
        exportArray.push([]);
        exportArray[i]['Alunos'] = res.data[i].name;
      }
      for (let i = 0; i < res.data.length; i++) {
        for (let j = 0; j < res.data[i].moduleGrade.length; j++) {
          if (!moduleArray.includes(res.data[i].moduleGrade[j].moduleName)) {
            moduleArray.push('Módulo: ' + res.data[i].moduleGrade[j].moduleName);
            moduleArray.push('Módulo-Peso: ' + res.data[i].moduleGrade[j].moduleName);
          }
        }
        for (let k = 0; k < res.data[i].eventGrade.length; k++) {
          if (!eventArray.includes(res.data[i].eventGrade[k].eventName)) {
            eventArray.push('Evento: ' + res.data[i].eventGrade[k].eventName);
            eventArray.push('Evento-Peso: ' + res.data[i].eventGrade[k].eventName);
          }
        }
        for (let l = 0; l < res.data[i].eventPresence.length; l++) {
          if (!eventPresenceArray.includes(res.data[i].eventPresence[l].eventName)) {
            eventPresenceArray.push(res.data[i].eventPresence[l].eventName);
          }
        }
      }
      for (let i = 0; i < res.data.length; i++) {
        for (let j = 0; j < moduleArray.length; j++) {
          exportArray[i][moduleArray[j]] = '';
        }
        for (let j = 0; j < res.data[i].moduleGrade.length; j++) {
          exportArray[i]['Módulo: ' + res.data[i].moduleGrade[j].moduleName] = res.data[i].moduleGrade[j].grade;
          exportArray[i]['Módulo-Peso: ' + res.data[i].moduleGrade[j].moduleName] = res.data[i].moduleGrade[j].weightGrade;
        }
      }
      for (let i = 0; i < res.data.length; i++) {
        for (let k = 0; k < eventArray.length; k++) {
          exportArray[i][eventArray[k]] = '';
        }
        for (let k = 0; k < res.data[i].eventGrade.length; k++) {
          exportArray[i]['Evento: ' + res.data[i].eventGrade[k].eventName] = res.data[i].eventGrade[k].finalGrade;
          exportArray[i]['Evento-Peso: ' + res.data[i].eventGrade[k].eventName] = res.data[i].eventGrade[k].weightGrade;
        }
      }
      for (let i = 0; i < res.data.length; i++) {
        for (let l = 0; l < eventPresenceArray.length; l++) {
          exportArray[i][eventPresenceArray[l]] = '';
        }
        for (let l = 0; l < res.data[i].eventPresence.length; l++) {
          exportArray[i][res.data[i].eventPresence[l].eventName] = res.data[i].eventPresence[l].userPresence;
        }
      }
      this._excelService.exportAsExcelFile(exportArray, 'Notas-Trilha-' + this.track.id);
    }, err => { this.notify(this.getErrorNotification(err)); });
  }

  public goToTest(testId: string): void {
    this._router.navigate([ '/teste-de-avaliacao/' + testId ]);
  }

  public getModuleTestPercent(test: ValuationTest): number {
    return test.testTracks.find(x => x.id === this.track.id).percent;
  }

  public disablePercentButtonLogic(answered: boolean, currentPercent: number, percentNeeded: number): boolean {
    const progress = currentPercent !== null && currentPercent > 1 ? currentPercent / 100 : 0;
    if (answered) {
      return true;
    }
    if (percentNeeded > 1) {
      percentNeeded = percentNeeded / 100;
    }
    if (currentPercent == null) {
      return true;
    } else {
      if (progress >= percentNeeded) {
        return false;
      } else {
        return true;
      }
    }
  }

  public disablePercentButtonLogicText(answered: boolean, currentPercent: number, percentNeeded: number): string {
    const progress = currentPercent !== null && currentPercent > 1 ? currentPercent / 100 : 0;
    if (answered) {
      return 'Teste respondido';
    }
    if (percentNeeded > 1) {
      percentNeeded = percentNeeded / 100;
    }
    if (this.completedPercentage == null) {
      return 'Progresso necessário ' + (percentNeeded * 100).toString() + '%';
    } else {
      if (progress >= percentNeeded) {
        return 'Fazer o teste';
      } else {
        return 'Progresso necessário ' + (percentNeeded * 100).toString() + '%';
      }
    }
  }

  public checkEventItemValuation(item:  TrackModule | TrackEvent): boolean {
    if (this.trackTestsOrdered !== null &&
      this.trackTestsOrdered.length !== 0 &&
      this.trackTestsOrdered.some(x => x.testTracks.some(y => y.order === item.order))) {
        return true;
    }
    return false;
  }

  public checkAlwaysAvailable(item: TrackModule | TrackEvent): boolean {
    if (item.alwaysAvailable !== null &&
      item.alwaysAvailable !== undefined &&
      !item.alwaysAvailable &&
      item.openDate) {
      return true;
    }
    return false;
  }

  public listEventItemValuation(item:  TrackModule | TrackEvent): any[] {
    if (this.trackTestsOrdered !== null &&
      this.trackTestsOrdered.length !== 0 &&
      this.trackTestsOrdered.some(x => x.testTracks.some(y => y.order === item.order))) {
      return this.trackTestsOrdered.filter(x =>
        x.testTracks.some(y => y.order === item.order)
      );
    }
    return [];
  }

}
