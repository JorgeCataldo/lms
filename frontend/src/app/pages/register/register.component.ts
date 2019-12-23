import { Component, OnInit } from '@angular/core';
import { FormGroup, FormControl, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { MatSnackBar, MatDialog } from '@angular/material';
import { ImageUploadClass } from 'src/app/shared/classes/image-upload';
import { SharedService } from 'src/app/shared/services/shared.service';
import { AuthService } from 'src/app/shared/services/auth.service';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.scss']
})
export class RegisterComponent extends ImageUploadClass implements OnInit {

  public loading: boolean;
  public formGroup: FormGroup;
  public firstAccessFormGroup: FormGroup;
  public hasSSO: boolean;
  public hasSignUp: boolean;
  public isFirstAccess: boolean = false;

  constructor(
    protected _snackBar: MatSnackBar,
    protected _matDialog: MatDialog,
    protected _sharedService: SharedService,
    private _authService: AuthService,
    private router: Router) {
      super(_snackBar, _matDialog, _sharedService);
  }

  ngOnInit() {
    this.formGroup = this._createUserForm();
  }

  private _createUserForm(): FormGroup {
    return new FormGroup({
      'imageUrl': new FormControl('./assets/img/user-image-placeholder.png'),
      'name': new FormControl('', [Validators.required]),
      'cpf': new FormControl('', [Validators.required]),
      'userName': new FormControl('', [Validators.required]),
      'password': new FormControl('', [Validators.required]),
      'repeatPassword': new FormControl('', [Validators.required]),
      'info': new FormControl(''),
      'email': new FormControl('', [Validators.required]),
      'phone': new FormControl('', [Validators.required])
    });
  }

  public async save() {
    const user = this.formGroup.getRawValue();
    if (!this._cpfValidator(user.cpf)) {
      this.notify('Cpf inválido');
    } else if (user.password !== user.repeatPassword) {
      this.notify('A senha e confirmação estão diferentes');
    } else {
      try {
        const result = await this._authService.register(user);
        if (result.success) {
          this.notify('Cadastro efetivado com sucesso');
          this.router.navigate(['confirmacao-email']);
        } else {
          const error = result && result.errors && result.errors.length > 0 ?
              result.errors[0] : 'Houve um erro ao se comunicar com o servidor. Por favor tente mais tarde.';
          this.notify(error);
        }
      } catch (err) {
        this.notify(this.getErrorNotification(err));
      }
    }
  }

  private _cpfValidator(cpf: string) {
    cpf = cpf.replace(/[^\d]+/g, '');
    if (cpf === '') return false;

    if (cpf.length !== 11 ||
      cpf === '00000000000' ||
      cpf === '11111111111' ||
      cpf === '22222222222' ||
      cpf === '33333333333' ||
      cpf === '44444444444' ||
      cpf === '55555555555' ||
      cpf === '66666666666' ||
      cpf === '77777777777' ||
      cpf === '88888888888' ||
      cpf === '99999999999')
        return false;

    let add = 0;
    let rev = 0;
    for (let i = 0; i < 9; i ++)
      add += parseInt(cpf.charAt(i), 10) * (10 - i);
      rev = 11 - (add % 11);
      if (rev === 10 || rev === 11)
        rev = 0;
      if (rev !== parseInt(cpf.charAt(9), 10))
        return false;

    add = 0;
    for (let i = 0; i < 10; i ++)
      add += parseInt(cpf.charAt(i), 10) * (11 - i);
    rev = 11 - (add % 11);
    if (rev === 10 || rev === 11)
      rev = 0;
    if (rev !== parseInt(cpf.charAt(10), 10))
      return false;
    return true;
  }
}
