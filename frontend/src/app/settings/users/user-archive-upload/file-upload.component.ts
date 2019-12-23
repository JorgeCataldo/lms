import { Component, Output, EventEmitter, OnInit } from '@angular/core';
import { UploadResource } from 'src/app/models/shared/upload-resource.interface';
import { FormGroup, FormArray, FormControl, Validators, AbstractControl } from '@angular/forms';
import { NotificationClass } from 'src/app/shared/classes/notification';
import { MatSnackBar } from '@angular/material';
import { SharedService } from 'src/app/shared/services/shared.service';
import { SupportMaterial } from 'src/app/models/support-material.interface';
import { SettingsUsersService } from '../../_services/users.service';
import { AuthService } from 'src/app/shared/services/auth.service';
import { UserFile } from 'src/app/models/user-file.interface';
import { ActivatedRoute } from '@angular/router';

@Component({
    templateUrl: './file-upload.component.html',
    styleUrls: ['./file-upload.component.scss']
})

export class FileUploadComponent extends NotificationClass implements OnInit {

  @Output() addSupportMaterials = new EventEmitter<Array<UserFile>>();

  public formGroup: FormGroup;
  public contador: number = 0;
  public arquivos: UserFile[];
  public loading: boolean = false;

  constructor(
    protected _snackBar: MatSnackBar,
    private _sharedService: SharedService,
    private _settingsUsersService: SettingsUsersService,
    private _activatedRoute: ActivatedRoute
  ) {
    super(_snackBar);
  }

  ngOnInit() {
    this.loading = true;
    this._loadUserFiles();
  }

  public addMaterial(): void {
    if (this.contador >= 10) {
      return null;
    }
    const materials = this.formGroup.get('materials') as FormArray;
    materials.push(
      this._createMaterialForm()
    );
    this.contador++;
  }

  public removeFile(index: number): void {
    if (this.loading) { return; }

    this.arquivos.splice(index, 1);
    this.loading = true;
    this._settingsUsersService.manageUserFiles(
      this._activatedRoute.snapshot.paramMap.get('userId'),
      this.arquivos
    ).subscribe(() => {
      this.notify('Documento removido com sucesso.');
      this._loadUserFiles();
    });
  }

  private _setMaterialsFormArray(): FormArray {
    return new FormArray(
      this.arquivos.map((mat) => this._createMaterialForm(mat))
    );
  }

  private _createMaterialForm(material: SupportMaterial = null): FormGroup {
    return new FormGroup({
      'id': new FormControl(material ? material.id : null),
      'title': new FormControl(material ? material.title : '', [Validators.required]),
      'downloadLink': new FormControl(material ? material.downloadLink : ''),
      'description': new FormControl(material ? material.description : '')
    });
  }

  private _createFormGroup(): FormGroup {
    return new FormGroup({
      'materials': this._setMaterialsFormArray()
    });
  }

  public openFileUpload(index: number): void {
    (document.getElementById('inputFile' + index) as HTMLElement).click();
  }

  public setDocumentFile(files: FileList, childFormGroup: FormGroup): void {
    const file = files.item(0);
    const callback = this._sendToServer.bind(this);
    const reader = new FileReader();

    if (file.type.includes('application/pdf') || file.type.includes('image/')) {
      if (file.size / 1024 / 1024 <= 3) {
        reader.onloadend = function (e) {
          callback(
            this.result as string,
            file.name,
            childFormGroup
          );
        };
        reader.readAsDataURL(file);
      } else {
        this.notify('Erro no envio do arquivo ' + file.name + '. Tamanho maior do que 3 mb.');
        return null;
      }
    } else {
      this.notify('Erro no envio do arquivo ' + file.name + '. Formato inválido.');
      return null;
    }
  }

  private _sendToServer(result: string, fileName: string, childFormGroup: FormGroup) {
    const resource: UploadResource = {
      data: result,
      filename: fileName,
      resource: 'secretary'
    };

    this._sharedService.uploadFile(resource).subscribe((response) => {
      childFormGroup.get('downloadLink').setValue(response.value.data.url);
      this._saveUserFile(childFormGroup);

    }, (error) => this._handleError(error) );
  }

  private _loadUserFiles(): void {
    this._settingsUsersService.getUserArchive(
      this._activatedRoute.snapshot.paramMap.get('userId')
    ).subscribe(res => {
      this.arquivos = res.data;
      this.contador = this.arquivos.length;
      this.formGroup = this._createFormGroup();
      this.loading = false;
    }, (error) => this._handleError(error) );
  }

  private _saveUserFile(childFormGroup: FormGroup): void {
    this._fillFilesWithNew(childFormGroup);

    this.loading = true;
    this._settingsUsersService.manageUserFiles(
      this._activatedRoute.snapshot.paramMap.get('userId'),
      this.arquivos
    ).subscribe(() => {
      this.notify('Documentos enviados com sucesso.');
      this._loadUserFiles();
    });
  }

  private _fillFilesWithNew(childFormGroup: FormGroup): void {
    const fileId = childFormGroup.get('id').value;
    const currFile = this.arquivos.find(a => fileId && (a.id === fileId));

    if (currFile) {
      currFile.description = childFormGroup.get('description').value;
      currFile.downloadLink = childFormGroup.get('downloadLink').value;
      currFile.title = childFormGroup.get('title').value;

    } else {
      this.arquivos.push({
        description: childFormGroup.get('description').value,
        downloadLink: childFormGroup.get('downloadLink').value,
        title: childFormGroup.get('title').value
      });
    }
  }

  private _handleError(error): void {
    this.notify(
      this.getErrorNotification(error)
    );
    this.loading = false;
  }
}
