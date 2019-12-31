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

  private _trackEventInfo: TrackOverview;
  private _trackId: string;
  private _currentPage: number = 1;
  private _searchTerm: string = '';

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

}
