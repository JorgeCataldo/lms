import { Injectable } from '@angular/core';
import { MatSnackBar } from '@angular/material';
import { HttpErrorResponse } from '@angular/common/http';

@Injectable()
export class ErrorsService {

  constructor(
    private _snackBar: MatSnackBar
  ) { }

  public notify(message: string) {
    this._snackBar.open(
      message, 'OK',
      { duration: 4000, verticalPosition: 'top' }
    );
  }

  public getErrorNotification(response: HttpErrorResponse): string {
    if (!response || !response.status)
      return 'Ocorreu um erro, por favor tente novamente mais tarde';

    switch (response.status) {
      case 401:
        return 'Você precisa estar logado para finalizar o cadastro';
      case 500:
        return 'Ocorreu um erro, por favor tente novamente mais tarde';
      default:
        if (response.error && response.error.errors && response.error.errors.length > 0)
          return response.error.errors[0];
        return 'Ocorreu um erro, por favor tente novamente mais tarde';
    }
  }
}
