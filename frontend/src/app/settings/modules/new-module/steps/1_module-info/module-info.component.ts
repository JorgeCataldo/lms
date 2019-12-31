import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { FormGroup, FormControl, Validators, FormArray, AbstractControl } from '@angular/forms';
import { Module } from '../../../../../models/module.model';
import { MatDialog, MatSnackBar } from '@angular/material';
import { ImageCropDialogComponent } from '../../../../../shared/dialogs/image-crop/image-crop.dialog';
import { NotificationClass } from '../../../../../shared/classes/notification';
import { UtilService } from '../../../../../shared/services/util.service';
import { UploadResource } from '../../../../../models/shared/upload-resource.interface';
import { SharedService } from '../../../../../shared/services/shared.service';
import { SettingsUsersService } from 'src/app/settings/_services/users.service';
import { User } from 'src/app/settings/users/user-models/user';
import * as pdfform from 'pdfform.js/pdfform';
import { HttpClient } from '@angular/common/http';
import { TutorInfo } from 'src/app/models/previews/tutor-info.interface';
import { environment } from 'src/environments/environment';

@Component({
  selector: 'app-new-module-module-info',
  templateUrl: './module-info.component.html',
  styleUrls: ['../new-module-steps.scss', './module-info.component.scss']
})
export class NewModuleModuleInfoComponent extends NotificationClass implements OnInit {

  @Input() readonly module: Module;
  @Input() readonly showCertification: boolean = false;
  @Output() setModuleInfo = new EventEmitter();

  public formGroup: FormGroup;
  public users: Array<User> = [];
  public userName: string = '';
  public certificateTestDisabled: boolean = true;
  private _selectedUser: any;
  public certificate_url: string;
  public testedCertificate: boolean = false;
  public tutors: Array<TutorInfo> = [];
  public selectedTutors: Array<TutorInfo> = [];
  public extraInstructors: Array<TutorInfo> = [];
  public selectedExtraInstructors: Array<TutorInfo> = [];
  public hasEcommerceIntegration: boolean = environment.ecommerceIntegration;

  constructor(
    protected _snackBar: MatSnackBar,
    private _dialog: MatDialog,
    private _utillService: UtilService,
    private _sharedService: SharedService,
    private _usersService: SettingsUsersService,
    private _httpClient: HttpClient
  ) {
    super(_snackBar);
  }

  ngOnInit() {
    this.formGroup = this._createFormGroup( this.module );
    if (this.module.certificateUrl) {
      this.certificate_url = this.module.certificateUrl;
      this.certificateTestDisabled = false;
      this.testedCertificate = true;
    }
    if (this.module.tutors) {
      this.selectedTutors = this.module.tutors;
    }
    if (this.module.extraInstructors) {
      this.selectedExtraInstructors = this.module.extraInstructors;
    }
  }

  public uploadImage(imageWidth: number, imageHeight: number, controlName: string) {
    const dialogRef = this._dialog.open(ImageCropDialogComponent, {
      width: window.innerWidth < 768 ? '100vw' : '50vw',
      data: {
        'canvasWidth': window.innerWidth < 768 ? 250 : 300,
        'canvasHeight': window.innerWidth < 768 ? 250 : 300,
        'croppedWidth': imageWidth,
        'croppedHeight': imageHeight
      }
    });
    dialogRef.afterClosed().subscribe(result => {
      if (result && result.image) {
        this._uploadImage(
          result.image,
          this.formGroup.get(controlName)
        );
      }
    });
  }

  public addTag(event): void {
    const tag = event.target.value;
    if (tag && tag.trim() !== '') {

      if (tag.length > 30) {
        this.notify('As TAGs devem ter no máximo 30 caracteres');
      } else {

        const tagsArray = this.formGroup.get('tags') as FormArray;
        tagsArray.push(
          new FormControl( tag )
        );
        event.target.value = '';
      }
    }
  }

  public removeTag(index: number): void {
    const tagsArray = this.formGroup.get('tags') as FormArray;
    tagsArray.removeAt(index);
  }

  public nextStep(): void {
    if (this.formGroup.valid) {
      if (this._selectedUser || this.userName) {
        if (this.certificateTestDisabled || (!this.certificateTestDisabled && this.testedCertificate)) {
          const moduleInfo = this.formGroup.getRawValue();
          moduleInfo.instructorId = this._selectedUser ? this._selectedUser.id : null;
          moduleInfo.instructor = this.userName;
          moduleInfo.certificateUrl = this.certificate_url;
          if (this.selectedTutors.length > 0)
            moduleInfo.tutorsIds = this.selectedTutors.map(x => x.id);
          if (this.selectedExtraInstructors.length > 0)
            moduleInfo.extraInstructorIds = this.selectedExtraInstructors.map(x => x.id);
          this.setModuleInfo.emit( moduleInfo );
        } else {
          this.notify('Por favor, teste o certificado antes de continuar');
        }
      } else {
        this.notify('Por favor, selecione um instrutor para o evento');
      }
    } else {
      this.formGroup = this._utillService.markFormControlsAsTouch( this.formGroup );
      this.notify('Por favor, preencha todos os campos obrigatórios');
    }
  }

  private _createFormGroup(module: Module): FormGroup {
    this.userName = module.instructor ? module.instructor : '';
    this._selectedUser = module.instructorId ? { id: module.instructorId } : null;

    return new FormGroup({
      'title': new FormControl(
        module ? module.title : '', [ Validators.required ]
      ),
      'storeUrl': new FormControl(
        module ? module.storeUrl : ''
      ),
      'ecommerceUrl': new FormControl(
        module ? module.ecommerceUrl : ''
      ),
      'createInEcommerce': new FormControl(
        module ? module.createInEcommerce : false
      ),
      'ecommerceId': new FormControl(
        module && module.ecommerceId ? module.ecommerceId : ''
      ),
      'published': new FormControl(
        module ? module.published : true, [ Validators.required ]
      ),
      'excerpt': new FormControl(
        module ? module.excerpt : '', [ Validators.required ]
      ),
      'instructorMiniBio': new FormControl(
        module ? module.instructorMiniBio : ''
      ),
      'imageUrl': new FormControl(
        module && module.imageUrl ? module.imageUrl : './assets/img/420x210-placeholder.png',
        [ Validators.required ]
      ),
      'instructorImageUrl': new FormControl(
        module && module.instructorImageUrl ? module.instructorImageUrl : './assets/img/240x240-placeholder.png',
        [ Validators.required ]
      ),
      'tags': new FormArray(
        module && module.tags ? module.tags.map(tag => new FormControl(tag)) : []
      ),
      'validFor': new FormControl(
        module && module.validFor ? module.validFor : ''
      ),
    });
  }

  private _uploadImage(image: string, formControl: AbstractControl) {
    const resource: UploadResource = {
      data: image,
      filename: 'module.png',
      resource: 'module'
    };

    this._sharedService.uploadImage(resource).subscribe((response) => {
      formControl.setValue(response.data.imageUrl);
    }, () => {
      this.notify('Ocorreu um erro ao enviar a imagem, por favor tente novamente mais tarde');
    });
  }

  public triggerUserSearch(searchValue: string) {
    if (searchValue && searchValue.trim() !== '') {
      this._loadUsers( searchValue );

      this._selectedUser = { 'name': searchValue };
      this.userName = searchValue;
    }
  }

  private _loadUsers(searchValue: string = ''): void {
    this._usersService.getProfessors(searchValue).subscribe(response => {
      this.users = response.data;
    });
  }

  public addUser(user: User) {
    this.module.instructorId = user.id;
    if (user.imageUrl)
      this.formGroup.get('instructorImageUrl').setValue(user.imageUrl);

    this._selectedUser = user;
    this.userName = user.name;
    this.users = [];
  }

  public setDocumentFile(files: FileList) {
    const file = files.item(0);
    const callback = this._sendToServer.bind(this);
    const reader = new FileReader();
    reader.onloadend = function(e) {
      callback(
        this.result as string,
        file.name,
        file.name,
        file.name
      );
    };
    reader.readAsDataURL(file);
  }

  public setEcommerceId(id?: number): void {
    this.formGroup.get('ecommerceId').setValue(id);
  }

  private _sendToServer(result: string, fileName: string) {
    const resource: UploadResource = {
      data: result,
      filename: fileName,
      resource: 'module_certificate'
    };

    this._sharedService.uploadFile(resource).subscribe((response) => {
      this.certificate_url = response.value.data.url;
      this.certificateTestDisabled = false;
      this.testedCertificate = false;
      this.notify('Arquivo enviado com sucesso.');
    }, () => {
      this.notify('Ocorreu um erro ao enviar o arquivo, por favor tente novamente mais tarde');
    });
  }

  public generateCertificatePDF(): void {
    this._httpClient.get(
      this.certificate_url, { responseType: 'arraybuffer' }
    ).subscribe(
      (response) => {
        const fields = { };
        fields['nome_modulo'] = [ 'Modulo Teste' ];
        fields['nome_aluno'] = [ 'Aluno Teste' ];
        fields['data_conclusao'] = [ '01/01/1990' ];
        fields['data_conclusao_extenso'] = [ '01 de Janeiro de 1990' ];
        fields['nivel'] = [ 'Expert' ];
        const out_buf = pdfform().transform(response, fields);

        const blob = new Blob([out_buf], { type: 'application/pdf' });
        const fileURL = URL.createObjectURL(blob);
        window.open(fileURL);
        this.testedCertificate = true;
    }, () => {
      this.notify('Ocorreu um erro ao carregar o certificado');
    });
  }

  private _loadTutors(searchValue: string = ''): void {
    this._usersService.getProfessors(searchValue).subscribe(response => {
      this.tutors = response.data;
    });
  }

  public triggerTutorSearch(searchValue: string) {
    if (searchValue && searchValue.trim() !== '')
      this._loadTutors( searchValue );
    else
      this.resetTutorSearch();
  }

  public resetTutorSearch(): void {
    this.tutors = [];
  }

  public addTutor(user: TutorInfo) {
    const tutorExists = this.selectedTutors.find(u => u.id === user.id);
    if (!tutorExists)
      this.selectedTutors.push( user );
    this.resetTutorSearch();
  }

  public removeSelectedTutor(user: TutorInfo) {
    const index = this.selectedTutors.findIndex(x => x.id === user.id);
    this.selectedTutors.splice(index , 1);
  }

  private _loadExtraInstructors(searchValue: string = ''): void {
    this._usersService.getProfessors(searchValue).subscribe(response => {
      this.extraInstructors = response.data;
    });
  }

  public triggerExtraInstructorSearch(searchValue: string) {
    if (searchValue && searchValue.trim() !== '')
      this._loadExtraInstructors( searchValue );
    else
      this.resetExtraInstructorSearch();
  }

  public resetExtraInstructorSearch(): void {
    this.extraInstructors = [];
  }

  public addExtraInstructor(user: TutorInfo) {
    const extraInstructorExists = this.selectedExtraInstructors.find(u => u.id === user.id);
    if (!extraInstructorExists)
      this.selectedExtraInstructors.push( user );
    this.resetExtraInstructorSearch();
  }

  public removeSelectedExtraInstructor(user: TutorInfo) {
    const index = this.selectedExtraInstructors.findIndex(x => x.id === user.id);
    this.selectedExtraInstructors.splice(index , 1);
  }
}
