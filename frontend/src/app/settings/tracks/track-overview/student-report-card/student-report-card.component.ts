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
  selector: 'app-track-student-report-card',
  templateUrl: './student-report-card.component.html',
  styleUrls: ['./student-report-card.component.scss']
})
export class TrackStudentReportCardComponent extends NotificationClass implements OnInit {

  public track: any;
  public levels: Array<Level> = [];

  private _trackId: string;
  private _studentId: string;

  public readonly displayedColumns: string[] = [
    'title', 'grade'
   ];

   public readonly eventsDisplayedColumns: string[] = [
     'title', 'grade', 'date'
    ];

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
    this._loadStudentReportCard( this._trackId, this._studentId );
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

  public getLevelDescription(level: number): string {
    if (level < 0 || !this.levels) return '';

    const selectedLevel = this.levels.find(l => l.id === level);
    return ' (' + selectedLevel.description + ')';
  }

  private _loadStudentReportCard(trackId: string, studentId: string): void {
    this._tracksService.getTrackStudentReportCard(
      trackId, studentId
    ).subscribe((response) => {
      this.track = response.data;

      for (let i = 0; i < this.track.student.modules.length; i++) {
        this.track.student.modules[i].openDate = new Date(this.track.student.modules[i].openDate);
        this.track.student.modules[i].valuationDate = new Date(this.track.student.modules[i].valuationDate);
        this.track.student.modules[i].moduleGrade = parseFloat(this.track.student.modules[i].moduleGrade.toFixed(2));
      }

      for (let i = 0; i < this.track.student.events.length; i++) {
        this.track.student.events[i].date = new Date(this.track.student.events[i].date);
        this.track.student.events[i].finalGrade = parseFloat(this.track.student.events[i].finalGrade.toFixed(2));
      }

console.log('this.track -> ', this.track);

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
