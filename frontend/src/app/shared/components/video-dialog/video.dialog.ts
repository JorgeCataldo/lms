import { Component, Inject, OnInit } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material';
import Player from '@vimeo/player';

@Component({
  selector: 'app-video-dialog',
  template: `<div>
    <div id="dialogVideoContent" style="width: 100%; visibility: hidden;" ></div>
    <p *ngIf="incorrectUrl">Video inválido</p>
  </div>`,
  styleUrls: ['./video.dialog.scss']
})
export class VideoDialogComponent implements OnInit {

  public player;
  public incorrectUrl: boolean = false;

  constructor(public dialogRef: MatDialogRef<VideoDialogComponent>,
              @Inject(MAT_DIALOG_DATA) public data: { videoUrl: string }) {}

  ngOnInit() {
    this.watchVideo();
  }

  public dismiss(result: boolean): void {
    this.dialogRef.close( result );
  }

  private watchVideo() {
    if (this.data.videoUrl && this.data.videoUrl !== '') {
      const options = {
        id: this.data.videoUrl,
        height: '470'
      };
      this.player = new Player('dialogVideoContent', options);

      this.player.on('loaded', () => {
        const frame = document.querySelector('iframe');
        if (frame) { frame.style.width = '100%'; }
        const divFrame = document.getElementById('dialogVideoContent');
        divFrame.style.visibility = 'initial';
      });

      this.player.on('ended', () => {
        this.dialogRef.close( true );
      });

    } else {
      document.getElementById('dialogVideoContent').innerHTML = '';
      this.incorrectUrl = true;
    }
  }

}
