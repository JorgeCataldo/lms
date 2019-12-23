import { Component, OnInit} from '@angular/core';
import { FormGroup, FormArray, FormControl, Validators, AbstractControl } from '@angular/forms';
import { NotificationClass } from 'src/app/shared/classes/notification';
import { MatSnackBar, MatDialog } from '@angular/material';
import { Router } from '@angular/router';
import { AuthService } from 'src/app/shared/services/auth.service';

@Component({
    templateUrl: 'email-confirmation.component.html',
    styleUrls: ['email-confirmation.component.scss']
})

export class EmailConfirmationComponent extends NotificationClass implements OnInit {
    public formGroup: FormGroup;
    public email: string;

    constructor(
        protected _snackBar: MatSnackBar,
        private _authService: AuthService,
        private router: Router) {
            super(_snackBar);
    }

    ngOnInit() {
        const emailVerif = this._authService.getEmailVerification();
        if (emailVerif)
            this.email = emailVerif.email ? emailVerif.email : '';
        this.formGroup = this._newForm();
        this.checkSendEmail();
    }

    private _newForm(): FormGroup {
        return new FormGroup({
            'code': new FormControl('', [Validators.required])
        });
    }

    public async confirm() {
        const rawForm = this.formGroup.getRawValue();
        try {
            const result = await this._authService.verifyEmailCode(rawForm.code);
            if (result.success) {
                this.router.navigate(['home']);
            } else {
                const error = result && result.errors && result.errors.length > 0 ?
                result.errors[0] : 'Houve um erro ao se comunicar com o servidor. Por favor tente mais tarde.';
                this.notify(error);
            }
        } catch (err) {
            this.notify(this.getErrorNotification(err));
        }
    }

    public async sendNewVerificationCode() {
        await this._authService.sendVerificationEmail(true);
    }

    private async checkSendEmail() {
        await this._authService.sendVerificationEmail();
    }

}
