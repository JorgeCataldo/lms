import { Component, OnInit } from '@angular/core';
import { MatSnackBar } from '@angular/material';
import { ActivatedRoute } from '@angular/router';
import { SettingsTracksService } from '../../_services/tracks.service';
import { NotificationClass } from 'src/app/shared/classes/notification';
import { TrackOverview } from 'src/app/models/track-overview.interface';
import { UtilService } from 'src/app/shared/services/util.service';
import { SharedService } from 'src/app/shared/services/shared.service';
import { Level } from 'src/app/models/shared/level.interface';
import { AuthService } from 'src/app/shared/services/auth.service';
import { TimelineElement } from 'src/app/shared/components/horizontal-timeline/timeline-element';

@Component({
  selector: 'app-settings-track-overview',
  templateUrl: './track-overview.component.html',
  styleUrls: ['./track-overview.component.scss']
})
export class SettingsTrackOverviewComponent extends NotificationClass implements OnInit {

  public track: TrackOverview;
  public itemsCount: number = 0;
  public viewOptions = [
    { selected: true, title: 'OPERACIONAL' },
    { selected: false, title: 'GERENCIAL' }
  ];
  public levels: Array<Level> = [];
  public trackParticipation: any[] = [];
  public isStudent: boolean = false;
  public isBusinessManager: boolean = false;
  public content: string = '';
  public timeline: TimelineElement[] = [];

  private _trackEventInfo: TrackOverview;
  private _trackId: string;
  private _currentPage: number = 1;
  private _searchTerm: string = '';
  public totalDays: number = 0;
  public expectedProgress: number = 0;

  constructor(
    protected _snackBar: MatSnackBar,
    private _tracksService: SettingsTracksService,
    private _activatedRoute: ActivatedRoute,
    private _utilService: UtilService,
    private _sharedService: SharedService,
    private _authService: AuthService
  ) {
    super(_snackBar);
  }

  ngOnInit() {
    this._trackId = this._activatedRoute.snapshot.paramMap.get('trackId');
    this.isStudent = this._authService.getLoggedUserRole() === 'Student' ||
      this._authService.getLoggedUserRole() === 'Admin';

    this.isBusinessManager = this._authService.getLoggedUserRole() === 'BusinessManager' ||
      this._authService.getLoggedUserRole() === 'Admin';

    this._loadTrack(this._trackId);
    this._loadTrackOverviewEventInfo(this._trackId);
    this._loadLevels();
    this._loadTrackContents(this._trackId);
  }

  public getFormattedHour(): string {
    if (!this.track || !this.track.duration) return '--';
    return this._utilService.formatSecondsToHourMinute(this.track.duration);
  }

  public goToPage(page: number) {
    if (page !== this._currentPage) {
      this._currentPage = page;
      this._loadTrack( this._trackId );
    }
  }

  public searchStudent(name: string) {
    this._searchTerm = name;
    this._loadTrack( this._trackId );
  }

  public selectViewOption(optTitle: string) {
    this.viewOptions.forEach(opt => { opt.selected = false; });
    this.viewOptions.find(x => x.title === optTitle).selected = true;

    if (optTitle === 'GESTOR') {
      console.log('GESTOR');
    }
  }

  public isViewOption(title: string): boolean {
    return this.viewOptions.find(x => x.title === title).selected;
  }

  public getRadarLabels(): Array<number> {
    if (this.track)
      return this.track.modulesConfiguration.map((m, index) => index + 1);
  }

  public getRadarTitleCallback(tooltipItem, data): string {
    const moduleIndex = data.labels[tooltipItem[0].index] - 1;
    return this.track.modulesConfiguration.find(
      (m, index) => index === moduleIndex
    ).title;
  }

  public getGantProgressTitleCallback(tooltipItem, data): string {
    return tooltipItem[0].yLabel;

    // const moduleIndex = data.labels[tooltipItem[0].index] - 1;
    // return this.track.modulesConfiguration.find(
    //   (m, index) => index === moduleIndex
    // ).title;
  }

  public getRadarDataset() {
    const modules = this.track ? this.track.modulesConfiguration : [];
    return [{
      label: 'OBJETIVO',
      data: modules.map(m => m.level + 1),
      backgroundColor: 'rgba(255, 67, 118, 0.35)',
      borderColor: 'transparent',
      pointBackgroundColor: 'rgba(255, 67, 118, 0.35)',
      pointRadius: 8
    }, {
      label: 'TURMA',
      data: modules.map(m => m.classLevel),
      backgroundColor: 'rgba(36, 188, 209, 0.35)',
      borderColor: 'rgb(36, 188, 209)',
      pointBackgroundColor: 'rgb(36, 188, 209)',
      pointRadius: 15
    }];
  }

  public getRadarTooltipCallback(tooltipItem, data): string {
    const level = data.datasets[tooltipItem.datasetIndex].data[tooltipItem.index];
    return 'Level ' + level.toFixed(1) + this.getLevelDescription(
      Math.round(level - 1)
    );
  }

  public getGantProgressTooltipCallback(tooltipItem, data): boolean {

    console.log('tooltipItem -> ', tooltipItem);
    console.log('data -> ', data);

    const startDate = new Date(this.track.modulesConfiguration[tooltipItem.index].openDate);
    const endDate = new Date(this.track.modulesConfiguration[tooltipItem.index].valuationDate);
    data = 'Início: ' + this.formatDate(startDate) + '\n' +  ' Fim: ' + this.formatDate(endDate);

return data;
    // return 'Início: ' + this.formatDate(startDate) + '<br>' +
    //   ' Fim: ' + this.formatDate(endDate);
  }

  public getLevelDescription(level: number): string {
    if (level < 0 || !this.levels) return '';

    const selectedLevel = this.levels.find(l => l.id === level);
    return ' (' + selectedLevel.description + ')';
  }

  private _buildTrackParticipation() {
    for (let i = 0; i < this.track.modulesConfiguration.length; i++) {
      const moduleConfig = this.track.modulesConfiguration[i];
      for (let j = 0; j < moduleConfig.students.length; j++) {
        const student = moduleConfig.students[j];
        const participationIndex = this.trackParticipation.findIndex(x => x.userId === student.userId);
        const grade = student.grade * moduleConfig.weight;
        if (participationIndex === -1) {
          this.trackParticipation.push({
            userId: student.userId,
            imageUrl: student.imageUrl,
            name: student.userName,
            grade: +grade
          });
        } else {
          this.trackParticipation[participationIndex].grade += +grade;
        }
      }
    }
    for (let l = 0; l < this.track.eventsConfiguration.length; l++) {
      const eventConfig = this.track.eventsConfiguration[l];
      const hasCustomGrades = eventConfig.keys !== null && eventConfig.keys.length > 0;
      for (let m = 0; m < eventConfig.applications.length; m++) {
        const application = eventConfig.applications[m];
        const participationIndex = this.trackParticipation.findIndex(x => x.userId === application.userId);
        let grade = 0;
        if (hasCustomGrades) {
          for (let n = 0; n < eventConfig.keys.length; n++) {
            const key = eventConfig.keys[n];
            if (application.customEventGradeValues != null && application.customEventGradeValues.length > 0) {
              const customGradeIndex = application.customEventGradeValues.findIndex(x => x.key === key);
              if (customGradeIndex !== -1) {
                grade += +application.customEventGradeValues[customGradeIndex].grade;
              }
            }
          }
          grade = grade / eventConfig.keys.length;
        } else {
          grade = (application.inorganicGrade + application.organicGrade) / 2;
        }
        grade = grade * eventConfig.weight;
        if (participationIndex === -1) {
          this.trackParticipation.push({
            userId: application.userId,
            imageUrl: application.imageUrl,
            name: application.name,
            grade: grade
          });
        } else {
          this.trackParticipation[participationIndex].grade += grade;
        }
      }
    }
  }

  private _loadTrack(trackId: string): void {
    this._tracksService.getTrackOverview(
      trackId, true, this._currentPage, this._searchTerm
    ).subscribe((response) => {
      this.track = response.data;
      if (this._trackEventInfo) {
        this.track.eventsConfiguration = this._trackEventInfo.eventsConfiguration;
        this._buildTrackParticipation();
      }

      if (this.isStudent && this.track.isStudent) {
        this.viewOptions.push({
          selected: false,
          title: 'ALUNO'
        });
      }

      if (this.isBusinessManager) {
        this.viewOptions.push({
          selected: false,
          title: 'GESTOR'
        });
      }

      console.log('this.track -> ', this.track);

      this.getGanttProgressDataset();

    }, () => this.notify('Ocorreu um erro, por favor tente novamente mais tarde'));
  }

  private _loadTrackContents(trackId: string): void {
    this._tracksService.getAllContent(
      trackId).subscribe((response) => {
      const content = response.data;
      localStorage.setItem('allContents', JSON.stringify(content));
    }, () => this.notify('Ocorreu um erro, por favor tente novamente mais tarde'));
  }

  private _loadTrackOverviewEventInfo(trackId: string): void {
    this._tracksService.getTrackOverviewEventInfo(
      trackId
    ).subscribe((response) => {
      this._trackEventInfo = response.data;
      if (this.track) {
        this.track.eventsConfiguration = response.data.eventsConfiguration;
        this.track.lateStudents = response.data.lateStudents;
        this._buildTrackParticipation();
      }

    }, () => this.notify('Ocorreu um erro, por favor tente novamente mais tarde'));
  }

  private _loadLevels(): void {
    this._sharedService.getLevels(true).subscribe((response) => {
      this.levels = response.data;
    }, () => this.notify('Ocorreu um erro, por favor tente novamente mais tarde'));
  }

  public goToPageEcommerceUrl() {
    window.open(this.track.ecommerceUrl, '_blank');
  }

  public goToPageStoreUrl() {
    window.open(this.track.storeUrl, '_blank');
  }

  public getGanttProgressDataset() {
    if (this.track) {

      const startDate = new Date(this.track.modulesConfiguration[0].openDate);
      const endDate = new Date(this.track.modulesConfiguration[this.track.modulesConfiguration.length - 1].valuationDate);

      this.totalDays = (endDate.getTime() - startDate.getTime()) / (1000 * 3600 * 24);
      const today = new Date();

      const daysActive = (today.getTime() - startDate.getTime()) / (1000 * 3600 * 24);
      this.expectedProgress = parseFloat((daysActive * 100 / this.totalDays).toFixed(2));
      this.expectedProgress = this.expectedProgress > 100 ? 100 : this.expectedProgress;

      const progressOffsetDataset = {
        data: [],
        backgroundColor: 'transparent',
        label: 'Tempo Livre'
      };

      const progressCompletedDataset = {
        data: [],
        backgroundColor: '#5AFF59',
        label: 'Módulo Finalizado'
      };

      const progressCurrentFallOffDataset = {
        data: [],
        backgroundColor: '#EBE759',
        label: 'Duração Restante'
      };

      const progressRemainingDataset = {
        data: [],
        backgroundColor: '#59ACFF',
        label: 'Módulo Não Iniciado'
      };


      for (let i = 0; i < this.track.modulesConfiguration.length; i++) {
        const module = this.track.modulesConfiguration[i];
        const currentModuleStartDate = new Date(module.openDate);
        const currentModuleEndDate = new Date(module.valuationDate);
        const currentModuleOffsetDate = (currentModuleStartDate.getTime() - startDate.getTime()) / (1000 * 3600 * 24);
        let currentModuleCompletedProgress = 0;
        let currentModuleFallOffProgress = 0;
        let currentModuleRemainingProgress = 0;

        if (currentModuleStartDate < today && currentModuleEndDate < today) {
          currentModuleCompletedProgress = (currentModuleEndDate.getTime() - currentModuleStartDate.getTime()) / (1000 * 3600 * 24);
        } else {
          if (currentModuleStartDate < today && currentModuleEndDate > today) {
            currentModuleCompletedProgress = (today.getTime() - currentModuleStartDate.getTime()) / (1000 * 3600 * 24);
            currentModuleFallOffProgress = (currentModuleEndDate.getTime() - today.getTime()) / (1000 * 3600 * 24);
          }
        }

        if (currentModuleStartDate > today && currentModuleEndDate > today) {
          currentModuleRemainingProgress = (currentModuleEndDate.getTime() - currentModuleStartDate.getTime()) / (1000 * 3600 * 24);
        }

        progressOffsetDataset.data[i] = parseFloat((currentModuleOffsetDate * 100 / this.totalDays).toFixed(2));
        progressCompletedDataset.data[i] = parseFloat((currentModuleCompletedProgress * 100 / this.totalDays).toFixed(2));
        progressCurrentFallOffDataset.data[i] = parseFloat((currentModuleFallOffProgress * 100 / this.totalDays).toFixed(2));
        progressRemainingDataset.data[i] = parseFloat((currentModuleRemainingProgress * 100 / this.totalDays).toFixed(2));
      }

      const dataset = [ progressOffsetDataset, progressCompletedDataset, progressCurrentFallOffDataset, progressRemainingDataset];
      return dataset;
    }
  }

  public getGanttProgressLabels(): Array<string> {
    if (this.track)
      return this.track.modulesConfiguration.map((m, index) => m.title);
  }

  private formatDate(date: Date): string {
    const mm = date.getMonth() + 1;
    const dd = date.getDate();

    return [
           (dd > 9 ? '' : '0') + dd,
            (mm > 9 ? '' : '0') + mm,
            date.getFullYear()
           ].join('/');
  }

}
