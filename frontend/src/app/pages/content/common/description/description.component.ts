import { Component, Input, Output, EventEmitter, OnInit, AfterViewInit, OnDestroy } from '@angular/core';
import { Content, ConceptReference } from '../../../../models/content.model';
import { UtilService } from '../../../../shared/services/util.service';
import { ContentTypeEnum } from '../../../../models/enums/content-type.enum';

@Component({
  selector: 'app-content-description',
  templateUrl: './description.component.html',
  styleUrls: ['./description.component.scss']
})
export class ContentDescriptionComponent implements AfterViewInit, OnDestroy {

  @Input() readonly content: Content;
  @Input() readonly resumedView: boolean = false;
  @Input() readonly contentNote: string = '';

  @Output() goToPosition = new EventEmitter<number>();
  @Output() goToAnchor = new EventEmitter<string>();
  @Output() saveConceptViewAction = new EventEmitter<string>();
  @Output() saveUserContentNote = new EventEmitter<string>();

  public viewOptions = [
    { selected: true, title: 'INFORMAÇÕES' },
    { selected: false, title: 'ANOTAÇÕES' }
  ];
  public showContent: boolean = true;
  private timeout = null;

  constructor(
    private _utilService: UtilService
  ) { }

  ngAfterViewInit() {
    this.setDescriptionContentHeight();
    document.body.style.overflow = 'hidden';
    setTimeout(() => {
      this.toggleShowContent();
    });
  }

  public selectViewOption(optTitle: string) {
    this.viewOptions.forEach(opt => { opt.selected = false; });
    this.viewOptions.find(x => x.title === optTitle).selected = true;
    setTimeout(() => {
      this.setDescriptionContentHeight();
    }, 100);
  }

  public updateNote(noteTextValue: string) {
    clearTimeout(this.timeout);
    this.timeout = setTimeout(() => {
      this.saveUserContentNote.next( noteTextValue );
    }, 3000);
  }

  public isViewOption(title: string): boolean {
    return this.viewOptions.find(x => x.title === title).selected;
  }

  public getFormattedDuration(): string {
    return this._utilService.formatSecondsToMinutes(this.content.duration);
  }

  public getReferenceUrls() {
    if (!this.content || !this.content.referenceUrls) return [];
    return this.content.referenceUrls.filter(ref => ref.trim() !== '');
  }

  public getMarkedConcepts() {
    if (!this.content || !this.content.concepts) return [];
    return this._orderConcepts(
      this._getFilteredConcepts()
    );
  }

  private _orderConcepts(concepts) {
    return concepts.sort((a, b) => {
      if (!a.positions || a.positions.length === 0)
        return 0;
      else if (a.positions[0] < b.positions[0])
        return -1;
      else if (a.positions[0] > b.positions[0])
        return 1;
      return 0;
    });
  }

  private _getFilteredConcepts() {
    return this.content.concepts.filter((concept: any) => {
      if (concept.positions)
        concept.positions = concept.positions.filter(pos => pos < this.content.duration);

      return (concept.positions && concept.positions.length > 0) ||
        (concept.anchors && concept.anchors.length > 0);
    });
  }

  public getConceptTag(position: number): string {
    switch (this.content.type) {
      case ContentTypeEnum.Video:
        return 'ver ' + this._utilService.formatSecondsToMinutes(position) + ' min';

      case ContentTypeEnum.Pdf:
        return 'ir para a página ' + position;

      case ContentTypeEnum.Text:
        return 'ir para conceito no texto';

      default:
        return '';
    }
  }

  public goToContentPosition(position, concept: ConceptReference): void {
    if (this.content.type === ContentTypeEnum.Pdf ||
        this.content.type === ContentTypeEnum.Video
    ) {
      this.goToPosition.emit(position);
    } else
      this.goToAnchor.emit(position);

    this.saveConceptViewAction.emit(concept.name);
  }

  public toggleShowContent() {
    this.showContent = !this.showContent;
    setTimeout(() => {
      this.showContent ?
        this.setDescriptionContentHeight() :
        this._setNoDescriptionContentHeight();
    });
  }

  public getContentElementByType(): HTMLElement {
    switch (this.content.type) {
      case ContentTypeEnum.Video:
        return document.getElementById('videoContent');

      case ContentTypeEnum.Pdf:
        return document.querySelector('.pdf-content');

      case ContentTypeEnum.Text:
        return document.querySelector('.htmlContent');

      default:
        return null;
    }
  }

  public setDescriptionContentHeight(): void {
    const descriptionElement: HTMLElement = document.getElementById('ContentDescription');
    const contentElement: HTMLElement = this.getContentElementByType();
    if (descriptionElement && contentElement) {
      let totalOffset = (this.resumedView ? 193 : 220) + descriptionElement.offsetHeight;

      if (this.content.type === ContentTypeEnum.Video)
        totalOffset = totalOffset + (this.resumedView ? -5 : 25);
      else if (this.content.type === ContentTypeEnum.Text)
        totalOffset = totalOffset + 50;

      contentElement.style.height = 'calc(100vh - ' + totalOffset + 'px)';
    }
  }

  private _setNoDescriptionContentHeight(): void {
    const contentElement: HTMLElement = this.getContentElementByType();
    if (contentElement) {

      let offset = this.resumedView ? 193 : 220;
      if (this.content.type === ContentTypeEnum.Video)
        offset = offset + (this.resumedView ? -5 : 25);
      else if (this.content.type === ContentTypeEnum.Text)
        offset = offset + 50;

      contentElement.style.height = 'calc(100vh - ' + offset + 'px)';
    }
  }

  ngOnDestroy() {
    document.body.style.overflow = 'initial';
  }

}
