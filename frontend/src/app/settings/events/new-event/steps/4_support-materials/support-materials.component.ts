import { Component, OnInit, Output, EventEmitter, Input } from '@angular/core';
import { FormGroup, FormControl, Validators, FormArray, AbstractControl } from '@angular/forms';
import { SupportMaterial, SupportMaterialType } from '../../../../../models/support-material.interface';
import { Event } from '../../../../../models/event.model';
import { NotificationClass } from '../../../../../shared/classes/notification';
import { MatSnackBar } from '@angular/material';
import { UploadResource } from '../../../../../models/shared/upload-resource.interface';
import { SharedService } from '../../../../../shared/services/shared.service';

@Component({
  selector: 'app-new-event-support-materials',
  templateUrl: './support-materials.component.html',
  styleUrls: ['../new-event-steps.scss', './support-materials.component.scss']
})
export class NewEventSupportMaterialsComponent extends NotificationClass implements OnInit {

  @Input() readonly event: Event;
  @Output() addEventSupportMaterials = new EventEmitter<Array<SupportMaterial>>();

  public formGroup: FormGroup;

  constructor(
    protected _snackBar: MatSnackBar,
    private _sharedService: SharedService
  ) {
    super(_snackBar);
  }

  ngOnInit() {
    this.formGroup = this._createFormGroup( this.event );
  }

  public addMaterial(): void {
    const materials = this.formGroup.get('materials') as FormArray;
    materials.push(
      this._createMaterialForm()
    );
  }

  public removeMaterial(index: number): void {
    const materials = this.formGroup.get('materials') as FormArray;
    materials.removeAt(index);
  }

  public openFileUpload(index: number): void {
    (document.getElementById('inputFile' + index) as HTMLElement).click();
  }

  public setDocumentFile(event, childFormGroup: FormGroup) {
    if (event.target && event.target.files && event.target.files.length > 0) {
      this._uploadPdf(
        event.target.files[0],
        childFormGroup.get('downloadLink'),
        childFormGroup.get('fileName')
      );
    }
  }

  public nextStep(): void {
    if (this.formGroup.valid) {
      const formArray = this.formGroup.getRawValue();
      this.addEventSupportMaterials.emit( formArray.materials );
    } else
      this.notify('Por favor, preencha todos os campos obrigatórios');
  }

  private _createFormGroup(event: Event): FormGroup {
    return new FormGroup({
      'materials': this._setMaterialsFormArray( event )
    });
  }

  private _setMaterialsFormArray(event: Event): FormArray {
    if (event && event.supportMaterials) {
      return new FormArray(
        event.supportMaterials.map((mat) => this._createMaterialForm(mat))
      );
    } else {
      return new FormArray([
        this._createMaterialForm()
      ]);
    }
  }

  private _createMaterialForm(material: SupportMaterial = null): FormGroup {
    return new FormGroup({
      'title': new FormControl(material ? material.title : '', [ Validators.required ]),
      'fileName': new FormControl(material ? material.title : ''),
      'downloadLink': new FormControl(material ? material.downloadLink : '', [ Validators.required ]),
      'description': new FormControl(material ? material.description : '', [ Validators.required ]),
      'type': new FormControl(
        material && material.type ? material.type : SupportMaterialType.File, [ Validators.required ]
      )
    });
  }

  private _uploadPdf(file, valueControl: AbstractControl, nameControl: AbstractControl) {
    const callback = this._sendPdfToServer.bind(this);
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

  private _sendPdfToServer(result: string, fileName: string, valueControl: AbstractControl, nameControl: AbstractControl) {
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
}
