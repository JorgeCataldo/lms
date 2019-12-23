import { Component, Input, Output, EventEmitter } from '@angular/core';
import { ContentPreview } from '../../../../models/previews/content.interface';

@Component({
  selector: 'app-content-list',
  templateUrl: './content-list.component.html',
  styleUrls: ['./content-list.component.scss']
})
export class ExamReviewContentListComponent {

  @Input() readonly concept;
  @Input() readonly contents: Array<ContentPreview> = [];
  @Output() selectContent = new EventEmitter<ContentPreview>();
  @Output() finishReview = new EventEmitter();

  public viewContent(content: ContentPreview): void {
    content.viewed = true;
    this.selectContent.emit(content);
  }

  public closeReview(): void {
    this.finishReview.emit();
  }

}
