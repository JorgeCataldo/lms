import { Component, Inject } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA, MatSnackBar } from '@angular/material';
import { Content, VideoReference } from '../../../../../../../models/content.model';
import { UtilService } from '../../../../../../../shared/services/util.service';
import { ExternalService } from '../../../../../../../shared/services/external.service';
import { NotificationClass } from '../../../../../../../shared/classes/notification';

@Component({
  selector: 'app-video-config-dialog',
  templateUrl: './video-config.dialog.html',
  styleUrls: ['../content-config.scss', './video-config.dialog.scss']
})
export class VideoConfigDialogComponent extends NotificationClass {

  constructor(
    protected _snackBar: MatSnackBar,
    public dialogRef: MatDialogRef<VideoConfigDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public content: Content,
    private _utilService: UtilService,
    private _externalService: ExternalService
  ) {
    super(_snackBar);
  }

  public addReference(): void {
    this.content.referenceUrls ?
      this.content.referenceUrls.push('') :
      this.content.referenceUrls = [''];
  }

  public setConcept(concept: VideoReference) {
    concept.checked = !concept.checked;
    if (concept.checked)
      concept.positions = ['000000'];
  }

  public referencesTrackBy(index: number, obj: any) {
    return index;
  }

  public isString(concept): boolean {
    return typeof concept['positions'][0] === 'string';
  }

  public dismiss(): void {
    this.dialogRef.close();
  }

  public save(): void {
    const adjustedContent = this._adjustConcepts(this.content);

    if (!adjustedContent) {
      this.notify('URL do vídeo não foi inserida ou é inválida');
      return;
    }
    this.dialogRef.close(adjustedContent);
  }

  public getFormattedByDuration(duration: number): string {
    return this._utilService.formatDurationToHour(duration);
  }

  private _adjustConcepts(content: Content): Content {
    if (!content.value || content.value.trim() === '') return null;

    const videoId = this._externalService.getVideoIdFromUrlIfValid(content.value);
    if (!videoId) return null;

    content.value = 'https://player.vimeo.com/video/' + videoId;
    content.concepts.forEach((concept: VideoReference) => {
      if (concept.positions) {
        concept.positions = concept.positions.map((pos: any) => {
          return this._utilService.getDurationFromFormattedHour(pos);
        });
      }
    });
    return content;
  }

}
