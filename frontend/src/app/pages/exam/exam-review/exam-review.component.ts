import { Component, Input, EventEmitter, Output, ViewChild } from '@angular/core';
import { ContentPreview } from '../../../models/previews/content.interface';
import { Content } from 'src/app/models/content.model';
import { ContentTypeEnum } from 'src/app/models/enums/content-type.enum';
import { PDFContentComponent } from '../../content/pdf/pdf.component';

@Component({
  selector: 'app-exam-review',
  templateUrl: './exam-review.component.html',
  styleUrls: ['./exam-review.component.scss']
})
export class ExamReviewComponent {

  @ViewChild('pdfContent') pdfContent: PDFContentComponent;
  @Input() readonly contents: Array<Content> = [];
  @Input() readonly concept: string;
  @Output() closeReview = new EventEmitter();
  @Output() resizeWindows = new EventEmitter<number>();
  @Output() setFinalOffset = new EventEmitter();

  public readonly contentTypeEnum = ContentTypeEnum;
  public selectedContent: Content;
  public conceptPosition: number = 0;
  public isDragging: boolean = false;
  private _initialPosX: number = 0;

  public viewContent(content: Content): void {
    this.selectedContent = content;
  }

  public finishReview(): void {
    this.closeReview.emit();
  }

  public goBack(): void {
    this.selectedContent ?
      this.selectedContent = null :
      this.finishReview();
  }

  public startDraggingBar(event): void {
    this._initialPosX = event.pageX;
    this.isDragging = true;
  }

  public endDraggingBar(): void {
    this.isDragging = false;
    this.setFinalOffset.emit();
  }

  public draggingBar(event): void {
    if (this.isDragging) {
      const offset = this._initialPosX - event.pageX;
      this.resizeWindows.emit( offset );
    }
  }

  public goToPosition(position: number): void {
    if (this.pdfContent)
      this.pdfContent.goToPage(position);
    this.conceptPosition = position;
  }

  public goToAnchor(anchor: string): void {
    const anchorElement: any = document.querySelector('a[href=\'' + anchor + '\']');
    document.querySelector('.htmlContent').scrollTop = anchorElement.offsetTop - 50;
  }

}
