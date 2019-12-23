import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { FormGroup, FormControl, Validators, FormArray, AbstractControl } from '@angular/forms';
import { Module } from '../../../../../models/module.model';
import { SupportMaterial, SupportMaterialType } from '../../../../../models/support-material.interface';
import { MatSnackBar } from '@angular/material';
import { SharedService } from '../../../../../shared/services/shared.service';
import { FileUploadClass } from '../../../../../shared/classes/file-upload';
import { UploadService } from '../../../../../shared/services/upload.service';

@Component({
  selector: 'app-new-module-support-materials',
  templateUrl: './support-materials.component.html',
  styleUrls: ['../new-module-steps.scss', './support-materials.component.scss']
})
export class NewModuleSupportMaterialsComponent extends FileUploadClass implements OnInit {

  @Input() readonly module: Module;
  @Output() addSupportMaterials = new EventEmitter<Array<SupportMaterial>>();

  public formGroup: FormGroup;

  constructor(
    protected _snackBar: MatSnackBar,
    protected _sharedService: SharedService,
    protected _uploadService: UploadService
  ) {
    super(_snackBar, _sharedService, _uploadService);
  }

  ngOnInit() {
    this.formGroup = this._createFormGroup( this.module );
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

  public nextStep(): void {
    if (this.formGroup.valid) {
      const formArray = this.formGroup.getRawValue();
      this.addSupportMaterials.emit( formArray.materials );
    } else
      this.notify('Por favor, preencha todos os campos obrigatórios');
  }

  private _createFormGroup(module: Module): FormGroup {
    return new FormGroup({
      'materials': this._setMaterialsFormArray( module )
    });
  }

  private _setMaterialsFormArray(module: Module): FormArray {
    if (module && module.supportMaterials) {
      return new FormArray(
        module.supportMaterials.map((mat) => this._createMaterialForm(mat))
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

}
