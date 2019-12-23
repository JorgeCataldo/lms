import { Component, OnInit, Input, EventEmitter, Output } from '@angular/core';
import { MatSnackBar, MatDialog } from '@angular/material';
import { SettingsUsersService } from '../../_services/users.service';
import { ActivatedRoute, Router } from '@angular/router';
import { FormGroup, FormControl, Validators } from '@angular/forms';
import { SharedService } from '../../../shared/services/shared.service';
import { ImageUploadClass } from '../../../shared/classes/image-upload';
import { User, Address } from '../user-models/user';
import { UserRelationalItem, UserLocationRelationalItem } from '../user-models/user-relational-item';
import { AuthService } from 'src/app/shared/services/auth.service';
import { CategoryEnum } from 'src/app/models/enums/category.enum';
import { environment } from 'src/environments/environment';
import { ExternalService } from 'src/app/shared/services/external.service';

@Component({
  selector: 'app-manage-user',
  templateUrl: './manage-user.component.html',
  styleUrls: ['./manage-user.component.scss']
})
export class SettingsManageUserComponent extends ImageUploadClass implements OnInit {
  @Input() summaryForm: boolean = false;
  @Output() saveForm = new EventEmitter();

  public formGroup: FormGroup;
  public jobs: UserRelationalItem[] = [];
  public ranks: UserRelationalItem[] = [];
  public segments: UserRelationalItem[] = [];
  public sectors: UserRelationalItem[] = [];
  public businessGroups: UserRelationalItem[] = [];
  public businessUnits: UserRelationalItem[] = [];
  public frontBackOffices: UserRelationalItem[] = [];
  public users: UserRelationalItem[] = [];
  public countries: UserRelationalItem[] = [];
  public locations: UserLocationRelationalItem[] = [];
  public isEditNewUser: boolean = false;
  public loadedUser: boolean;
  public loadedCategories: boolean;
  public userRole: string;
  public disableEdit: boolean = true;
  public responsibles: UserRelationalItem[] = [];
  public responsible: string = '';
  public lockChangeUserInfo: boolean = false;
  public hasRecruitment: boolean = environment.features.recruitment;
  public isBtg: boolean = false;

  constructor(
    protected _snackBar: MatSnackBar,
    protected _matDialog: MatDialog,
    protected _sharedService: SharedService,
    private _usersService: SettingsUsersService,
    private _activatedRoute: ActivatedRoute,
    private _router: Router,
    private _externalService: ExternalService,
    private _authService: AuthService
  ) {
    super(_snackBar, _matDialog, _sharedService);
  }

  ngOnInit() {
    const userId = this._activatedRoute.snapshot.paramMap.get('userId');
    this.isEditNewUser = userId && userId !== '' && userId !== '0';
    this.formGroup = this._createUserForm();
    this._loadCategories();
    this.isEditNewUser ? this._loadUser(userId) : this.formGroup = this._createUserForm();
  }

  public searchResponsible(searchValue: string) {
    this._usersService.getPagedFilteredUsersList(
      1, 2, searchValue
    );
  }

  public save(): void {
    const userInfo = this.formGroup.getRawValue();
    this._usersService.createUpdateUser(
      this._setUserInfo( userInfo )
    ).subscribe(() => {

      const loggedUser = this._authService.getLoggedUser();
      if (userInfo.id === loggedUser.user_id && userInfo.role !== loggedUser.role)  {
        this._authService.logout();
      } else {
        this.notify('Salvo com sucesso!');
        if (this.summaryForm) {
          this.saveForm.emit();
        }
      }

    }, (err) => {
      this.notify( this.getErrorNotification(err) );
    });
  }

  public canSetRole(): boolean {
    const loggedUser = this._authService.getLoggedUser();
    return loggedUser && (
      loggedUser.role === 'Admin' || loggedUser.role === 'HumanResources'
    );
  }

  public canUpdatePassword(): boolean {
    if (!this.isEditNewUser)
      return false;

    const loggedUser = this._authService.getLoggedUser();
    return (loggedUser.role === 'Admin' || loggedUser.role === 'HumanResources') ||
      loggedUser.user_id === this._activatedRoute.snapshot.paramMap.get('userId');
  }

  public changePassword() {
    this._router.navigate([
      '/configuracoes/usuarios/senha/' + this._activatedRoute.snapshot.paramMap.get('userId')
    ]);
  }

  private _setUserInfo(user: User): User {
    const userId = this._activatedRoute.snapshot.paramMap.get('userId');
    user.id = userId && userId !== '' && userId !== '0' ? userId : '';

    user.role = this.userRole;

    user.lineManager = user.responsibleId && user.responsibleId !== '000000000000000000000000' ?
      (this.users.find(x => x.id === user.responsibleId) ? this.users.find(x => x.id === user.responsibleId).name :
      '') : null;

    user.businessGroup = this._setCategoryInfo('businessGroup', this.businessGroups);
    user.businessUnit = this._setCategoryInfo('businessUnit', this.businessUnits);
    user.frontBackOffice = this._setCategoryInfo('office', this.frontBackOffices);
    user.job = this._setCategoryInfo('jobTitle', this.jobs);
    user.rank = this._setCategoryInfo('rank', this.ranks);
    user.country = this._setCategoryInfo('country', this.countries);
    user.location = this._setCategoryInfo('location', this.locations);
    user.address = new Address();
    user.address.zipCode = this.formGroup.get('zipCode').value;
    user.address.city = this.formGroup.get('city').value;
    user.address.state = this.formGroup.get('state').value;
    user.address.street = this.formGroup.get('street').value;
    user.address.district = this.formGroup.get('district').value;
    user.emitDate = user.emitDate ? user.emitDate : null;
    user.expirationDate = user.expirationDate ? user.expirationDate : null;
    user.sectors = [];

    if (this.formGroup.get('sectorOne').value)
      user.sectors.push( this._getSector('sectorOne') );

    if (this.formGroup.get('sectorTwo').value)
      user.sectors.push( this._getSector('sectorTwo') );

    if (this.formGroup.get('sectorThree').value)
      user.sectors.push( this._getSector('sectorThree') );

    if (this.formGroup.get('sectorFour').value)
      user.sectors.push( this._getSector('sectorFour') );

    user.segment = this.segments.find(
      x => x.id === this.formGroup.get('segment').value
    );
    return user;
  }

  public triggerResponsibleSearch(searchValue: string) {
    if (searchValue && searchValue.trim() !== '') {
      this.responsible = searchValue;
      this._usersService.getUserCategory(CategoryEnum.Users, searchValue).subscribe((response) => {
        this.responsibles = response.data.items;
      });
    }
  }

  public resetResponsibleSearch(): void {
    this.responsibles = [];
  }

  public addResponsibleIntoForm(responsible: UserRelationalItem) {
    this.formGroup.get('responsibleId').setValue(responsible.id);
    this.responsible = responsible.name;
    this.responsibles = [];
  }

  private _setCategoryInfo(control: string, array: Array<any>) {
    const value = this.formGroup.get(control).value;
    return value ? array.find(x => x.id === value) : null;
  }

  private _getSector(control: string) {
    return this.sectors.find(x => x.id === this.formGroup.get(control).value);
  }

  private setFieldDisabled(field: string, user: User = null): boolean {
    const loggedUser = this._authService.getLoggedUser();
    if (user) {
      if (loggedUser && loggedUser.role === 'Student') {
        const index = user.blockedFields.findIndex(x => x === field);
        return index !== -1;
      } else {
        return false;
      }
    } else {
      return !loggedUser || loggedUser.role === 'Student';
    }
  }

  private _createUserForm(user: User = null): FormGroup {
    this.userRole = user && user.role ? user.role : 'Student';

    const loggedUser = this._authService.getLoggedUser();
    this.disableEdit = !loggedUser || loggedUser.role === 'Student';

    const formGroup = new FormGroup({
      'imageUrl': new FormControl({
        value : user && user.imageUrl ? user.imageUrl : './assets/img/user-image-placeholder.png',
        disabled: this.setFieldDisabled('imageUrl', user)
      }),
      'name': new FormControl({
        value: user && user.name ? user.name : '',
        disabled: this.setFieldDisabled('name', user)
      }, [Validators.required]),
      'registrationId': new FormControl({
        value: user && user.registrationId ? user.registrationId : '',
        disabled: environment.features.autoGenerateRegistrationId || this.disableEdit || this.setFieldDisabled('registrationId', user)
      }),
      'dateBorn': new FormControl(user && user.dateBorn ? user.dateBorn : '', [Validators.required]),
      'cpf': new FormControl({
        value: user && user.cpf ? user.cpf : '',
        disabled: this.disableEdit || this.setFieldDisabled('cpf', user)
      }, [Validators.required]),
      'userName': new FormControl({
        value: user && user.userName ? user.userName : '',
        disabled: this.setFieldDisabled('userName', user)
      }, [ Validators.required ]),
      'password': new FormControl({
        value: user && user.password ? user.password : '',
        disabled: this.disableEdit || this.setFieldDisabled('password', user)
      }, [ Validators.required ]),
      'responsibleId': new FormControl({
        value: user && user.responsibleId ? user.responsibleId : '',
        disabled: this.disableEdit || this.setFieldDisabled('responsibleId', user)
      }),
      'linkedIn': new FormControl({
        value: user && user.linkedIn ? user.linkedIn : '',
        disabled: this.setFieldDisabled('linkedIn', user)
      }),
      'info': new FormControl(user && user.info ? user.info : ''),
      'specialNeeds': new FormControl({
        value: user ? user.specialNeeds : '',
        disabled: this.setFieldDisabled('specialNeeds', user)
      }),
      'specialNeedsDescription': new FormControl({
        value: user && user.specialNeedsDescription ? user.specialNeedsDescription : '',
        disabled: (user ? !user.specialNeeds : true) || this.setFieldDisabled('specialNeedsDescription', user)
      }),
      'zipCode': new FormControl({
        value: user && user.address && user.address.zipCode ? user.address.zipCode : '',
        disabled: this.setFieldDisabled('zipCode', user)
      }, [Validators.required]),
      'city': new FormControl({
        value: user && user.address && user.address.city ? user.address.city : '',
        disabled: this.setFieldDisabled('city', user)
      }, [Validators.required]),
      'state': new FormControl({
        value: user && user.address && user.address.state ? user.address.state : '',
        disabled: this.setFieldDisabled('state', user)
      }, [Validators.required]),
      'street': new FormControl({
        value: user && user.address && user.address.street ? user.address.street : '',
        disabled: this.setFieldDisabled('street', user)
      }, [Validators.required]),
      'district': new FormControl({
        value: user && user.address && user.address.district ? user.address.district : '',
        disabled: this.setFieldDisabled('district', user)
      }, [Validators.required]),
      'email': new FormControl({
        value: user && user.email ? user.email : '',
        disabled: this.disableEdit || this.setFieldDisabled('email', user)
      }, [ Validators.required ]),
      'phone': new FormControl({
        value: user && user.phone ? user.phone : '',
        disabled: this.setFieldDisabled('phone', user),
      }, [Validators.required] ),
      'phone2': new FormControl({
        value: user && user.phone2 ? user.phone2 : '',
        disabled: this.setFieldDisabled('phone2', user)
      }),
      'document': new FormControl({
        value: user && user.document ? user.document : '',
        disabled: this.setFieldDisabled('document', user)
      }),
      'documentNumber': new FormControl({
        value: user && user.documentNumber ? user.documentNumber : '',
        disabled: this.setFieldDisabled('documentNumber', user)
      }),
      'documentEmitter': new FormControl({
        value: user && user.documentEmitter ? user.documentEmitter : '',
        disabled: this.setFieldDisabled('documentEmitter', user)
      }),
      'emitDate': new FormControl({
        value: user && user.emitDate ? user.emitDate : '',
        disabled: this.setFieldDisabled('emitDate', user)
      }),
      'expirationDate': new FormControl({
        value: user && user.expirationDate ? user.expirationDate : '',
        disabled: this.setFieldDisabled('expirationDate', user)
      }),
      'businessGroup': new FormControl({
        value: user && user.businessGroup ? user.businessGroup.id : '',
        disabled: this.disableEdit || this.setFieldDisabled('businessGroup', user)
      }),
      'businessUnit': new FormControl({
        value: user && user.businessUnit ? user.businessUnit.id : '',
        disabled: this.disableEdit || this.setFieldDisabled('businessUnit', user)
      }),
      'country': new FormControl({
        value: user && user.country ? user.country.id : '',
        disabled: this.disableEdit || this.setFieldDisabled('country', user)
      }),
      'office': new FormControl({
        value: user && user.frontBackOffice ? user.frontBackOffice.id : '',
        disabled: this.disableEdit || this.setFieldDisabled('office', user)
      }),
      'jobTitle': new FormControl({
        value: user && user.job ? user.job.id : '',
        disabled: this.disableEdit || this.setFieldDisabled('jobTitle', user)
      }),
      'location': new FormControl({
        value: user && user.location ? user.location.id : '',
        disabled: this.disableEdit || this.setFieldDisabled('location', user)
      }),
      'rank': new FormControl({
        value: user && user.rank ? user.rank.id : '',
        disabled: this.disableEdit || this.setFieldDisabled('rank', user)
      }),
      'sectorOne': new FormControl({
        value: user && user.sectors && user.sectors[0] ? user.sectors[0].id : '',
        disabled: this.disableEdit || this.setFieldDisabled('sectorOne', user)
      }),
      'sectorTwo': new FormControl({
        value: user && user.sectors && user.sectors[1] ? user.sectors[1].id : '',
        disabled: this.disableEdit || this.setFieldDisabled('sectorTwo', user)
      }),
      'sectorThree': new FormControl({
        value: user && user.sectors && user.sectors[2] ? user.sectors[2].id : '',
        disabled: this.disableEdit || this.setFieldDisabled('sectorThree', user)
      }),
      'sectorFour': new FormControl({
        value: user && user.sectors && user.sectors[3] ? user.sectors[3].id : '',
        disabled: this.disableEdit || this.setFieldDisabled('sectorFour', user)
      }),
      'segment': new FormControl({
        value: user && user.segment ? user.segment.id : '',
        disabled: this.disableEdit || this.setFieldDisabled('segment', user)
      }),
      'forumActivities': new FormControl(user && user.forumActivities ? user.forumActivities : ''),
      'forumEmail': new FormControl({
        value: user && user.forumEmail ? user.forumEmail : '',
        disabled: user ? !user.forumActivities : true
      })
    });
    this.responsible = user ? user.responsibleName : '';
    this.loadedUser = true;
    return formGroup;
  }

  public changeSpecialNeeds(value: boolean) {
    if (value) {
      this.formGroup.get('specialNeedsDescription').enable();
    } else {
      this.formGroup.get('specialNeedsDescription').setValue('');
      this.formGroup.get('specialNeedsDescription').disable();
    }
  }

  public changeForumActivity(value: boolean) {
    if (value) {
      this.formGroup.get('forumEmail').enable();
      this.formGroup.get('forumEmail').setValue(this.formGroup.get('email').value);
    } else {
      this.formGroup.get('forumEmail').setValue('');
      this.formGroup.get('forumEmail').disable();
    }
  }

  public async searchCep() {
    const cep = this.formGroup.get('zipCode').value;
    this._externalService.getAddressByCep(cep).subscribe(res => {
      this.formGroup.get('city').setValue(res.data.localidade);
      this.formGroup.get('state').setValue(res.data.uf);
      this.formGroup.get('street').setValue(res.data.logradouro);
      this.formGroup.get('district').setValue(res.data.bairro);
    }, err => { this.notify(this.getErrorNotification(err)); });
  }

  private _loadUser(userId: string) {
    this._usersService.getUserById(
      userId
    ).subscribe((response) => {
      this.formGroup = this._createUserForm(response.data);
    });
  }

  private _loadCategories(): void {
    this._usersService.getUserCategory(CategoryEnum.BusinessGroups).subscribe((response) => {
      this.jobs = response.data.items;
    });

    this._usersService.getUserCategory(CategoryEnum.BusinessUnits).subscribe((response) => {
      this.ranks = response.data.items;
    });

    this._usersService.getUserCategory(CategoryEnum.FrontBackOffices).subscribe((response) => {
      this.segments = response.data.items;
    });

    this._usersService.getUserCategory(CategoryEnum.Jobs).subscribe((response) => {
      this.sectors = response.data.items;
    });

    this._usersService.getUserCategory(CategoryEnum.Ranks).subscribe((response) => {
      this.businessGroups = response.data.items;
    });

    this._usersService.getUserCategory(CategoryEnum.Sectors).subscribe((response) => {
      this.businessUnits = response.data.items;
    });

    this._usersService.getUserCategory(CategoryEnum.Segments).subscribe((response) => {
      this.frontBackOffices = response.data.items;
    });

    this._usersService.getUserCategory(CategoryEnum.Countries).subscribe((response) => {
      this.countries = response.data.items;
    });

    this._usersService.getUserCategory(CategoryEnum.Locations).subscribe((response) => {
      this.locations = response.data.items;
    });

    this.loadedCategories = true;
  }

}
