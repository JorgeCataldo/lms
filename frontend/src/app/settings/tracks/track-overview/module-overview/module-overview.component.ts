import { Component, OnInit } from '@angular/core';
import { MatSnackBar } from '@angular/material';
import { ActivatedRoute } from '@angular/router';
import { NotificationClass } from 'src/app/shared/classes/notification';
import { SettingsTracksService } from 'src/app/settings/_services/tracks.service';
import { TrackModuleOverview } from 'src/app/models/track-overview.interface';
import { UtilService } from 'src/app/shared/services/util.service';
import { SharedService } from 'src/app/shared/services/shared.service';
import { Level } from 'src/app/models/shared/level.interface';

@Component({
  selector: 'app-track-module-overview',
  templateUrl: './module-overview.component.html',
  styleUrls: ['./module-overview.component.scss']
})
export class TrackModuleOverviewComponent extends NotificationClass implements OnInit {

  public track: TrackModuleOverview;
  public levels: Array<Level> = [];

  private _trackId: string;
  private _moduleId: string;

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
    this._moduleId = this._activatedRoute.snapshot.paramMap.get('moduleId');

    localStorage.setItem('selectedTrackId', JSON.stringify(this._trackId));
    localStorage.setItem('selectedModuleId', JSON.stringify(this._moduleId));

    this._loadModuleOverview( this._trackId, this._moduleId );
    this._loadLevels();
  }

  public getFormattedHour(): string {
    if (!this.track || !this.track.duration) return '--';
    return this._utilService.formatSecondsToHourMinute(this.track.duration);
  }

  public getRadarLabels(): Array<number> {
    return this.track.subjectsProgress.map((m, index) => index + 1);
  }

  public getRadarTitleCallback(tooltipItem, data): string {
    const moduleIndex = data.labels[tooltipItem[0].index] - 1;
    return this.track.subjectsProgress.find(
      (m, index) => index === moduleIndex
    ).subjectTitle;
  }

  public getRadarDataset() {
    const subjects = this.track.subjectsProgress;
    return [{
      label: 'TURMA',
      data: subjects.map(m => m.level > 3 ? 3 : m.level),
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

  private _loadModuleOverview(trackId: string, moduleId: string): void {
    this._tracksService.getTrackModuleOverview(
      trackId, moduleId
    ).subscribe((response) => {
      this.track = response.data;
    }, () => this.notify('Ocorreu um erro, por favor tente novamente mais tarde'));
  }

  private _loadLevels(): void {
    this._sharedService.getLevels(true).subscribe((response) => {
      this.levels = response.data;
    }, () => this.notify('Ocorreu um erro, por favor tente novamente mais tarde'));
  }

}
