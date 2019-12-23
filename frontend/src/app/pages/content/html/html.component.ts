import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { Content, PdfReference } from '../../../models/content.model';
import { PdfMarker } from './marker.model';
import { SharedService } from 'src/app/shared/services/shared.service';
import { ScormWrapperService } from 'src/app/pages/_services/scorm-wrapper.service';

@Component({
  selector: 'app-content-html',
  templateUrl: './html.component.html',
  styleUrls: ['./html.component.scss']
})
export class HTMLContentComponent {

  @Input() readonly resumedView?: boolean = false;
  @Input() set setContent (content: Content) {
    this.content = content;
    this.totalPages = this.content.numPages || 1;
  }
  @Output() savePdfFinishedAction: EventEmitter<string> = new EventEmitter();

  public content: Content;
  public totalPages: number;
  public pageNumber: number = 1;
  public changingPage: boolean = false;
  private _finishedHtml: boolean = false;

  constructor(
    private _sharedService: SharedService
  ) {
    this._sharedService.forumQuestion.subscribe(() => {
      this._sharedService.forumQuestionResponse.next('Página ' + this.pageNumber.toString());
    });
  }

  public changePagePrevious() {
    this.pageNumber = this.pageNumber - 1;
    this._setIframeSrc(this.content.value, this.pageNumber);
  }


  private _setIframeSrc(url: string, page: number): void {
    console.log('url -> ', url);
    this.changingPage = true;
    const iFrame: any = document.getElementById('ContentIframe');
    iFrame.src = '';
    setTimeout(() => {
      iFrame.src = url;
      this.changingPage = false;
    }, 100);
  }
}
