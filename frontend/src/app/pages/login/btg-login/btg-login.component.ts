import { Component, OnInit } from '@angular/core';
import { FormGroup, FormControl, Validators } from '@angular/forms';
import { MatSnackBar, MatDialogRef } from '@angular/material';
import { AuthService } from 'src/app/shared/services/auth.service';
import { NotificationClass } from 'src/app/shared/classes/notification';
import { CookieService } from 'ngx-cookie-service';


@Component({
  selector: 'app-btg-login',
  templateUrl: './btg-login.component.html',
  styleUrls: ['./btg-login.component.scss']
})
export class BtgLoginComponent extends NotificationClass implements OnInit {

  public formGroup: FormGroup;
  public isLogged: boolean = false;
  public username: string = '';
  private cookieName: string = 'BTG';

  constructor(
    protected _snackBar: MatSnackBar,
    private cookieService: CookieService,
    private _dialogRef: MatDialogRef<BtgLoginComponent>) {
      super(_snackBar);
      this.formGroup = new FormGroup({
        'username': new FormControl('', [Validators.required]),
        'password': new FormControl('', [Validators.required])
      });
  }

  ngOnInit() {
    this.isLogged = this.cookieService.check(this.cookieName);
    if (this.isLogged) {
      const cookieValue = this.cookieService.get(this.cookieName);
      this.username = cookieValue;
    }
  }

  public closeModal() {
    this._dialogRef.close();
  }

  public doLogin() {
    const credentials = this.formGroup.getRawValue();
    // LOGIN VIA BTG - Retorna o TOKEN e USERNAME (ou Email)
    // Primeiro acesso tem que cadastrar o usuario na nossa base
    this.cookieService.set(this.cookieName, credentials.username);
    this._dialogRef.close();
  }

  public notUser() {
    this.cookieService.delete(this.cookieName);
    this.isLogged = false;
  }
}
