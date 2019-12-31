import { ContentTypeEnum } from './enums/content-type.enum';

export class Content {
  public id: string;
  public type: ContentTypeEnum;
  public title: string;
  public excerpt: string;
  public duration: number;
  public referenceUrls: Array<string>;
  public moduleId?: string;
  public subjectId?: string;
  public concepts: Array<ConceptReference | VideoReference | PdfReference | TextReference>;
  public value: string;
  public originalValue?: string;
  public numPages?: number;

  public subjectTitle?: string;
  public accessed?: boolean;

  constructor(subjectId: string, concepts: Array<ConceptReference> = []) {
    this.subjectId = subjectId;
    this.concepts = concepts;
    this.referenceUrls = [''];
  }
}

export interface ConceptReference {
  name: string;
  checked?: boolean;
}

export interface VideoReference extends ConceptReference {
  positions: Array<number | string>;
}

export interface PdfReference extends ConceptReference {
  positions: Array<number | string>;
}

export interface TextReference extends ConceptReference {
  anchors: Array<string>;
}

export interface HtmlReference extends ConceptReference {
  positions: Array<number | string>;
}
