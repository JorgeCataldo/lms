import { Component, OnInit, Output, EventEmitter, Input } from '@angular/core';
import { MatSnackBar } from '@angular/material';
import { FormGroup, FormControl, Validators } from '@angular/forms';
import { ExternalService } from '../../../../../shared/services/external.service';
import { UtilService } from '../../../../../shared/services/util.service';
import { NotificationClass } from '../../../../../shared/classes/notification';
import { Track } from 'src/app/models/track.model';

@Component({
  selector: 'app-new-track-video',
  templateUrl: './video.component.html',
  styleUrls: ['../new-track-steps.scss', './video.component.scss']
})
export class NewTrackVideoComponent extends NotificationClass implements OnInit {

  @Input() readonly track: Track;
  @Input() isCourse: boolean = false;
  @Output() setTrackVideo = new EventEmitter();

  public formGroup: FormGroup;
  public courseFormGroup: FormGroup;

  constructor(
    protected _snackBar: MatSnackBar,
    private _externalService: ExternalService,
    private _utilService: UtilService
  ) {
    super(_snackBar);
  }

  ngOnInit() {
    this.formGroup = this._createFormGroup( this.track );
    this.isCourse = !this.track.published;
    if (this.isCourse) this.courseFormGroup = this._createCourseFormGroup( this.track );
  }

  public nextStep(): void {
    const videoInfo: Track = this.formGroup.getRawValue();

    if (videoInfo.videoId && videoInfo.videoId !== '') {
      videoInfo.videoUrl = 'https://player.vimeo.com/video/' + videoInfo.videoId;
      videoInfo.videoDuration = this._utilService.getDurationFromFormattedHour(
        (videoInfo.videoDuration as any).split(':').join('')
      );
    } else {
      videoInfo.videoUrl = null;
      videoInfo.videoDuration = null;
    }

    delete videoInfo.videoTitle;
    delete videoInfo.videoId;

    if (this.isCourse) {
      const courseVideoInfo: Track = this.courseFormGroup.getRawValue();
      videoInfo.mandatoryCourseVideo = courseVideoInfo.mandatoryCourseVideo;
      videoInfo.courseVideoTitle = courseVideoInfo.courseVideoTitle;
      if (courseVideoInfo.courseVideoId && courseVideoInfo.courseVideoId !== '') {
        videoInfo.courseVideoUrl = 'https://player.vimeo.com/video/' + courseVideoInfo.courseVideoId;
        videoInfo.courseVideoDuration = this._utilService.getDurationFromFormattedHour(
          (courseVideoInfo.courseVideoDuration as any).split(':').join('')
        );
      } else {
        videoInfo.courseVideoUrl = null;
        videoInfo.courseVideoDuration = null;
      }
      delete videoInfo.courseVideoTitle;
      delete videoInfo.courseVideoId;
    }

    this.setTrackVideo.emit( videoInfo );
  }

  private _createFormGroup(track: Track): FormGroup {
    return new FormGroup({
      'videoUrl': new FormControl(
        track && track.videoUrl ? track.videoUrl : ''
      ),
      'videoDuration': new FormControl(
        track && track.videoDuration ?
          this._utilService.formatDurationToHour(track.videoDuration) : ''
      ),
      'videoId': new FormControl(
        track && track.videoUrl ?
          this._externalService.getVideoIdFromUrlIfValid(track.videoUrl) : null
      ),
      'videoTitle': new FormControl(null)
    });
  }

  private _createCourseFormGroup(track: Track): FormGroup {
    return new FormGroup({
      'mandatoryCourseVideo': new FormControl(
        track ? track.mandatoryCourseVideo : false
      ),
      'courseVideoUrl': new FormControl(
        track && track.courseVideoUrl ? track.courseVideoUrl : ''
      ),
      'courseVideoDuration': new FormControl(
        track && track.courseVideoDuration ?
          this._utilService.formatDurationToHour(track.courseVideoDuration) : ''
      ),
      'courseVideoId': new FormControl(
        track && track.courseVideoUrl ?
          this._externalService.getVideoIdFromUrlIfValid(track.courseVideoUrl) : null
      ),
      'courseVideoTitle': new FormControl(null)
    });
  }
}
