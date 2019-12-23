import { Component, OnInit, Output, EventEmitter, Input } from '@angular/core';
import { FormGroup, FormControl, Validators, FormArray, AbstractControl } from '@angular/forms';
import { MatDialog, MatSnackBar } from '@angular/material';
import { ImageCropDialogComponent } from '../../../../../shared/dialogs/image-crop/image-crop.dialog';
import { Event } from '../../../../../models/event.model';
import { UploadResource } from '../../../../../models/shared/upload-resource.interface';
import { SharedService } from '../../../../../shared/services/shared.service';
import { NotificationClass } from '../../../../../shared/classes/notification';
import { User } from 'src/app/settings/users/user-models/user';
import { SettingsUsersService } from 'src/app/settings/_services/users.service';
import * as pdfform from 'pdfform.js/pdfform';
import { HttpClient } from '@angular/common/http';
import { TutorInfo } from 'src/app/models/previews/tutor-info.interface';
import { environment } from 'src/environments/environment';

@Component({
  selector: 'app-new-event-event-info',
  templateUrl: './event-info.component.html',
  styleUrls: ['../new-event-steps.scss', './event-info.component.scss']
})
export class NewEventEventInfoComponent extends NotificationClass implements OnInit {

  @Input() readonly event: Event;
  @Output() setEventInfo = new EventEmitter();

  public formGroup: FormGroup;
  public users: Array<User> = [];
  public userName: string = '';
  public certificateTestDisabled: boolean = true;
  private _selectedUser: any;
  public certificate_url: string;
  public testedCertificate: boolean = false;
  public tutors: Array<TutorInfo> = [];
  public selectedTutors: Array<TutorInfo> = [];
  public hasEcommerceIntegration: boolean = environment.ecommerceIntegration;

  constructor(
    protected _snackBar: MatSnackBar,
    private _dialog: MatDialog,
    private _sharedService: SharedService,
    private _usersService: SettingsUsersService,
    private _httpClient: HttpClient
  ) {
    super(_snackBar);
  }

  ngOnInit() {
    this.formGroup = this._createFormGroup( this.event );
    if (this.event.certificateUrl) {
      this.certificate_url = this.event.certificateUrl;
      this.certificateTestDisabled = true;
      this.testedCertificate = true;
    }
    if (this.event.tutors) {
      this.selectedTutors = this.event.tutors;
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
          new FormControl( event.target.value )
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
      if (this._selectedUser) {
        if (this.certificateTestDisabled || (!this.certificateTestDisabled && this.testedCertificate)) {
          const eventInfo = this.formGroup.getRawValue();
          eventInfo.instructorId = this._selectedUser.id;
          eventInfo.instructor = this.userName;
          eventInfo.certificateUrl = this.certificate_url;
          if (this.selectedTutors.length > 0)
            eventInfo.tutorsIds = this.selectedTutors.map(x => x.id);
          this.setEventInfo.emit( eventInfo );
        } else {
          this.notify('Por favor, teste o certificado antes de continuar');
        }
      } else {
        this.notify('Por favor, selecione um instrutor para o evento');
      }
    } else {
      this.notify('Por favor, preencha todos os campos obrigatórios');
    }
  }

  public setEcommerceId(id?: number): void {
    this.formGroup.get('ecommerceId').setValue(id);
  }

  private _createFormGroup(event: Event): FormGroup {
    this.userName = event.instructor ? event.instructor : '';
    this._selectedUser = event.instructorId ? { id: event.instructorId } : null;
    return new FormGroup({
      'title': new FormControl(
        event ? event.title : '', [ Validators.required ]
      ),
      'storeUrl': new FormControl(
        event ? event.storeUrl : ''
      ),
      'createInEcommerce': new FormControl(
        event ? event.createInEcommerce : false
      ),
      'forceProblemStatement': new FormControl(
        event ? event.forceProblemStatement : false
      ),
      'ecommerceId': new FormControl(
        event && event.ecommerceId ? event.ecommerceId : ''
      ),
      'excerpt': new FormControl(
        event ? event.excerpt : '', [ Validators.required ]
      ),
      'instructorMiniBio': new FormControl(
        event ? event.instructorMiniBio : ''
      ),
      'imageUrl': new FormControl(
        event && event.imageUrl ? event.imageUrl : './assets/img/240x240-placeholder.png', [ Validators.required ]
      ),
      'instructorImageUrl': new FormControl(
        event && event.instructorImageUrl ? event.instructorImageUrl : './assets/img/240x240-placeholder.png', [ Validators.required ]
      ),
      'tags': new FormArray(
        event && event.tags ? event.tags.map(tag => new FormControl(tag)) : []
      )
    });
  }

  private _uploadImage(image: string, formControl: AbstractControl) {
    const resource: UploadResource = {
      data: image,
      filename: 'event.png',
      resource: 'event'
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
    this.event.instructorId = user.id;
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

  private _sendToServer(result: string, fileName: string) {
    const resource: UploadResource = {
      data: result,
      filename: fileName,
      resource: 'event_certificate'
    };

    this._sharedService.uploadFile(resource).subscribe((response) => {
      this.certificate_url = response.value.data.url;
      this.certificateTestDisabled = true;
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
        fields['nome_evento'] = [ 'Evento Teste' ];
        fields['nome_evento_pequeno'] = [ 'Evento Teste' ];
        fields['nome_aluno'] = [ 'Aluno Teste' ];
        fields['data_conclusao'] = [ '01/01/1990' ];
        fields['data_conclusao_extenso'] = [ '01 de Janeiro de 1990' ];
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
}
