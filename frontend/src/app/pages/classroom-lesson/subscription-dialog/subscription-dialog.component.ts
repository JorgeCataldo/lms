import { Component, Inject } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material';
import { UploadResource } from '../../../models/shared/upload-resource.interface';
import { SharedService } from '../../../shared/services/shared.service';
import { UserFile } from 'src/app/models/user-file.interface';
import { AuthService } from '../../../shared/services/auth.service';
import { SettingsUsersService } from '../../../settings/_services/users.service';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-subscription-dialog',
  templateUrl: './subscription-dialog.component.html',
  styleUrls: ['./subscription-dialog.component.scss']
})
export class SubscriptionDialogComponent {

public Questions: Array<any>;
public hasUpload: boolean = true;
private selectedFile: File;
public arquivos: Array<UserFile> = [];
private currentQuestionIndex: number = null;

  constructor(
    public dialogRef: MatDialogRef<SubscriptionDialogComponent>,
    private _sharedService: SharedService,
    private _settingsUsersService: SettingsUsersService,
    private _authService: AuthService,
    private _activatedRoute: ActivatedRoute,
    @Inject(MAT_DIALOG_DATA) public questions: Array<any> = []
  ) {
    this.Questions = questions;
  }

  public doSomething(event, question) {
    question.answer = event.target.value;
  }

  public dismiss(resolution: Array<any>): void {
    this.dialogRef.close( resolution );
  }

  public openFileUpload(index: number): void {
    this.currentQuestionIndex = index;
    (document.getElementById('inputEventFile' + index) as HTMLElement).click();
  }

  public setDocumentFile(event) {
    if (event.target && event.target.files && event.target.files.length > 0) {
      this.selectedFile = event.target.files[0];

      this._uploadFile(
        event.target.files[0],
        this.selectedFile.name
      );
    }
  }

  private _uploadFile(file, valueControl: string) {
    const callback = this._sendFileToServer.bind(this);
    const reader = new FileReader();
    reader.onloadend = function(e) {
      callback(
        this.result as string,
        file.name
      );
    };
    reader.readAsDataURL(file);
  }

  private _sendFileToServer(result: string, fileName: string) {
    const eventId = this._activatedRoute.snapshot.paramMap.get('eventId');
    const resource: UploadResource = {
      data: result,
      filename: fileName,
      resource: 'event_assesment'
    };

    this._sharedService.uploadFile(resource).subscribe((response) => {
      const arquivo: UserFile = {
        downloadLink: response.value.data.url,
        description: response.value.data.description || null,
        title: response.value.data.filename,
        id: null
      };

      this.arquivos.push(arquivo);
      const user = this._authService.getLoggedUser();

      this._settingsUsersService.addUserFiles(
        user.user_id,
        this.arquivos
      ).subscribe(() => {
        this.Questions[this.currentQuestionIndex].answer = arquivo.downloadLink;
        this.Questions[this.currentQuestionIndex].fileAsAnswer = true;
        this.Questions[this.currentQuestionIndex].fileName = arquivo.title || resource.filename;
        this.currentQuestionIndex = null;
        this.arquivos = [];
      });
        }, () => {
          this.arquivos = [];
    });
  }

}
