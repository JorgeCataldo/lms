import { Component, Input, Output, EventEmitter, OnInit } from '@angular/core';
import { MatSnackBar, MatDialog } from '@angular/material';
import { NotificationClass } from '../../../../../shared/classes/notification';
import { Content } from '../../../../../models/content.model';
import { Module } from '../../../../../models/module.model';
import { VideoConfigDialogComponent } from './content-config/video-config/video-config.dialog';
import { PdfConfigDialogComponent } from './content-config/pdf-config/pdf-config.dialog';
import { TextConfigDialogComponent } from './content-config/text-config/text-config.dialog';
import { Subject } from '../../../../../models/subject.model';
import { ContentTypeEnum } from '../../../../../models/enums/content-type.enum';
import { UtilService } from '../../../../../shared/services/util.service';
import { ZipConfigDialogComponent } from './content-config/zip-config/zip-config.dialog';

@Component({
  selector: 'app-new-module-contents',
  templateUrl: './contents.component.html',
  styleUrls: ['../new-module-steps.scss', './contents.component.scss']
})
export class NewModuleContentsComponent extends NotificationClass implements OnInit {

  @Input() module: Module;
  @Output() addContents = new EventEmitter<Array<Content>>();

  public contentTypeEnum = ContentTypeEnum;
  public newContent: Content;

  constructor(
    protected _snackBar: MatSnackBar,
    private _dialog: MatDialog,
    private _utilService: UtilService
  ) {
    super(_snackBar);
  }

  ngOnInit() {
    this._adjustContents();
  }

  public addContent(subject: Subject): void {
    const concepts = subject.concepts ? subject.concepts : [];
    const content = new Content(
      subject.id,
      concepts.map((concept: string) => ({ 'name': concept }))
    );
    subject.contents = subject.contents ? subject.contents : [];
    subject.contents.push( content );
  }

  public removeContent(subject: Subject, index: number): void {
    subject.contents.splice(index, 1);
  }

  public openConfigDialog(content: Content, subject: Subject) {
    content.subjectTitle = subject.title;
    content = this._adjustContent(content, subject);
    const dialog = this._getConfigDialog( content ) as any;
    const dialogRef = this._dialog.open(dialog, {
      width: '1000px',
      data: content
    });

    dialogRef.afterClosed().subscribe((result: Content) => {
      if (result) {
        delete result.subjectTitle;
        content = result;
      }
    });
  }

  public nextStep(): void {
    const validationError = this._checkSubjectsInfo();
    if (validationError === null) {
      const contents: Array<Content> = [];
      this.module.subjects.forEach(sub => contents.push.apply(contents, sub.contents));
      this.addContents.emit(
        this._adjustDuration( contents )
      );

    } else this.notify(validationError);
  }

  public isString(val): boolean {
    return typeof val === 'string';
  }

  private _getConfigDialog(content: Content) {
    switch (content.type) {
      case ContentTypeEnum.Video:
        return VideoConfigDialogComponent;
      case ContentTypeEnum.Pdf:
        return PdfConfigDialogComponent;
      case ContentTypeEnum.Text:
        return TextConfigDialogComponent;
      case ContentTypeEnum.Zip:
        return ZipConfigDialogComponent;
      default:
        return TextConfigDialogComponent;
    }
  }

  private _adjustDuration(contents: Array<Content>): Array<Content> {
    contents.forEach((content) => {
      if (typeof content.duration === 'string') {
        content.duration = this._utilService.getDurationFromFormattedHour(
          content.duration as any
        );
      }
      if (content.concepts) {
        content.concepts.forEach((conc: any) => {
          if (conc.positions && conc.positions.length > 0) {
            conc.positions = conc.positions.map((pos) =>
              typeof pos === 'string' ?
                this._utilService.getDurationFromFormattedHour(pos) : pos
            );
          }
        });
      }
    });
    return contents;
  }

  private _checkSubjectsInfo(): string {
    if (!this.module.subjects || this.module.subjects.length === 0) return null;

    let validationError = null;
    this.module.subjects.forEach(subj => {
      if (subj.contents && subj.contents.length > 0) {
        subj.contents.forEach((cont, index: number) => {
          if (!cont || !cont.title || (cont.type == null) || !cont.duration || !cont.value) {
            validationError = 'O Conteúdo \'' + (cont.title ? cont.title : (index + 1)) +
              '\' do Assunto \'' + subj.title +
              '\' possui campos ausentes de preenchimento obrigatório';
          } else if (cont.concepts && cont.concepts.length > 0) {
            const hasError = cont.concepts.some((conc: any) =>
              conc.positions && conc.positions.some(pos => !pos)
            );
            if (hasError) {
              validationError = 'O Conteúdo \'' + (cont.title ? cont.title : (index + 1)) +
                '\' do Assunto \'' + subj.title +
                '\' possui informações em \'Conceitos\' ausentes de preenchimento obrigatório';
            }
          }
        });
      }
    });

    return validationError;
  }

  private _adjustContents() {
    if (this.module && this.module.subjects) {
      this.module.subjects.forEach(subject => {
        if (subject.contents) {
          subject.contents.forEach((content: any) => {
            content.subjectId = subject.id;
            if (typeof content.duration !== 'string')
              content.duration = this._utilService.formatDurationToHour(content.duration);
            content.concepts.forEach(conc => conc.checked = true);
          });
        }
      });
    }
  }

  private _adjustContent(content, subject: Subject) {
    content.subjectId = subject.id;

    if (typeof content.duration !== 'string')
      content.duration = this._utilService.formatDurationToHour(content.duration);

    content.concepts = subject.concepts.map((concept: string) => {
      const existingConcept = this._retrieveExistingConcept(content.concepts, concept);
      return {
        name: concept,
        checked: !!existingConcept && (existingConcept.positions || existingConcept.anchors),
        positions: existingConcept ? existingConcept.positions : null,
        anchors: existingConcept ? existingConcept.anchors : null
      };
    });

    switch (content.type) {
      case ContentTypeEnum.Video:
        content = this._adjustVideoContent(content);
        break;
      case ContentTypeEnum.Pdf:
        content = this._adjustPDFContent(content);
        break;
      case ContentTypeEnum.Zip:
        content = this._adjustZipContent(content);
        break;
      case ContentTypeEnum.Text:
      default:
        content = this._adjustTextContent(content);
        break;
    }

    return content;
  }

  private _retrieveExistingConcept(concepts, name: string) {
    return concepts ? concepts.find(concept => concept.name === name) : null;
  }

  private _adjustVideoContent(content) {
    content.concepts.forEach(conc => {
      if (conc.positions && conc.positions.length > 0) {
        conc.checked = true;
        conc.positions = conc.positions.map((pos) =>
          typeof pos === 'string' ? pos :
          this._utilService.formatDurationToHour(pos)
        );
      }
    });
    return content;
  }

  private _adjustPDFContent(content) {
    content.concepts.forEach(conc => {
      conc.checked = conc.positions && conc.positions.length > 0;
    });
    return content;
  }

  private _adjustZipContent(content) {
    content.concepts.forEach(conc => {
      conc.checked = conc.positions && conc.positions.length > 0;
    });
    return content;
  }

  private _adjustTextContent(content) {
    content.concepts.forEach(conc => {
      conc.checked = conc.anchors && conc.anchors.length > 0;
    });
    return content;
  }

}
