import { Component, Inject, OnInit } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material';

import * as Editor from 'tui-editor';
import { Content, TextReference } from '../../../../../../../models/content.model';

@Component({
  selector: 'app-text-config-dialog',
  templateUrl: './text-config.dialog.html',
  styleUrls: ['../content-config.scss', './text-config.dialog.scss']
})
export class TextConfigDialogComponent implements OnInit {

  public editor: Editor;

  constructor(
    public dialogRef: MatDialogRef<TextConfigDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public content: Content
  ) { }

  ngOnInit() {
    this.editor = new Editor({
        el: document.querySelector('#htmlEditor'),
        initialEditType: 'markdown',
        previewStyle: 'vertical',
        height: '300px'
    });

    this.editor.setMarkdown(
      this.content.value
    );
  }

  public referencesTrackBy(index: number, obj: any) {
    return index;
  }

  public dismiss(): void {
    this.dialogRef.close();
  }

  public save(): void {
    this.content.value = this.editor.getMarkdown();
    this.dialogRef.close( this.content );
  }

  public addReference(): void {
    this.content.referenceUrls ?
      this.content.referenceUrls.push('') :
      this.content.referenceUrls = [''];
  }

  public setConcept(concept: TextReference) {
    concept.checked = !concept.checked;
    if (concept.checked)
      concept.anchors = [ '#' ];
  }

}
