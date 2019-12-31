import { Component, Inject } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material';
import { UserService } from 'src/app/pages/_services/user.service';
import { SettingsUsersService } from 'src/app/settings/_services/users.service';
import { Activation } from 'src/app/models/activation.model';

@Component({
  selector: 'app-nps-dialog',
  template: `
    <div class="nps-dialog" >
      <h4>{{ data.title }}</h4>
      <p> {{ data.text }}</p>
      <div class="content-title">
        <span class="tier"><b>NEM UM POUCO PROVÁVEL</b></span>
        <span class="tier"><b>EXTREMAMENTE PROVÁVEL</b></span>
      </div>
      <div>
        <div class="radio-options">
          <label class="container" >
            <input type="radio" name="sliderValue" (click)="setSliderValue(0)" />
            <span class="checkmark"></span>
            <span class="radio-value">0</span>
          </label>
          <label class="container" >
            <input type="radio" name="sliderValue" (click)="setSliderValue(1)" />
            <span class="checkmark"></span>
            <span class="radio-value" style="left: 13px;">1</span>
          </label>
          <label class="container" >
            <input type="radio" name="sliderValue" (click)="setSliderValue(2)" />
            <span class="checkmark"></span>
            <span class="radio-value" style="left: 12px;">2</span>
          </label>
          <label class="container" >
            <input type="radio" name="sliderValue" (click)="setSliderValue(3)" />
            <span class="checkmark"></span>
            <span class="radio-value" style="left: 12px;">3</span>
          </label>
          <label class="container" >
            <input type="radio" name="sliderValue" (click)="setSliderValue(4)" />
            <span class="checkmark"></span>
            <span class="radio-value">4</span>
          </label>
          <label class="container" >
            <input type="radio" name="sliderValue" (click)="setSliderValue(5)" />
            <span class="checkmark"></span>
            <span class="radio-value">5</span>
          </label>
          <label class="container" >
            <input type="radio" name="sliderValue" (click)="setSliderValue(6)" />
            <span class="checkmark"></span>
            <span class="radio-value">6</span>
          </label>
          <label class="container" >
            <input type="radio" name="sliderValue" (click)="setSliderValue(7)" />
            <span class="checkmark"></span>
            <span class="radio-value">7</span>
          </label>
          <label class="container" >
            <input type="radio" name="sliderValue" (click)="setSliderValue(8)" />
            <span class="checkmark"></span>
            <span class="radio-value">8</span>
          </label>
          <label class="container" >
            <input type="radio" name="sliderValue" (click)="setSliderValue(9)" />
            <span class="checkmark"></span>
            <span class="radio-value" style="left: 12px;">9</span>
          </label>
          <label class="container" >
            <input type="radio" name="sliderValue" (click)="setSliderValue(10)" />
            <span class="checkmark"></span>
            <span class="radio-value" style="left: 7px;">10</span>
          </label>
        </div>
      </div>
    </div>
  `,
  styleUrls: ['./nps.dialog.scss']
})
export class NpsDialogComponent {

  private _valuating: boolean = false;

  constructor(public dialogRef: MatDialogRef<NpsDialogComponent>,
              private _userService: SettingsUsersService,
              @Inject(MAT_DIALOG_DATA) public data: Activation) { }

  public dismiss(): void {
    this.dialogRef.close(null);
  }

  public setSliderValue(value: number) {
    if (!this._valuating) {
      this._valuating = true;
      this._userService.saveNpsValuation(value).subscribe(res => {
        this.dialogRef.close('Avaliação salva com sucesso');
      }, () => {
        this.dialogRef.close('Ocorreu um erro ao salvar sua avaliação, tente novamente mais tarde');
      });
    }
  }

}
