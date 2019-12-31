import { Component, OnInit } from '@angular/core';
import { MatSnackBar } from '@angular/material';
import { SettingsUsersService } from '../../_services/users.service';
import { ActivatedRoute, Router } from '@angular/router';
import { FormGroup, FormControl, Validators } from '@angular/forms';
import { User } from '../user-models/user';
import { NotificationClass } from 'src/app/shared/classes/notification';
import { LoginResponse, AuthService } from 'src/app/shared/services/auth.service';

@Component({
  selector: 'app-manage-user-password',
  templateUrl: './manage-user-password.component.html',
  styleUrls: ['./manage-user-password.component.scss']
})
export class SettingsManageUserPasswordComponent extends NotificationClass implements OnInit {

  public formGroup: FormGroup;
  public adminChange: boolean = false;
  public user: LoginResponse;
  private _userId: string;

  constructor(
    protected _snackBar: MatSnackBar,
    private _authService: AuthService,
    private _usersService: SettingsUsersService,
    private _activatedRoute: ActivatedRoute,
    private _router: Router
  ) {
    super(_snackBar);
  }

  ngOnInit() {
    this.user = this._authService.getLoggedUser();
    this._userId = this._activatedRoute.snapshot.paramMap.get('userId');
    if (this.user.role === 'Admin' || this.user.role === 'HumanResources') {
      if (this._userId === this.user.user_id) {
        this.formGroup = this._createUserForm();
      } else {
        this.adminChange = true;
        this.formGroup = this._createAdminForm();
      }
    } else {
      this.formGroup = this._createUserForm();
    }
  }

  public backToUserManager() {
    this._router.navigate(['/configuracoes/usuarios/' + this._userId]);
  }

  public save(): void {
    if (this.formGroup.get('newPassword').value !== this.formGroup.get('newPasswordConfirmation').value) {
      this.notify('A nova senha e a confirmação estão diferentes');
    } else {
      if (this.adminChange) {
        this._usersService.adminChangePassword(
          this._userId,
          this.formGroup.get('newPassword').value
        ).subscribe(() => {
          this._router.navigate(['/configuracoes/usuarios/' + this._userId]);
          this.notify('Senha alterada com sucesso');
        }, (err) => {
          this.notify(this.getErrorNotification(err));
        });
      } else {
        this._usersService.changePassword(
          this.formGroup.get('currentPassword').value,
          this.formGroup.get('newPassword').value
        ).subscribe(() => {
          this._router.navigate(['/configuracoes/usuarios/' + this._userId]);
          this.notify('Senha alterada com sucesso');
        }, (err) => {
          this.notify(this.getErrorNotification(err));
        });
      }
    }
  }

  private _createUserForm(user: User = null): FormGroup {
    const formGroup = new FormGroup({
      'currentPassword': new FormControl(user && user.name ? user.name : '',
        [Validators.required]
      ),
      'newPassword': new FormControl(user && user.name ? user.name : '',
        [Validators.required]
      ),
      'newPasswordConfirmation': new FormControl(user && user.name ? user.name : '',
        [Validators.required]
      )
    });
    return formGroup;
  }

  private _createAdminForm(user: User = null): FormGroup {
    const formGroup = new FormGroup({
      'newPassword': new FormControl(user && user.name ? user.name : '',
        [Validators.required]
      ),
      'newPasswordConfirmation': new FormControl(user && user.name ? user.name : '',
        [Validators.required]
      )
    });
    return formGroup;
  }
}
