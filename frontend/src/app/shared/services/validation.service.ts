import { Injectable } from '@angular/core';

@Injectable()
export class ValidationsService {

  public validateCNPJ(cnpjStr: string): boolean {
    if (!cnpjStr) return false;

    const cnpj = cnpjStr.replace(/[^\d]+/g, '');
    let i = 0;
    let l = 0;
    let strNum = '';
    const strMul = '6543298765432';
    let iSoma = 0;
    let strNum_base = '';
    let iLenNum_base = 0;
    let iLenMul = 0;

    if (cnpj !== '') {

      if ((cnpj === '00000000000000') || (cnpj === '11111111111111') || (cnpj === '22222222222222') ||
          (cnpj === '33333333333333') || (cnpj === '44444444444444') || (cnpj === '55555555555555') ||
          (cnpj === '66666666666666') || (cnpj === '77777777777777') || (cnpj === '88888888888888') ||
          (cnpj === '99999999999999'))
          return false;

      l = cnpj.length;
      for (i = 0; i < l; i++) {
        const caracter = cnpj.substring(i, i + 1);
        if ((caracter >= '0') && (caracter <= '9'))
            strNum = strNum + caracter;
      }

      if (strNum.length !== 14)
          return false;

      strNum_base = strNum.substring(0, 12);
      iLenNum_base = strNum_base.length - 1;
      iLenMul = strMul.length - 1;
      for (i = 0; i < 12; i++)
          iSoma = iSoma +
              parseInt(strNum_base.substring((iLenNum_base - i), (iLenNum_base - i) + 1), 10) *
              parseInt(strMul.substring((iLenMul - i), (iLenMul - i) + 1), 10);

      iSoma = 11 - (iSoma - Math.floor(iSoma / 11) * 11);
      if (iSoma === 11 || iSoma === 10)
          iSoma = 0;

      strNum_base = strNum_base + iSoma;
      iSoma = 0;
      iLenNum_base = strNum_base.length - 1;

      for (i = 0; i < 13; i++) {
        iSoma = iSoma +
          parseInt(
            strNum_base.substring((iLenNum_base - i), (iLenNum_base - i) + 1), 10
          ) * parseInt(
            strMul.substring((iLenMul - i), (iLenMul - i) + 1), 10
          );
      }
      iSoma = 11 - (iSoma - Math.floor(iSoma / 11) * 11);
      if (iSoma === 11 || iSoma === 10)
          iSoma = 0;
      strNum_base = strNum_base + iSoma;

      if (strNum !== strNum_base)
          return false;
      return true;
    }
  }
}
