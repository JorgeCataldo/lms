import { Component, OnInit, Output, EventEmitter, Input } from '@angular/core';
import { MatSnackBar } from '@angular/material';
import { FormGroup, FormControl, Validators } from '@angular/forms';
import { ExternalService } from '../../../../../shared/services/external.service';
import { UtilService } from '../../../../../shared/services/util.service';
import { NotificationClass } from '../../../../../shared/classes/notification';
import { Module } from '../../../../../models/module.model';

@Component({
  selector: 'app-new-module-video',
  templateUrl: './video.component.html',
  styleUrls: ['../new-module-steps.scss', './video.component.scss']
})
export class NewModuleVideoComponent extends NotificationClass implements OnInit {

  @Input() readonly module: Module;
  @Output() setModuleVideo = new EventEmitter();

  public formGroup: FormGroup;

  constructor(
    protected _snackBar: MatSnackBar,
    private _externalService: ExternalService,
    private _utilService: UtilService
  ) {
    super(_snackBar);
  }

  ngOnInit() {
    this.formGroup = this._createFormGroup( this.module );
  }

  public nextStep(): void {
    const videoInfo: Module = this.formGroup.getRawValue();

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

    this.setModuleVideo.emit( videoInfo );
  }

  private _createFormGroup(module: Module): FormGroup {
    return new FormGroup({
      'videoUrl': new FormControl(
        module && module.videoUrl ? module.videoUrl : ''
      ),
      'videoDuration': new FormControl(
        module && module.videoDuration ?
          this._utilService.formatDurationToHour(module.videoDuration) : ''
      ),
      'videoId': new FormControl(
        module && module.videoUrl ?
          this._externalService.getVideoIdFromUrlIfValid(module.videoUrl) : null
      ),
      'videoTitle': new FormControl(null)
    });
  }
}
