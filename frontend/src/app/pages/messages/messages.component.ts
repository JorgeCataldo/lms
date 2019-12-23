import { Component, OnInit } from '@angular/core';
import { NotificationClass } from '../../shared/classes/notification';
import { MatSnackBar } from '@angular/material';
import { FormGroup, FormControl, Validators } from '@angular/forms';
import { UserService } from '../_services/user.service';
import { ContactArea } from 'src/app/models/previews/contact-area.interface';
import { UploadResource } from 'src/app/models/shared/upload-resource.interface';
import { SharedService } from 'src/app/shared/services/shared.service';
import { environment } from 'src/environments/environment';

@Component({
  selector: 'app-messages',
  templateUrl: './messages.component.html',
  styleUrls: ['./messages.component.scss']
})
export class MessagesComponent extends NotificationClass implements OnInit {

  public formGroup: FormGroup;
  public areas: Array<ContactArea> = [];

  constructor(
    protected _snackBar: MatSnackBar,
    private _userService: UserService,
    private _sharedService: SharedService
  ) {
    super(_snackBar);
    this.formGroup = this._createForm();
  }

  ngOnInit() {
    this._userService.getContactAreas().subscribe((response) => {
      this.areas = response.data;

      if (!environment.features.career) {
        for ( let i = 0; i < this.areas.length; i++) {
          if ( this.areas[i].description === 'Carreiras') {
            this.areas.splice(i, 1);
          }
       }
      }

    }, () => {
      this.notify('Ocorreu um erro, por favor tente novamente mais tarde');
    });
  }

  public sendMail(): void {
    const formInfo = this.formGroup.getRawValue();
    this._userService.sendMessage(
      formInfo.areaId, formInfo.title, formInfo.message, formInfo.fileUrl
    ).subscribe(() => {
      this.formGroup = this._createForm();
      this.notify('Mensagem enviada com sucesso!');
    }, err => {
      this.notify(this.getErrorNotification(err));
    });
  }

  private _createForm(): FormGroup {
    return new FormGroup({
      'areaId': new FormControl('', [ Validators.required ]),
      'title': new FormControl('', [ Validators.required ]),
      'message': new FormControl('', [ Validators.required ]),
      'fileUrl': new FormControl(''),
      'fileName': new FormControl('')
    });
  }

  public openFileUpload(): void {
    (document.getElementById('inputFile') as HTMLElement).click();
  }

  private _sendToServer(result: string, fileName: string) {
    const resource: UploadResource = {
      data: result,
      filename: fileName,
      resource: 'messages'
    };

    this._sharedService.uploadFile(resource).subscribe((response) => {
      this.formGroup.get('fileName').setValue(resource.filename);
      this.formGroup.get('fileUrl').setValue(response.value.data.url);
      this.notify('Arquivo anexado com sucesso.');
    }, () => {
      this.notify('Ocorreu um erro ao anexar o arquivo, por favor tente novamente mais tarde');
    });
  }

  public setDocumentFile(files: FileList): void {
    const file = files.item(0);
    if (file.size > 10000000) {
      this.notify('O arquivo selecionado ultrapassa o tamanho limite de 10MB');
    } else {
      const callback = this._sendToServer.bind(this);
      const reader = new FileReader();
      reader.onloadend = function (e) {
        callback(
          this.result as string,
          file.name
        );
      };
      reader.readAsDataURL(file);
    }
  }
}
