import { Component, Inject, OnInit } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA, MatSnackBar } from '@angular/material';
import { FormGroup, FormControl } from '@angular/forms';
import { Content, PdfReference } from '../../../../../../../models/content.model';
import { FileUploadClass } from '../../../../../../../shared/classes/file-upload';
import { SharedService } from '../../../../../../../shared/services/shared.service';
import { UploadService } from 'src/app/shared/services/upload.service';

@Component({
  selector: 'app-pdf-config-dialog',
  templateUrl: './pdf-config.dialog.html',
  styleUrls: ['../content-config.scss', './pdf-config.dialog.scss']
})
export class PdfConfigDialogComponent extends FileUploadClass implements OnInit {

  public formGroup: FormGroup;

  constructor(
    protected _snackbar: MatSnackBar,
    protected _sharedService: SharedService,
    protected _uploadService: UploadService,
    public dialogRef: MatDialogRef<PdfConfigDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public content: Content
  ) {
    super(_snackbar, _sharedService, _uploadService);
  }

  ngOnInit() {
    this.formGroup = new FormGroup({
      'downloadLink': new FormControl(
        this.content ? this.content.value : ''
      ),
      'fileName': new FormControl(
        this.content && this.content.value ? this.content.value.split('/module/')[1] : ''
      ),
      'numPages': new FormControl(
        this.content && this.content.numPages ? this.content.numPages : 1
      )
    });
  }

  public referencesTrackBy(index: number, obj: any) {
    return index;
  }

  public dismiss(): void {
    this.dialogRef.close();
  }

  public save(): void {
    const formValues = this.formGroup.getRawValue();
    this.content.value = formValues.downloadLink;
    this.content.numPages = formValues.numPages;
    this.dialogRef.close( this.content );
  }

  public addReference(): void {
    this.content.referenceUrls ?
      this.content.referenceUrls.push('') :
      this.content.referenceUrls = [''];
  }

  public setConcept(concept: PdfReference) {
    concept.checked = !concept.checked;
    if (concept.checked)
      concept.positions = [ 1 ];
  }

  public openFileUpload(): void {
    (document.getElementById('inputFile') as HTMLElement).click();
  }

}
