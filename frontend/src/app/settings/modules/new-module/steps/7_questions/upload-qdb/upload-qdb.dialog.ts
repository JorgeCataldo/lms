import { Component, Inject } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA, MatSnackBar } from '@angular/material';
import { NotificationClass } from '../../../../../../shared/classes/notification';
import { SharedService } from 'src/app/shared/services/shared.service';
import { Module } from 'src/app/models/module.model';
import { SettingsModulesDraftsService } from 'src/app/settings/_services/modules-drafts.service';

@Component({
  selector: 'app-upload-qbd-dialog',
  templateUrl: './upload-qdb.dialog.html',
  styleUrls: ['./upload-qdb.dialog.scss']
})
export class UploadQuestionDatabaseDialogComponent extends NotificationClass {

  public module: Module;
  public addQuestions = true;

  constructor(
    protected _snackBar: MatSnackBar,
    public dialogRef: MatDialogRef<UploadQuestionDatabaseDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: { module: Module },
    private _draftsService: SettingsModulesDraftsService,
    public _sharedService: SharedService
  ) {
    super(_snackBar);
    this.module = this.data.module;
  }

  public dismiss(): void {
    this.dialogRef.close();
  }

  public openFileUpload(): void {
    (document.getElementById('qdbInputFile') as HTMLElement).click();
  }

  public setDocumentFile(event) {
    if (event.target && event.target.files && event.target.files.length > 0) {
      this._uploadPdf(
        event.target.files[0]
      );
    }
  }

  private _uploadPdf(file) {
    const callback = this._importQdb.bind(this);
    const reader = new FileReader();
    reader.onloadend = function (e) {
      callback(
        this.result as string
      );
    };
    reader.readAsDataURL(file);
  }

  private _importQdb(file: string) {
    this._draftsService.importDraftQdb(
      file, this.module.id, this.addQuestions
    ).subscribe(() => {
      this.notify('Questões importadas com sucesso!');
      this.dismiss();
    }, (err) => this.notify( this.getErrorNotification(err) ));
  }
}
