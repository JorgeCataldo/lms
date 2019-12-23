import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { FormGroup, FormControl, Validators, FormArray, AbstractControl } from '@angular/forms';
import { MatDialog, MatSnackBar } from '@angular/material';
import { ImageCropDialogComponent } from '../../../../../shared/dialogs/image-crop/image-crop.dialog';
import { NotificationClass } from '../../../../../shared/classes/notification';
import { UtilService } from '../../../../../shared/services/util.service';
import { UploadResource } from '../../../../../models/shared/upload-resource.interface';
import { SharedService } from '../../../../../shared/services/shared.service';
import { Track } from '../../../../../models/track.model';
import * as pdfform from 'pdfform.js/pdfform';
import { HttpClient } from '@angular/common/http';
import { environment } from 'src/environments/environment';
import { SettingsProfileTestsService } from 'src/app/settings/_services/profile-tests.service';
import { ProfileTest } from 'src/app/models/profile-test.interface';

@Component({
  selector: 'app-new-track-track-info',
  templateUrl: './track-info.component.html',
  styleUrls: ['../new-track-steps.scss', './track-info.component.scss']
})
export class NewTrackTrackInfoComponent extends NotificationClass implements OnInit {

  @Input() readonly track: Track;
  @Output() setTrackInfo = new EventEmitter<Track>();

  public formGroup: FormGroup;
  public certificateTestDisabled: boolean = true;
  public certificate_url: string;
  public testedCertificate: boolean = false;
  public hasEcommerceIntegration: boolean = environment.ecommerceIntegration;
  public hasProfileTest: boolean = environment.features.profileTest;
  public tests: Array<ProfileTest>;
  public selectedTest: string = '';

  constructor(
    protected _snackBar: MatSnackBar,
    private _dialog: MatDialog,
    private _utillService: UtilService,
    private _sharedService: SharedService,
    private _httpClient: HttpClient,
    private _testsService: SettingsProfileTestsService
  ) {
    super(_snackBar);
  }

  ngOnInit() {
    this.formGroup = this._createFormGroup( this.track );
    if (this.track.certificateUrl) {
      this.certificate_url = this.track.certificateUrl;
      this.certificateTestDisabled = false;
      this.testedCertificate = true;
    }
    if (this.track.profileTestId && this.track.profileTestName) {
      this.selectedTest = this.track.profileTestName;
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
      if (this.certificateTestDisabled || (!this.certificateTestDisabled && this.testedCertificate)) {
        const trackInfo = this.formGroup.getRawValue();
        trackInfo.certificateUrl = this.certificate_url;
        this.setTrackInfo.emit( trackInfo );
      } else {
        this.notify('Por favor, teste o certificado antes de continuar');
      }
    } else {
      this.formGroup = this._utillService.markFormControlsAsTouch( this.formGroup );
      this.notify('Por favor, preencha todos os campos obrigatórios');
    }
  }

  public triggerTestSearch(searchValue: string) {
    if (searchValue && searchValue.trim() !== '') {
      this._loadTests( searchValue );
      this.selectedTest = searchValue;
    } else
      this.tests = null;
  }

  public setTest(test: ProfileTest): void {
    this.selectedTest = test.title;
    this.formGroup.get('profileTestId').setValue(test.id);
    this.formGroup.get('profileTestName').setValue(test.title);
    this.tests = null;
  }

  public removeTest(): void {
    this.formGroup.get('profileTestId').setValue('');
    this.formGroup.get('profileTestName').setValue('');
    this.selectedTest = '';
    this.tests = null;
  }

  private _createFormGroup(track: Track): FormGroup {
    return new FormGroup({
      'title': new FormControl(
        track ? track.title : '', [ Validators.required ]
      ),
      'published': new FormControl(
        track ? track.published : '', [ Validators.required ]
      ),
      'requireUserCareer': new FormControl(
        track ? track.requireUserCareer : '', [ Validators.required ]
      ),
      'allowedPercentageWithoutCareerInfo': new FormControl({
        value: track ? track.allowedPercentageWithoutCareerInfo ?
        track.allowedPercentageWithoutCareerInfo.toString() : '0' : '0',
        disabled: track ? !track.requireUserCareer : true
      }),
      'storeUrl': new FormControl(
        track ? track.storeUrl : ''
      ),
      'ecommerceUrl': new FormControl(
        track ? track.ecommerceUrl : ''
      ),
      'createInEcommerce': new FormControl(
        track ? track.createInEcommerce : false
      ),
      'description': new FormControl(
        track ? track.description : '', [ Validators.required ]
      ),
      'imageUrl': new FormControl(
        track && track.imageUrl ? track.imageUrl : './assets/img/420x210-placeholder.png', [ Validators.required ]
      ),
      'tags': new FormArray(
        track && track.tags ? track.tags.map(tag => new FormControl(tag)) : []
      ),
      'profileTestId': new FormControl(
        track ? track.profileTestId : ''
      ),
      'profileTestName': new FormControl(
        track ? track.profileTestName : ''
      ),
      'validFor': new FormControl(
        track ? track.validFor : ''
      )
    });
  }

  private _uploadImage(image: string, formControl: AbstractControl) {
    const resource: UploadResource = {
      data: image,
      filename: 'track.png',
      resource: 'track'
    };

    this._sharedService.uploadImage(resource).subscribe((response) => {
      formControl.setValue(response.data.imageUrl);
    }, () => {
      this.notify('Ocorreu um erro ao enviar a imagem, por favor tente novamente mais tarde');
    });
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
      resource: 'track_certificate'
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
        fields['nome_trilha'] = [ 'Trilha Teste' ];
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

  public requiredUserCareerDisabled(value: boolean) {
    if (value) {
      this.formGroup.get('allowedPercentageWithoutCareerInfo').enable();
    } else {
      this.formGroup.get('allowedPercentageWithoutCareerInfo').setValue('0');
      this.formGroup.get('allowedPercentageWithoutCareerInfo').disable();
    }
  }

  private _loadTests(searchValue: string): void {
    this._testsService.getProfileTests().subscribe((response) => {
      this.tests = response.data;

    }, (error) => this.notify( this.getErrorNotification(error) ));
  }

}
