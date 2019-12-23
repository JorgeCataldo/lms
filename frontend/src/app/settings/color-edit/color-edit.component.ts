import { Component, OnInit } from '@angular/core';
import { FormGroup, FormControl } from '@angular/forms';
import { NotificationClass } from 'src/app/shared/classes/notification';
import { MatSnackBar } from '@angular/material';
import { ColorKey } from 'src/app/models/color-key';
import { SettingsUsersService } from '../_services/users.service';
import { AuthService } from 'src/app/shared/services/auth.service';


@Component({
  selector: 'app-color-edit',
  templateUrl: './color-edit.component.html',
  styleUrls: ['./color-edit.component.scss']
})
export class ColorEditComponent extends NotificationClass implements OnInit {

  public formGroup: FormGroup;
  public BusinessId: string;
  public foundColorPrimary: boolean = false;
  public foundColorPrimaryAlternate: boolean = false;

  constructor(
    protected _snackBar: MatSnackBar,
    private _settingsUsersService: SettingsUsersService,
    private _authService: AuthService
    ) {
       super(_snackBar);
      }

  ngOnInit(): void {
    this.formGroup = this._createColorEditForm();
    this._loadColorPalette();
  }

  private _createColorEditForm(): FormGroup {
    return new FormGroup({
      '--primary-color': new FormControl('#23BCD1'),
      '--alternate-primary-color': new FormControl('#239BD1'),
      '--semi-primary-color': new FormControl('#89D2DC'),
      '--card-primary-color': new FormControl('#23BCD1'),
      '--third-primary-color': new FormControl('#1988c8'),
      '--divider-color': new FormControl('#1c96df'),
      '--selected-color': new FormControl('#43b3f3'),
      '--progress-bar-uncomplete': new FormControl('#e8e8e8'),
      '--progress-bar-complete': new FormControl('#80d1dc'),
      '--danger-color': new FormControl('#FF8D9E'),
      '--warn-color': new FormControl('#FFE08D'),
      '--success-color': new FormControl('#A8F5B4'),
      '--text-color': new FormControl('#5D5D5D'),
      '--alt-text-color': new FormControl('#4f4f4f'),
      '--box-shadow': new FormControl('#b4b4b4'),
      '--header-accent': new FormControl('#efefef'),
      '--main-background': new FormControl('#ffffff'),
      '--light-color': new FormControl('#1aAFc9'),
      '--footer-background': new FormControl('#EDEDED')
    });
  }

  private _loadColorPalette() {
    this._settingsUsersService.getUserColorPalette().subscribe(res => {
      const colors: ColorKey[] = res.data.colorBaseValues;
      if (colors && colors.length > 0) {
        colors.forEach(e => {
          this.formGroup.get(e.key).setValue(e.color);
        });
      }
    }, err => {
      this.notify(this.getErrorNotification(err));
    });
  }

  public saveColorEdit(): void {
    const colorKeys: ColorKey[] = [];
    const colorEdit = this.formGroup.getRawValue();
    for (const key in colorEdit) {
      if (colorEdit.hasOwnProperty(key)) {
        colorKeys.push({
          key: key,
          color: colorEdit[key]
        });
      }
    }
    this._settingsUsersService.updateUserColorPalette(colorKeys).subscribe(() => {
      localStorage.setItem('color-palette', JSON.stringify(colorKeys));
      this._authService.updateColorPallete();
      this.notify('As cores foram mudadas com sucesso');
    }, err => {
      this.notify(this.getErrorNotification(err));
    });
  }

}
