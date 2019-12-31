import { Component, OnInit, Input } from '@angular/core';
import { MatSnackBar, MatDialog } from '@angular/material';
import { ExternalService } from 'src/app/shared/services/external.service';
import { FormGroup, FormControl, Validators } from '@angular/forms';
import { SharedService } from 'src/app/shared/services/shared.service';
import { ImageUploadClass } from 'src/app/shared/classes/image-upload';
import { RecruitingCompanyService } from '../../_services/recruiting-company.service';
import { RecruiterCompany } from 'src/app/models/recruiter-company.model';
import { ValidationsService } from 'src/app/shared/services/validation.service';

@Component({
  selector: 'app-manage-company-form',
  templateUrl: './company-form.component.html',
  styleUrls: ['./company-form.component.scss']
})
export class RecruitingCompanyFormComponent extends ImageUploadClass implements OnInit {

  public formGroup: FormGroup;

  constructor(
    protected _snackBar: MatSnackBar,
    protected _matDialog: MatDialog,
    protected _sharedService: SharedService,
    private _externalService: ExternalService,
    private _recruitingService: RecruitingCompanyService,
    private _validationService: ValidationsService
  ) {
    super(_snackBar, _matDialog, _sharedService);
  }

  ngOnInit() {
    this._loadRecruitingCompany();
  }

  public save(): void {
    if (!this.formGroup.valid) {
      this.notify('Por favor, preencha todos os campos obrigatórios');
      return;
    }

    const companyInfo: RecruiterCompany = this.formGroup.getRawValue();

    if (companyInfo.cnpj) {
      const cnpjValid = this._validationService.validateCNPJ(companyInfo.cnpj);
      if (!cnpjValid) {
        this.notify('CNPJ Inválido');
        return;
      }
    }

    this._recruitingService.manageRecruiterCompany(
      companyInfo
    ).subscribe(() => {
      this.notify('Salvo com sucesso!');

    }, (err) => {
      this.notify( this.getErrorNotification(err) );
    });
  }

  private _loadRecruitingCompany(): void {
    this._recruitingService.getRecruitingCompany().subscribe((response) => {
      this.formGroup = this._createCompanyForm( response.data );
    }, () => this.formGroup = this._createCompanyForm() );
  }

  private _createCompanyForm(comp: RecruiterCompany = null): FormGroup {
    const formGroup = new FormGroup({
      'socialName': new FormControl(comp ? comp.socialName : '', [ Validators.required ]),
      'businessName': new FormControl(comp ? comp.businessName : '', [ Validators.required ]),
      'cnpj': new FormControl(comp ? comp.cnpj : '', [ Validators.required ]),
      'address': new FormGroup({
        'zipCode': new FormControl(comp && comp.address ? comp.address.zipCode : ''),
        'city': new FormControl(comp && comp.address && comp.address.city ? comp.address.city : ''),
        'state': new FormControl(comp && comp.address && comp.address.state ? comp.address.state : ''),
        'street': new FormControl(comp && comp.address && comp.address.street ? comp.address.street : ''),
        'district': new FormControl(comp && comp.address && comp.address.district ? comp.address.district : ''),
      }),
      'humanResourcesResponsible': new FormGroup({
        'name': new FormControl(
          comp && comp.humanResourcesResponsible ? comp.humanResourcesResponsible.name : '', [ Validators.required ]
        ),
        'email': new FormControl(
          comp && comp.humanResourcesResponsible ? comp.humanResourcesResponsible.email : '', [ Validators.required ]
        ),
        'phone': new FormControl(
          comp && comp.humanResourcesResponsible ? comp.humanResourcesResponsible.phone : '', [ Validators.required ]
        ),
      }),
      'operationsResponsible': new FormGroup({
        'name': new FormControl(comp && comp.operationsResponsible ? comp.operationsResponsible.name : ''),
        'email': new FormControl(comp && comp.operationsResponsible ? comp.operationsResponsible.email : ''),
        'phone': new FormControl(comp && comp.operationsResponsible ? comp.operationsResponsible.phone : ''),
      }),
      'companySize': new FormControl(comp ? comp.companySize : ''),
      'businessActivity': new FormControl(comp ? comp.businessActivity : ''),
      'yearlyHiring': new FormControl(comp ? comp.yearlyHiring : ''),
      'profileMeasuringTool': new FormControl(comp ? comp.profileMeasuringTool : ''),
      'authLogo': new FormControl(comp ? comp.authLogo : false, [ Validators.required ]),
      'logoUrl': new FormControl(comp ? comp.logoUrl : './assets/img/user-image-placeholder.png')
    });

    return formGroup;
  }

  public searchCep(): void {
    const cep = this.formGroup.get('address.zipCode').value;
    this._externalService.getAddressByCep(cep).subscribe(res => {
      this.formGroup.get('address.city').setValue(res.data.localidade);
      this.formGroup.get('address.state').setValue(res.data.uf);
      this.formGroup.get('address.street').setValue(res.data.logradouro);
      this.formGroup.get('address.district').setValue(res.data.bairro);

    }, err => { this.notify('Erro ao buscar CEP'); });
  }
}
