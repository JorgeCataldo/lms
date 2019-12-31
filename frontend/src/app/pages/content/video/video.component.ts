import { Component, Input, AfterViewInit, EventEmitter, Output } from '@angular/core';
import { Content, VideoReference } from '../../../models/content.model';
import { VideoMarker } from './marker.model';
import Player from '@vimeo/player';
import { SharedService } from 'src/app/shared/services/shared.service';
import { MatSnackBar } from '@angular/material';

@Component({
  selector: 'app-content-video',
  templateUrl: './video.component.html',
  styleUrls: ['./video.component.scss']
})
export class VideoContentComponent implements AfterViewInit {

  @Input() readonly resumedView?: boolean = false;
  @Input() readonly preventPointerEvents?: boolean = false;

  @Input() set setContent(content: Content) {
    this.markers = this._setVideoMarkers(content);
    this.content = content;
    if (this.player) {
      this.player.destroy();
      this.player = null;
      this.ngAfterViewInit();
    }
  }

  @Input() set setPosition(position: number) {
    if (position)
      this.setVideoPosition(position);
  }

  @Output() saveVideoPlayedAction: EventEmitter<string> = new EventEmitter();
  @Output() saveVideoFinishedAction: EventEmitter<string> = new EventEmitter();

  public player;
  public markers: Array<VideoMarker> = [];
  public content: Content;
  private _watchedVideo: boolean = false;

  constructor(
    private _sharedService: SharedService,
    private _snackBar: MatSnackBar
  ) { }

  public getMarkerPosition(position: number, offset: number = 0): string {
    if (!this.content) return '';

    const time = (700 * position) / this.content.duration;
    return time + offset + 'px';
  }

  async ngAfterViewInit() {
    const self = this;

    try {
      const videoData = this.content.value.split('/');
      if (!this.player) {
        const options = {
          id: this.content.value,
          height: '470'
        };

        const returnPlayer = new Player('videoContent', options);
        await returnPlayer.ready().catch((error) => {
          throw error;
        });
        this.player = returnPlayer;

      } else {
        await this.player.loadVideo(videoData[videoData.length - 1]);
        this.player.off('ended');
        this.player.off('cuepoint');
      }

      const cuePoints = await this.player.getCuePoints();
      cuePoints.forEach(async cue => {
        await self.player.removeCuePoint(cue.id);
      });

      this.markers = this.markers.sort((a, b) => {
        if (a.position < b.position)
          return -1;
        if (a.position > b.position)
          return 1;
        return 0;
      });

      this.markers.forEach((marker: VideoMarker, idx) => {
        if (!(marker.position > this.content.duration))
          this.player.addCuePoint(marker.position, idx);
      });

      this.player.on('ended', (data) => { self.clearCues(data); });
      this.player.on('cuepoint', (data) => { self.setCues(data); });

      this.player.on('loaded', () => {
        const frame = document.querySelector('iframe');
        if (frame)
          frame.style.height = '100%';
          frame.style.width = '100%';
      });

      this.player.on('timeupdate', (data) => {
        if (data.percent >= 0.9) {
          // this.saveVideoFinishedAction.emit( this.content.id );
        }
      });

      this._handleVideoPlayed( this.player );

    } catch (error) {
      this._snackBar.open(
        'Não foi possível carregar o vídeo, tente novamente mais tarde', 'OK',
        { duration: 4000, verticalPosition: 'top' }
      );
    }
  }

  private clearCues(data: any) {
    this.markers.forEach((marker: VideoMarker) => {
      marker.hovered = false;
    });
  }

  private setCues(data) {
    this.markers.forEach((marker: VideoMarker) => {
      marker.hovered = false;
    });
    this.markers[data.data].hovered = true;
  }

  public setVideoPosition(position: number) {
    if (this.player)
      this.player.setCurrentTime(position);
  }

  private _setVideoMarkers(content: Content): Array<VideoMarker> {
    const markers = [];
    content.concepts.forEach((concept: VideoReference) => {
      if (concept.positions) {
        const mark = concept.positions.map((pos: number) => {
          return new VideoMarker(concept.name, pos);
        })[0];
        markers.push(mark);
      }
    });
    return markers;
  }

  private _handleVideoPlayed(player) {
    player.on('play', () => {
      if (!this._watchedVideo) {
        this._watchedVideo = true;
        this.saveVideoPlayedAction.emit( this.content.id );
      }
    });

    this._sharedService.forumQuestion.subscribe(() => {
      player.pause();
      player.getCurrentTime().then(seconds => {
        const date = new Date(null);
        date.setSeconds(seconds);
        const result = date.toISOString().substr(14, 5);
        this._sharedService.forumQuestionResponse.next(result);
      });
    });
  }
}
