import { MatSnackBar } from '@angular/material';

export class NotificationClass {

  constructor(protected _snackBar: MatSnackBar) { }

  protected notify(message: string, actionText: string = 'OK') {
    this._snackBar.open(
      message,
      actionText,
      { duration: 4000, verticalPosition: 'top' }
    );
  }

  protected getErrorNotification(response): string {
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
