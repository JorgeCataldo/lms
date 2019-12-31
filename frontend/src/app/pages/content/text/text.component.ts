import { Component, Input, OnInit, Output, EventEmitter } from '@angular/core';
import { Content, TextReference } from '../../../models/content.model';
import { TextMarker } from './marker.model';
import { SharedService } from 'src/app/shared/services/shared.service';

@Component({
  selector: 'app-content-text',
  templateUrl: './text.component.html',
  styleUrls: ['./text.component.scss']
})
export class TextContentComponent implements OnInit {

  @Input() readonly resumedView?: boolean = false;
  @Input() content: Content;
  @Output() saveTextFinishedAction: EventEmitter<string> = new EventEmitter();

  public markers: Array<TextMarker> = [];
  private _finishedText: boolean = false;

  constructor(
    private _sharedService: SharedService
  ) {
    this._sharedService.forumQuestion.subscribe(() => {
      this._sharedService.forumQuestionResponse.next('-');
    });
  }

  ngOnInit() {
    this.content.concepts.forEach((concept: TextReference) => {
      if (concept.anchors) {
        this.markers.push.apply(
          this.markers,
          concept.anchors.map((anchor: string) => {
            return new TextMarker(concept.name, anchor);
          })
        );
      }
    });
  }

  public goToAnchor(anchor: string): void {
    const anchorElement: any = document.querySelector('a[href=\'' + anchor + '\']');
    document.querySelector('.htmlContent').scrollTop = anchorElement.offsetTop - 50;
  }

  public onScroll(event) {
    if (!this._finishedText) {
      const scrolled = Math.ceil(
        event.target.offsetHeight +
        event.target.scrollTop
      );

      if (scrolled >= event.target.scrollHeight) {
        this._finishedText = true;
        this.saveTextFinishedAction.emit( this.content.id );
      }
    }
  }

}
