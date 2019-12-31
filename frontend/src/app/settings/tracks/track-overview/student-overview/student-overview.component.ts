import { Component, OnInit } from '@angular/core';
import { MatSnackBar } from '@angular/material';
import { ActivatedRoute } from '@angular/router';
import { NotificationClass } from 'src/app/shared/classes/notification';
import { SettingsTracksService } from 'src/app/settings/_services/tracks.service';
import { TrackStudentOverview } from 'src/app/models/track-overview.interface';
import { UtilService } from 'src/app/shared/services/util.service';
import { Level } from 'src/app/models/shared/level.interface';
import { SharedService } from 'src/app/shared/services/shared.service';

@Component({
  selector: 'app-track-student-overview',
  templateUrl: './student-overview.component.html',
  styleUrls: ['./student-overview.component.scss']
})
export class TrackStudentOverviewComponent extends NotificationClass implements OnInit {

  public track: TrackStudentOverview;
  public levels: Array<Level> = [];

  private _trackId: string;
  private _studentId: string;

  constructor(
    protected _snackBar: MatSnackBar,
    private _tracksService: SettingsTracksService,
    private _activatedRoute: ActivatedRoute,
    private _utilService: UtilService,
    private _sharedService: SharedService
  ) {
    super(_snackBar);
  }

  ngOnInit() {
    this._trackId = this._activatedRoute.snapshot.paramMap.get('trackId');
    this._studentId = this._activatedRoute.snapshot.paramMap.get('studentId');
    this._loadStudentOverview( this._trackId, this._studentId );
    this._loadLevels();
  }

  public getFormattedHour(): string {
    if (!this.track || !this.track.duration) return '--';
    return this._utilService.formatSecondsToHourMinute(this.track.duration);
  }

  public getStudentImg(): string {
    return this.track && this.track.student && this.track.student.imageUrl ?
      this.track.student.imageUrl :
      './assets/img/user-image-placeholder.png';
  }

  public getRadarLabels(): Array<number> {
    return this.track.modulesConfiguration.map((m, index) => index + 1);
  }

  public getRadarTitleCallback(tooltipItem, data): string {
    const moduleIndex = data.labels[tooltipItem[0].index] - 1;
    return this.track.modulesConfiguration.find(
      (m, index) => index === moduleIndex
    ).title;
  }

  public getRadarDataset() {
    const modules = this.track.modulesConfiguration;
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
      backgroundColor: 'rgba(255, 166, 62, 0.35)',
      borderColor: 'transparent',
      pointBackgroundColor: 'rgba(255, 166, 62, 0.35)',
      pointRadius: 8
    }, {
      label: 'ALUNO',
      data: modules.map(m => m.studentLevel > 0 ? m.studentLevel : 0),
      backgroundColor: 'rgba(137, 210, 220, 0.35)',
      borderColor: 'rgb(137, 210, 220)',
      pointBackgroundColor: 'rgb(137, 210, 220)',
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

  private _loadStudentOverview(trackId: string, studentId: string): void {
    this._tracksService.getTrackStudentOverview(
      trackId, studentId
    ).subscribe((response) => {
      this.track = response.data;
    }, err => {
      this.notify(this.getErrorNotification(err));
    });
  }

  private _loadLevels(): void {
    this._sharedService.getLevels(true).subscribe((response) => {
      this.levels = response.data;
    }, err => {
      this.notify(this.getErrorNotification(err));
    });
  }
}
