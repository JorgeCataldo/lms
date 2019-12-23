import { AbstractControl, FormGroup } from '@angular/forms';
import { MatSnackBar } from '@angular/material';
import { UploadResource } from '../../models/shared/upload-resource.interface';
import { SharedService } from '../services/shared.service';
import { NotificationClass } from './notification';
import * as PDFJS from 'pdfjs-dist/build/pdf';
import { UploadService } from '../services/upload.service';
import { HttpEventType } from '@angular/common/http';
PDFJS.GlobalWorkerOptions.workerSrc = '/node_modules/pdfjs-dist/build/pdf.worker.js';

export class FileUploadClass extends NotificationClass {

  constructor(
    protected _snackBar: MatSnackBar,
    protected _sharedService: SharedService,
    protected _uploadService: UploadService
  ) {
    super(_snackBar);
  }

  public setDocumentFile(event, childFormGroup: FormGroup, isSupportMaterial: boolean = false) {
    if (event.target && event.target.files && event.target.files.length > 0) {

      if (event.target.files[0].size > 10000000) {
        this.notify('O arquivo selecionado ultrapassa o tamanho limite de 10MB');
        return;
      }

      if (isSupportMaterial) {
        const fileExtension = this._getFileExtension(event.target.files[0].name);
        if (fileExtension !== 'pdf') {
          this.uploadDoc(
            event.target.files[0],
            childFormGroup.get('downloadLink'),
            childFormGroup.get('fileName')
          );
          return;
        }
      }

      this.uploadPdf(
        event.target.files[0],
        childFormGroup.get('downloadLink'),
        childFormGroup.get('fileName'),
        childFormGroup.get('numPages')
      );
    }
  }

  public setZipFile(event, childFormGroup: FormGroup) {
    if (event.target && event.target.files && event.target.files.length > 0) {

      if (event.target.files[0].size > 50000000) {
        this.notify('O arquivo selecionado ultrapassa o tamanho limite de 50MB');
        return;
      }
      console.log('event.target.files[0] -> ', event.target.files[0]);
      this.uploadZip(
        event.target.files[0],
        childFormGroup.get('downloadLink'),
        childFormGroup.get('fileName')
      );
    }
  }

  protected uploadPdf(file, valueControl: AbstractControl, nameControl: AbstractControl, numPagesControl: AbstractControl) {
    const callback = this.sendPdfToServer.bind(this);
    const reader = new FileReader();
    reader.onloadend = function(e) {
      callback(
        this.result as string,
        file.name,
        valueControl,
        nameControl,
        numPagesControl
      );
    };
    reader.readAsDataURL(file);
  }

  protected uploadZip(file, valueControl: AbstractControl, nameControl: AbstractControl) {
    console.log('file -> ', file);
    const callback = this.sendZipToServer.bind(this);
    const reader = new FileReader();
    reader.onloadend = function(e) {
      callback(
        this.result as string,
        file,
        valueControl,
        nameControl
      );
    };
    reader.readAsDataURL(file);
  }

  protected uploadDoc(file, valueControl: AbstractControl, nameControl: AbstractControl) {
    const callback = this.sendDocToServer.bind(this);
    const reader = new FileReader();
    reader.onloadend = function(e) {
      callback(
        this.result as string,
        file.name,
        valueControl,
        nameControl
      );
    };
    reader.readAsDataURL(file);
  }

  protected sendDocToServer(result: string, fileName: string, valueControl: AbstractControl, nameControl: AbstractControl) {
    const resource: UploadResource = {
      data: result,
      filename: fileName,
      resource: 'module'
    };

    this._sharedService.uploadFile(resource).subscribe((response) => {
      valueControl.setValue(response.value.data.url);
      nameControl.setValue(fileName);
    }, () => {
      this.notify('Ocorreu um erro ao enviar o arquivo, por favor tente novamente mais tarde');
    });
  }

  protected sendPdfToServer(result: string, fileName: string,
    valueControl: AbstractControl, nameControl: AbstractControl, numPagesControl: AbstractControl) {

    const loadingTask = this._getPDFFromRawData(result);
    loadingTask.promise.then(function(pdf) {
      if (numPagesControl)
        numPagesControl.setValue( pdf.numPages );
    }, (reason) => console.error(reason));

    const resource: UploadResource = {
      data: result,
      filename: fileName,
      resource: 'module'
    };

    this._sharedService.uploadFile(resource).subscribe((response) => {
      valueControl.setValue(response.value.data.url);
      nameControl.setValue(fileName);
    }, () => {
      this.notify('Ocorreu um erro ao enviar o arquivo, por favor tente novamente mais tarde');
    });
  }

  protected sendZipToServer(result: string, file,
    valueControl: AbstractControl, nameControl: AbstractControl) {

    const resource: UploadResource = {
      data: result,
      filename: file.name,
      resource: 'module',
    };

    nameControl.setValue(file.name);

    this._uploadService.uploadFile(file).subscribe((response) => {
      console.log('this._uploadService.uploadFile - response -> ', response);
      valueControl.setValue(response);
      valueControl.setValue(response);
      this.notify('Arquivo enviado com sucesso!');
    }, () => {
      this.notify('Ocorreu um erro ao enviar o arquivo, por favor tente novamente mais tarde');
    });
  }

  private _getPDFFromRawData(rawData: string) {
    return PDFJS.getDocument({ data:
      atob(
        rawData.split('data:application/pdf;base64,')[1]
      )
    });
  }

  private _getFileExtension(fileName: string) {
    return fileName.split('.').pop();
  }
}
