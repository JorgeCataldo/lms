import { Component, OnInit } from '@angular/core';
import { MatSnackBar } from '@angular/material';
import { FormGroup, FormArray, FormControl, Validators } from '@angular/forms';
import { NotificationClass } from 'src/app/shared/classes/notification';
import { ActivatedRoute, Router } from '@angular/router';
import { Record, Activities, PreRequirements, ComplementaryInfo,
  Benefits, ComplementaryBenefits, LanguageInfo, Others } from 'src/app/models/employment.interface';
import { JobPosition } from 'src/app/models/previews/user-job-application.interface';
import { RecruitingCompanyService } from '../../_services/recruiting-company.service';

@Component({
  selector: 'app-manage-user-career',
  templateUrl: './employment.component.html',
  styleUrls: ['./employment.component.scss']
})
export class EmploymentComponent extends NotificationClass implements OnInit {
  public formGroup: FormGroup;
  public jobPositionId: string;

  constructor(
    protected _snackBar: MatSnackBar,
    private _recruitingCompanyService: RecruitingCompanyService,
    private _activatedRoute: ActivatedRoute,
    private _router: Router ) {
    super(_snackBar);
  }

  ngOnInit() {
    this.jobPositionId = this._activatedRoute.snapshot.paramMap.get('jobPositionId');
    if (this.jobPositionId === '0') {
      this.formGroup = this._createFormGroup(null);
    } else {
      this._loadJob(this.jobPositionId);
    }
  }

  public goBack() {
    this._router.navigate(['/configuracoes/vagas-empresa']);
  }

  private _loadJob(id: string): void {
    this._recruitingCompanyService.getJobPosition(id).subscribe(res => {
      this.formGroup = this._createFormGroup(res.data);
    }, err => {
      this.notify(this.getErrorNotification(err));
    });
  }

  private _createFormGroup(jobPosition: JobPosition): FormGroup {
    return new FormGroup({
      'title': new FormControl(jobPosition ? jobPosition.title : '', [Validators.required]),
      'dueTo': new FormControl(jobPosition ? jobPosition.dueTo : '', [Validators.required]),
      'priority': new FormControl(jobPosition ? jobPosition.priority : '', [Validators.required]),
      'record': this._createRecordForm(jobPosition ? jobPosition.employment ?
        jobPosition.employment.record : null : null),
      'activities': this._createActivitiesForm(jobPosition ? jobPosition.employment ?
        jobPosition.employment.activities : null : null),
      'preRequirements': this._createPreRequirementsForm(jobPosition ? jobPosition.employment ?
        jobPosition.employment.preRequirements : null : null),
      'values': this._setValuesFormArray(jobPosition ? jobPosition.employment ?
        jobPosition.employment.values : [] : []),
      'benefits': this._createBenefitsForm(jobPosition ? jobPosition.employment ?
        jobPosition.employment.benefits : null : null)
    });
  }

  private _createRecordForm(value: Record): FormGroup {
    return new FormGroup({
      'function': new FormControl(value ? value.function : ''),
      'contractType': new FormControl(value ? value.contractType : '')
    });
  }

  private _createActivitiesForm(value: Activities): FormGroup {
    return new FormGroup({
      'activity': new FormControl(value ? value.activity : ''),
      'character': new FormControl(value ? value.character : ''),
      'abilities': new FormControl(value ? value.abilities : ''),
      'report': new FormControl(value ? value.report : '')
    });
  }

  private _createPreRequirementsForm(value: PreRequirements): FormGroup {
    return new FormGroup({
      'education': new FormControl(value ? value.education : ''),
      'curseName': new FormControl(value ? value.curseName : ''),
      'dateConclusion': new FormControl(value ? value.dateConclusion : ''),
      'crAcumulation': new FormControl(value ? value.crAcumulation : ''),
      'minTime': new FormControl(value ? value.minTime : ''),
      'complementaryInfo': new FormArray([
        this._createFixedPreRequirementsForm('VBA', value && value.complementaryInfo ? value.complementaryInfo : []),
        this._createFixedPreRequirementsForm('Excel', value && value.complementaryInfo ? value.complementaryInfo : []),
        this._createFixedPreRequirementsForm('Software Estatístico', value && value.complementaryInfo ? value.complementaryInfo : []),
        this._createFixedPreRequirementsForm('Pacote Office', value && value.complementaryInfo ? value.complementaryInfo : [])
      ]),
      'others': this._setOthersFormArray(value && value.others ? value.others : []),
      'certification': new FormControl(value ? value.certification : ''),
      'languageInfo': this._setLanguageFormArray(value && value.languageInfo ? value.languageInfo : [])
    });
  }

  private _createFixedPreRequirementsForm(name: string, values: ComplementaryInfo[]): FormGroup {
    const hasPerk = values.find(v => v.name === name);

    return new FormGroup({
      'name': new FormControl(name),
      'done': new FormControl(hasPerk ? hasPerk.done : ''),
      'level': new FormControl({ value: hasPerk ? hasPerk.level : '', disabled: !(hasPerk ? hasPerk.done : false)})
    });
  }


  private _createValuesForm(value: string = null): FormGroup {
    return new FormGroup({
      'value': new FormControl(value ? value : '')
    });
  }
  private _setValuesFormArray(values: string[]): FormArray {
    return new FormArray(
      values.map((value) => this._createValuesForm(value))
    );
  }

  private _createLaguangeForm(value: LanguageInfo = null): FormGroup {
    return new FormGroup({
      'language': new FormControl(value ? value.language : ''),
      'level': new FormControl(value ? value.level : '')
    });
  }

  private _setLanguageFormArray(values: LanguageInfo[]): FormArray {
    return new FormArray(
      values.map((value) => this._createLaguangeForm(value))
    );
  }

  private _createOthersForm(value: Others = null): FormGroup {
    return new FormGroup({
      'name': new FormControl(value ? value.name : ''),
      'level': new FormControl(value ? value.level : '')
    });
  }

  private _setOthersFormArray(values: Others[]): FormArray {
    return new FormArray(
      values.map((value) => this._createOthersForm(value))
    );
  }

  private _createBenefitsForm(value: Benefits): FormGroup {
    return new FormGroup({
      'salary': new FormControl(value ? value.salary : ''),
      'complementaryBenefits': new FormArray([
        this._createFixedBenefitsForm('VT', value && value.complementaryBenefits ? value.complementaryBenefits : []),
        this._createFixedBenefitsForm('VA', value && value.complementaryBenefits ? value.complementaryBenefits : []),
        this._createFixedBenefitsForm('VR', value && value.complementaryBenefits ? value.complementaryBenefits : []),
        this._createFixedBenefitsForm('PLR', value && value.complementaryBenefits ? value.complementaryBenefits : [])
      ]),
      'employmentType': new FormControl(value ? value.employmentType : ''),
      'employmentChangeHouse': new FormControl(value ? value.employmentChangeHouse : '')
    });
  }

  private _createFixedBenefitsForm(name: string, values: ComplementaryBenefits[]): FormGroup {
    const hasPerk = values.find(v => v.name === name);
    return new FormGroup({
      'name': new FormControl(name),
      'done': new FormControl(hasPerk ? hasPerk.done : ''),
      'level': new FormControl({ value: hasPerk ? hasPerk.level : '', disabled: !(hasPerk ? hasPerk.done : false)})
    });
  }

  public addReward(): void {
    const rewards = this.formGroup.get('values') as FormArray;
    rewards.push(
      this._createValuesForm()
    );
  }

  public addLanguage(): void {
    const languages = this.formGroup.get('preRequirements').get('languageInfo') as FormArray;
    languages.push(
      this._createLaguangeForm()
    );
  }

  public addOthers(): void {
    const languages = this.formGroup.get('preRequirements').get('others') as FormArray;
    languages.push(
      this._createOthersForm()
    );
  }

  public saveJob(): void {
    const job = this.formGroup.getRawValue();
    job.values = job.values.map(x => x.value);
    if (this.jobPositionId === '0') {
      this._recruitingCompanyService.addJobPosition(job.title, job.dueTo,
        job.priority, job).subscribe(() => {
          this.notify('Salvo com sucesso');
          this._router.navigate(['/configuracoes/vagas-empresa']);
      }, err => {
          this.notify(this.getErrorNotification(err));
      });
    } else {
      this._recruitingCompanyService.updateJobPosition(this.jobPositionId, job.title,
        job.dueTo, job.priority, job).subscribe(() => {
          this.notify('Atualizado com sucesso');
          this._router.navigate(['/configuracoes/vagas-empresa']);
      }, err => {
          this.notify(this.getErrorNotification(err));
      });
    }
  }
}
