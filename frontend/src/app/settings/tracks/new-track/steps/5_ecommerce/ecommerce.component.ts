import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { FormGroup, FormArray, FormControl, Validators } from '@angular/forms';
import { MatSnackBar } from '@angular/material';
import { NotificationClass } from '../../../../../shared/classes/notification';
import { UtilService } from '../../../../../shared/services/util.service';
import { Track } from '../../../../../models/track.model';
import { EcommerceProduct } from 'src/app/models/ecommerce-product.model';

@Component({
  selector: 'app-new-track-ecommerce',
  templateUrl: './ecommerce.component.html',
  styleUrls: ['../new-track-steps.scss', './ecommerce.component.scss']
})
export class NewTrackEcommerceComponent extends NotificationClass implements OnInit {

  @Input() readonly track: Track;
  @Output() manageEcommerceInfo = new EventEmitter();

  public formGroup: FormGroup;

  constructor(
    protected _snackBar: MatSnackBar,
    private _utilService: UtilService
  ) {
    super(_snackBar);
  }

  ngOnInit() {
    this.formGroup = this._createFormGroup();
    this._fillForm( this.track );
  }

  public addEcommerceId(): void {
    const products = this.formGroup.get('ecommerceProducts') as FormArray;
    products.push(
      this._createEcommerceProductForm()
    );
  }

  public removeEcommerceProduct(index: number): void {
    const products = this.formGroup.get('ecommerceProducts') as FormArray;
    products.removeAt(index);
  }

  public nextStep(): void {
    if (this.formGroup.valid) {
      const formContent = this.formGroup.getRawValue();

      this._checkProducts( formContent.ecommerceProducts ) ?
        this.manageEcommerceInfo.emit( formContent.ecommerceProducts ) :
        this.notify('Há Ids de Ecommerce duplicado');

    } else {
      this.formGroup = this._utilService.markFormControlsAsTouch( this.formGroup );
      this.notify('Por favor, preencha todos os campos obrigatórios');
    }
  }

  private _createFormGroup(): FormGroup {
    return new FormGroup({
      'ecommerceProducts': new FormArray([])
    });
  }

  private _createEcommerceProductForm(ecommerceProduct: EcommerceProduct = null): FormGroup {
    return new FormGroup({
      'ecommerceId': new FormControl(
        ecommerceProduct ? ecommerceProduct.ecommerceId : '', [ Validators.required ]
      ),
      'usersAmount': new FormControl(
        ecommerceProduct ? ecommerceProduct.usersAmount : '', [ Validators.required ]
      ),
      'disableEcommerce': new FormControl(
        ecommerceProduct ? ecommerceProduct.disableEcommerce : '', [ Validators.required ]
      ),
      'price': new FormControl(
        ecommerceProduct ? ecommerceProduct.price : '', [ Validators.required ]
      ),
      'disableFreeTrial': new FormControl(
        ecommerceProduct ? ecommerceProduct.disableFreeTrial : '', [ Validators.required ]
      ),
      'linkEcommerce': new FormControl(
        ecommerceProduct ? ecommerceProduct.linkEcommerce : '', [ Validators.required ]
      ),
      'linkProduct': new FormControl(
        ecommerceProduct ? ecommerceProduct.linkProduct : '', [ Validators.required ]
      ),
      'subject': new FormControl(
        ecommerceProduct ? ecommerceProduct.subject : '', [ Validators.required ]
      ),
      'hours': new FormControl(
        ecommerceProduct ? ecommerceProduct.hours : '', [ Validators.required ]
      )
    });
  }

  private _fillForm(track: Track): void {
    if (track.ecommerceProducts) {
      const products = this.formGroup.get('ecommerceProducts') as FormArray;
      track.ecommerceProducts.forEach(product => {
        products.push(
          this._createEcommerceProductForm(product)
        );
      });
    }
  }

  private _checkProducts(formContent: Array<EcommerceProduct>): boolean {
    return formContent.every(outerProd => {
      return formContent.filter(innerProd => {
        return innerProd.ecommerceId === outerProd.ecommerceId;
      }).length === 1;
    });
  }

  public cleanLevel(index: number): void {
    const formArray = this.formGroup.get('ecommerceProducts') as FormArray;
    const formControl = formArray.controls[index];

    if (formControl.get('disableFreeTrial').value) {
      formControl.get('price').setValue('');
      formControl.get('price').disable();
    } else {
      formControl.get('price').enable();
    }
  }
}
