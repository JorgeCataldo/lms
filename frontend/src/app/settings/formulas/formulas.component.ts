import { Component, OnInit } from '@angular/core';
import { MatSnackBar, MatDialog } from '@angular/material';
import { NotificationClass } from '../../shared/classes/notification';
import { Router } from '@angular/router';
import { Formula } from 'src/app/models/formula.model';
import { SettingsFormulasService } from '../_services/formulas.service';
import { ConfirmDialogComponent } from 'src/app/shared/dialogs/confirm/confirm.dialog';

@Component({
  selector: 'app-settings-formulas',
  templateUrl: './formulas.component.html',
  styleUrls: ['./formulas.component.scss']
})
export class FormulasComponent extends NotificationClass implements OnInit {

  public formulas: Array<Formula>;
  public itemsCount: number = 0;

  private _currentPage: number = 1;

  constructor(
    protected _snackBar: MatSnackBar,
    private _router: Router,
    private _dialog: MatDialog,
    private _formulasService: SettingsFormulasService
  ) {
    super(_snackBar);
  }

  ngOnInit() {
    this._loadFormulas();
  }

  public createNewFormula(): void {
    this._router.navigate([ 'configuracoes/formulas/0' ]);
  }

  public manageFormula(formula: Formula): void {
    this._router.navigate([ 'configuracoes/formulas/' + formula.id ]);
  }

  public deleteFormula(formula: Formula) {
    const dialogRef = this._dialog.open(ConfirmDialogComponent, {
      width: '1000px',
      data: { message: 'Tem certeza que deseja remover esta fórmula?' }
    });

    dialogRef.afterClosed().subscribe((result: boolean) => {
      if (result) {
        this._formulasService.deleteFormula(formula.id).subscribe(() => {
          this._loadFormulas();
          this.notify('Fórmula deletada com sucesso');

        }, (error) => this.notify( this.getErrorNotification(error) ));
      }
    });
  }

  public goToPage(page: number): void {
    if (page !== this._currentPage) {
      this._currentPage = page;
      this._loadFormulas( this._currentPage );
    }
  }

  private _loadFormulas(page: number = 1): void {
    this._formulasService.getFormulas().subscribe((response) => {
      this.formulas = response.data.formulas;
      this.itemsCount = response.data.itemsCount;

    }, (error) => this.notify( this.getErrorNotification(error) ));
  }
}
