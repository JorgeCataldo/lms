import { Component, OnInit } from '@angular/core';
import { MatSnackBar, MatTabChangeEvent } from '@angular/material';
import { FormGroup, FormArray, FormControl, Validators } from '@angular/forms';
import { NotificationClass } from 'src/app/shared/classes/notification';
import { ActivatedRoute, Router } from '@angular/router';
import { SettingsUsersService } from '../../_services/users.service';
import { UserCareer, ProfessionalExperience, College, Reward,
  Perk, Certificate, Institute, PerkLanguage } from '../user-models/user-career';
import * as moment from 'moment/moment';

@Component({
  selector: 'app-manage-user-career',
  templateUrl: './manage-user-career.component.html',
  styleUrls: ['./manage-user-career.component.scss']
})
export class SettingsManageUserCareerComponent extends NotificationClass implements OnInit {

  public travelAvailability: boolean = false;
  public movingAvailability: boolean = false;
  public institutes: Array<Array<Institute>> = [];
  public formGroup: FormGroup;
  public previousTab: number = 0;
  public education: boolean  = false;
  public professionalExperience: boolean  = false;
  public complementaryExperience: boolean  = false;
  public complementaryInfo: boolean  = false;
  public professionalObjectives: boolean  = false;

  constructor(
    protected _snackBar: MatSnackBar,
    private _settingsUsersService: SettingsUsersService,
    private _activatedRoute: ActivatedRoute,
    private _router: Router ) {
    super(_snackBar);
  }

  ngOnInit() {
    this._loadUserCareer();
  }

  public goBack() {
    this._router.navigate(['/configuracoes/detalhes-usuario/' + this._activatedRoute.snapshot.paramMap.get('userId')]);
  }

  public selectTravelAvailability(value: boolean) {
    this.travelAvailability = value;
  }

  public selectMovingAvailability(value: boolean) {
    this.movingAvailability = value;
  }

  public addProfessionalExperience(): void {
    const professionalExperiences = this.formGroup.get('professionalExperiences') as FormArray;
    professionalExperiences.push(
      this._createProfessionalExperienceForm()
    );
  }

  public addCollege(): void {
    const colleges = this.formGroup.get('colleges') as FormArray;
    colleges.push(
      this._createCollegeForm()
    );
    this.institutes.push([]);
  }

  public addReward(): void {
    const rewards = this.formGroup.get('rewards') as FormArray;
    rewards.push(
      this._createRewardForm()
    );
  }

  public addCertificate(): void {
    const certificates = this.formGroup.get('certificates') as FormArray;
    certificates.push(
      this._createCertificateForm()
    );
  }

  public addLanguage(): void {
    const languages = this.formGroup.get('fixedLanguages') as FormArray;
    languages.push(
      this._createFixedLanguagesForm(),
    );
  }

  public addAbility(): void {
    const abilities = this.formGroup.get('abilities') as FormArray;
    abilities.push(
      this._createPerkForm()
    );
  }

  public addSkill(): void {
    const skills = this.formGroup.get('skills') as FormArray;
    skills.push(
      this._createSkillForm()
    );
  }

  private _loadUserCareer(): void {
    this._settingsUsersService.getUserCareer(
      this._activatedRoute.snapshot.paramMap.get('userId')
    ).subscribe(res => {
      this.formGroup = this._createFormGroup(res.data);

    }, () => {
      this.formGroup = this._createFormGroup( null );
    });
  }

  private _createFormGroup(career: UserCareer): FormGroup {
    return new FormGroup({
      'professionalExperience': new FormControl(career ? career.professionalExperience : false),
      'professionalExperiences': this._setProfessionalExperiencesFormArray(
        career && career.professionalExperiences ? career.professionalExperiences : []
      ),
      'colleges': this._setCollegesFormArray(career && career.colleges ? career.colleges : []),
      'rewards': this._setRewardsFormArray(career && career.rewards ? career.rewards : []),
      'abilities': this._setPerksFormArray(career && career.abilities ? career.abilities : []),
      'fixedLanguages': this._setFixedLanguagesFormArray(career && career.languages ? career.languages : []),
      'fixedAbilities': new FormArray([
        this._createFixedAbilityForm('VBA *', career && career.abilities ? career.abilities : []),
        this._createFixedAbilityForm('Excel *', career && career.abilities ? career.abilities : []),
        this._createFixedAbilityForm('Software Estatístico', career && career.abilities ? career.abilities : []),
        this._createFixedAbilityForm('Pacote Office', career && career.abilities ? career.abilities : [])
      ]),
      'skills': this._setSkillsFormArray(career && career.skills ? career.skills : []),
      'certificates': this._setCertificatesFormArray(career && career.certificates ? career.certificates : []),
      'shortDateObjectives': new FormControl(career && career.shortDateObjectives ? career.shortDateObjectives : '', [Validators.required]),
      'longDateObjectives': new FormControl(career && career.longDateObjectives ? career.longDateObjectives : '', [Validators.required])
    });
  }

  private _setProfessionalExperiencesFormArray(values: ProfessionalExperience[]): FormArray {
    return new FormArray(
      values.map((value) => this._createProfessionalExperienceForm(value))
    );
  }

  private _createProfessionalExperienceForm(value: ProfessionalExperience = null): FormGroup {
    return new FormGroup({
      'title': new FormControl(value ? value.title : ''),
      'role': new FormControl(value ? value.role : ''),
      'description': new FormControl(value ? value.description : ''),
      'startDate': new FormControl(value ? value.startDate : ''),
      'endDate': new FormControl(value ? value.endDate : '')
    });
  }

  private _setCollegesFormArray(values: College[]): FormArray {
    values.forEach(() => { this.institutes.push([]); });
    return new FormArray(
      values.map((value) => this._createCollegeForm(value))
    );
  }

  private _createCollegeForm(value: College = null): FormGroup {
    return new FormGroup({
      'instituteId': new FormControl(value ? value.instituteId : '', [Validators.required]),
      'title': new FormControl(value ? value.title : '', [Validators.required]) ,
      'campus': new FormControl(value ? value.campus : ''),
      'name': new FormControl(value ? value.name : '', [Validators.required]),
      'academicDegree': new FormControl(value ? value.academicDegree : '', [Validators.required]),
      'status': new FormControl(value ? value.status : '', [Validators.required]),
      'completePeriod': new FormControl({value : value ? value.completePeriod : '',
        disabled: value && value.status !== 'Completo' }),
      'startDate': new FormControl(value ? moment(value.startDate).format('MM/YYYY') : ''),
      'endDate': new FormControl(value ? moment(value.endDate).format('MM/YYYY')  : '', [Validators.required]),
      'cr': new FormControl(value ? value.cr : '')
    });
  }

  private _setRewardsFormArray(values: Reward[]): FormArray {
    return new FormArray(
      values.map((value) => this._createRewardForm(value))
    );
  }

  private _createRewardForm(value: Reward = null): FormGroup {
    return new FormGroup({
      'title': new FormControl(value ? value.title : ''),
      'name': new FormControl(value ? value.name : ''),
      'link': new FormControl(value ? value.link : ''),
      'date': new FormControl(value ? value.date : ''),
    });
  }

  private _setPerksFormArray(values: Perk[]): FormArray {
    const fixed = ['VBA *', 'Excel *', 'Software Estatístico', 'Pacote Office'];
    values = values.filter(v => !fixed.includes(v.name));
    return new FormArray(
      values.map((value) => this._createPerkForm(value))
    );
  }

  private _createPerkForm(value: Perk = null): FormGroup {
    return new FormGroup({
      'name': new FormControl(value ? value.name : ''),
      'level': new FormControl(value ? value.level : '')
    });
  }

  private _setSkillsFormArray(values: string[]): FormArray {
    return new FormArray(
      values.map((value) => this._createSkillForm(value))
    );
  }

  private _createSkillForm(value: string = ''): FormGroup {
    return new FormGroup({
      'name': new FormControl(value)
    });
  }

  private _setCertificatesFormArray(values: Certificate[]): FormArray {
    return new FormArray(
      values.map((value) => this._createCertificateForm(value))
    );
  }

  private _createCertificateForm(value: Certificate = null): FormGroup {
    return new FormGroup({
      'title': new FormControl(value ? value.title : ''),
      'link': new FormControl(value ? value.link : '')
    });
  }

  private _createFixedAbilityForm(name: string, values: Perk[]): FormGroup {
    const hasPerk = values.find(v => v.name === name);

    return new FormGroup({
      'name': new FormControl(name),
      'hasLevel': new FormControl(hasPerk && true),
      'level': new FormControl(hasPerk ? hasPerk.level : '')
    });
  }

  private _setFixedLanguagesFormArray(values: PerkLanguage[]): FormArray {
    return new FormArray(
      values.map((value) => this._createFixedLanguagesForm(value))
    );
  }

  private _createFixedLanguagesForm(value: PerkLanguage = null): FormGroup {
    return new FormGroup({
      'names': new FormControl(value ? value.names : ''),
      'languages': new FormControl(value ? value.languages : ''),
      'level': new FormControl(value ? value.level : '')
    });
  }

  public onLinkClick(event: MatTabChangeEvent) {
    switch (this.previousTab) {
      case 0:
          this.checkAcademicEducation();
          break;
      case 1:
          this.checkProfessionalExperience();
          break;
      case 2:
          this.checkComplementaryExperience();
          break;
      case 3:
          this.checkComplementaryInfo();
          break;
      case 4:
          this.checkProfessionalObjectives();
          break;
      default:
        break;
    }

    this.previousTab = event.index;
  }

  public checkAcademicEducation() {
    const career = this.formGroup.getRawValue();
    this.education = false;
    let checkEducationField = true;
    if (career.colleges) {
      career.colleges.forEach(x => {
        if ((x.title.length === 0) || (x.name.length === 0 ) ||
        (x.status.length === 0) || (x.academicDegree.length === 0) ||
        (x.endDate.length === 0)) {
            this.notify('Preencha em Histórico Acadêmico todos os campos obrigatórios (*)');
            checkEducationField = false;
          }
        });
    } if (career.colleges.length === 0) {
        this.notify('É obrigatório ter pelo menos uma Instituição em Histórico Acadêmico');
        checkEducationField = false;
      } if (checkEducationField) {
        this.education = true;
      }
  }

  public checkProfessionalExperience() {
    const career = this.formGroup.getRawValue();
    this.professionalExperience = false;
    let checkProfExpField = true;
    if (career.professionalExperiences) {
      career.professionalExperiences.forEach(x => {
        if ((x.title.length === 0) || (x.description.length === 0 ) ||
          (x.startDate === null)) {
              this.notify('Preencha em Experiência Profissional todos os campos obrigatórios (*)');
              checkProfExpField = false;
            }

          const start = new Date (x.startDate);
          const end = new Date(x.endDate);

          if (start > end) {
            this.notify('A data de ínicio não pode ser menor que a data de saída');
            checkProfExpField = false;
          }
        });
    } if (career.professionalExperiences.length === 0) {
        this.notify('É obrigatório ter pelo menos uma "Experiência Profissional"');
        checkProfExpField = false;
      } if (checkProfExpField) {
        this.professionalExperience = true;
      }
  }

  public checkComplementaryExperience() {
    const career = this.formGroup.getRawValue();
    this.complementaryExperience = false;
    let checkComplementaryExpField = true;
    if (career.fixedAbilities) {
      if (career.fixedAbilities[0].hasLevel === null ||
        career.fixedAbilities[0].hasLevel === false ||
        career.fixedAbilities[0].level.length === 0) {
        this.notify('O campo "VBA" é obrigatório');
        checkComplementaryExpField = false;
      } else if (career.fixedAbilities[1].hasLevel === null ||
        career.fixedAbilities[1].hasLevel === false ||
        career.fixedAbilities[1].level.length === 0) {
        this.notify('O campo "Excel" é obrigatório');
        checkComplementaryExpField = false;
      } else {
        career.abilities.forEach(x => {
          if (x.level.length === 0 || x.name.length === 0) {
            this.notify('Não podem haver nenhum campo em branco em "Formação Complementar"');
            checkComplementaryExpField = false;
          }
        });
      }
    } if (career.fixedAbilities.length === 0) {
        this.notify('Os campos "VBA" e "Excel" são obrigatórios');
        checkComplementaryExpField = false;
      } if (checkComplementaryExpField) {
        this.complementaryExperience = true;
      }
  }

  public checkComplementaryInfo() {
    const career = this.formGroup.getRawValue();
    this.complementaryInfo = false;
    let checkComplementaryInfoField = true;
    if (career.fixedLanguages) {
      career.fixedLanguages.forEach(x => {
        if (!x.level) {
            this.notify('O campo idioma é obrigatório');
            checkComplementaryInfoField = false;
        } if (x.languages === 'outro' && !x.names) {
          this.notify('O campo idioma é obrigatório');
          checkComplementaryInfoField = false;
        }
      });
    } if (checkComplementaryInfoField) {
      this.complementaryInfo = true;
    }
  }

  public checkProfessionalObjectives() {
    const career = this.formGroup.getRawValue();
    let checkProfessionalObjectivesField = true;
    if (!career.longDateObjectives || !career.shortDateObjectives) {
        this.notify('O campo Curto e Longo Prazo é obrigatório');
        checkProfessionalObjectivesField = false;
    } if (checkProfessionalObjectivesField) {
      this.professionalObjectives = true;
    }
  }

  public saveCareer(): void {
    const career = this.formGroup.getRawValue();
    let checkProfessionalObjectivesField = true;
    if (!career.longDateObjectives || !career.shortDateObjectives) {
      this.notify('O campo Curto e Longo Prazo é obrigatório');
      checkProfessionalObjectivesField = false;
    } else if (checkProfessionalObjectivesField) {
      this.professionalObjectives = true;
      if (this.education &&
        this.professionalExperience &&
        this.complementaryExperience &&
        this.complementaryInfo &&
        this.professionalObjectives) {
        this._settingsUsersService.updateUserCareer(
          this._activatedRoute.snapshot.paramMap.get('userId'),
          this._adjustCareer( career )
        ).subscribe(() => {
          this.notify('Informações salvas com sucesso');
        }, (error) => this.notify(this.getErrorNotification(error)) );
      } else {
        this.notify('Existe algum campo obrigatório em brano');
      }
    }

}

  private _adjustCareer(career): UserCareer {
    career = this._adjustAcademicEducationDates(career);
    career.skills = career.skills.map(x => x.name);
    career.professionalExperiences = career.professionalExperience ? career.professionalExperiences : [];
    career.travelAvailability = this.travelAvailability;
    career.movingAvailability = this.movingAvailability;
    return career;
  }

  private _adjustAcademicEducationDates(career: UserCareer) {
    if (career.colleges && career.colleges.length > 0) {
      career.colleges.forEach(college => {
        college.startDate = moment(college.startDate, 'MM/YYYY').toDate();
        college.endDate = moment(college.endDate, 'MM/YYYY').toDate();
      });
    }
    return career;
  }

  public checkDate(fromDate: Date, toDate: Date): boolean {
    if (fromDate && toDate) {
      fromDate = new Date(fromDate);
      toDate = new Date(toDate);
      if (fromDate > toDate) {
        return false;
      } else {
        return true;
      }
    } else if (fromDate) {
      return true;
    } else {
      return false;
    }
  }
}
