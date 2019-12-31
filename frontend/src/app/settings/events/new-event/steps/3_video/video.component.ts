import { Component, OnInit, Output, EventEmitter, Input } from '@angular/core';
import { FormGroup, FormControl, Validators } from '@angular/forms';
import { ExternalService } from '../../../../../shared/services/external.service';
import { UtilService } from '../../../../../shared/services/util.service';
import { Event } from '../../../../../models/event.model';
import { NotificationClass } from '../../../../../shared/classes/notification';
import { MatSnackBar } from '@angular/material';

@Component({
  selector: 'app-new-event-video',
  templateUrl: './video.component.html',
  styleUrls: ['../new-event-steps.scss', './video.component.scss']
})
export class NewEventVideoComponent extends NotificationClass implements OnInit {

  @Input() readonly event: Event;
  @Output() setEventVideo = new EventEmitter();

  public formGroup: FormGroup;

  constructor(
    protected _snackBar: MatSnackBar,
    private _externalService: ExternalService,
    private _utilService: UtilService
  ) {
    super(_snackBar);
  }

  ngOnInit() {
    this.formGroup = this._createFormGroup( this.event );
    this._createVideoUrlSubscription( this.formGroup );
  }

  public nextStep(): void {
    const videoInfo: Event = this.formGroup.getRawValue();
    videoInfo.videoUrl = 'https://player.vimeo.com/video/' + videoInfo.videoId;
    videoInfo.videoDuration = this._utilService.getDurationFromFormattedHour(
      (videoInfo.videoDuration as any).split(':').join('')
    );
    delete videoInfo.videoTitle;
    delete videoInfo.videoId;
    this.setEventVideo.emit( videoInfo );
  }

  private _createFormGroup(event: Event): FormGroup {
    return new FormGroup({
      'videoUrl': new FormControl(
        event && event.videoUrl ? event.videoUrl : '', [ Validators.required ]
      ),
      'videoDuration': new FormControl(
        event && event.videoDuration ?
          this._utilService.formatDurationToHour(event.videoDuration) : '',
          [ Validators.required ]
      ),
      'videoId': new FormControl(
        event && event.videoUrl ?
          this._externalService.getVideoIdFromUrlIfValid(event.videoUrl) : null
      ),
      'videoTitle': new FormControl(null)
    });
  }

  private _createVideoUrlSubscription(formGroup: FormGroup): void {
    formGroup.get('videoUrl').valueChanges.subscribe((url: string) => {
      const videoId = this._externalService.getVideoIdFromUrlIfValid(url);
      if (videoId) {
        formGroup.get('videoId').setValue(videoId);
        this._externalService.getVideoInfoFromVimeo(videoId).subscribe((response) => {
          if (response && response.length > 0) {
            formGroup.get('videoTitle').setValue(response[0].title);

            const duration = this._utilService.formatDurationToHour(response[0].duration);
            formGroup.get('videoDuration').setValue(duration);
          }
        }, () => {
          this.notify('Ocorreu um erro ao buscar as informações do vídeo');
        });
      }
    });
  }
}
