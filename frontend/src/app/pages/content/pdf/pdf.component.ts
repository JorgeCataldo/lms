import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { Content, PdfReference } from '../../../models/content.model';
import { PdfMarker } from './marker.model';
import { SharedService } from 'src/app/shared/services/shared.service';

@Component({
  selector: 'app-content-pdf',
  templateUrl: './pdf.component.html',
  styleUrls: ['./pdf.component.scss']
})
export class PDFContentComponent {

  @Input() readonly resumedView?: boolean = false;
  @Input() set setContent (content: Content) {
    this.content = content;
    this._createMarkers();
    this.totalPages = this.content.numPages || 1;
  }
  @Output() savePdfFinishedAction: EventEmitter<string> = new EventEmitter();

  public content: Content;
  public totalPages: number;
  public pageNumber: number = 1;
  public markers: Array<PdfMarker> = [];
  public changingPage: boolean = false;
  private _finishedPdf: boolean = false;

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

  public changePageNext() {
    this.pageNumber = this.pageNumber + 1;
    this._setIframeSrc(this.content.value, this.pageNumber);
    this._checkPdfFinished();
  }

  public goToPage(page: number) {
    if (page !== this.pageNumber &&
        page > 0 &&
        page <= this.totalPages
    ) {
      this.pageNumber = page;
      this._setIframeSrc(this.content.value, this.pageNumber);
    }

    this._checkPdfFinished();
  }

  private _checkPdfFinished(): void {
    if (this.pageNumber === this.totalPages && !this._finishedPdf) {
      this._finishedPdf = true;
      this.savePdfFinishedAction.emit( this.content.id );
    }
  }

  private _setIframeSrc(url: string, page: number): void {
    this.changingPage = true;
    const iFrame: any = document.getElementById('ContentIframe');
    iFrame.src = '';
    setTimeout(() => {
      iFrame.src = url + '#page=' + page;
      this.changingPage = false;
    }, 100);
  }

  private _createMarkers(): void {
    this.content.concepts.forEach((concept: PdfReference) => {
      if (concept.positions) {
        this.markers.push.apply(
          this.markers,
          concept.positions.map((pos: number) => {
            return new PdfMarker(concept.name, pos);
          })
        );
      }
    });
  }

}
