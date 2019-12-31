import { Component, OnInit } from '@angular/core';
import { MatSnackBar, MatDialog } from '@angular/material';
import { NotificationClass } from '../../shared/classes/notification';
import { Router } from '@angular/router';
import { ConfirmDialogComponent } from 'src/app/shared/dialogs/confirm/confirm.dialog';
import { SettingsValuationTestsService } from '../_services/valuation-tests.service';
import { ValuationTest, ValuationTestExcel, ValuationTestResponse } from 'src/app/models/valuation-test.interface';
import { ExcelService } from 'src/app/shared/services/excel.service';

@Component({
  selector: 'app-settings-valuation-tests',
  templateUrl: './valuation-tests.component.html',
  styleUrls: ['./valuation-tests.component.scss']
})
export class ValuationTestsComponent extends NotificationClass implements OnInit {

  public tests: Array<ValuationTest>;

  constructor(
    protected _snackBar: MatSnackBar,
    private _router: Router,
    private _dialog: MatDialog,
    private _testsService: SettingsValuationTestsService,
    private _excelService: ExcelService
  ) {
    super(_snackBar);
  }

  ngOnInit() {
    localStorage.removeItem('editingValuationTest');
    this._loadTests();
  }

  public createNewTest(): void {
    this._router.navigate(['configuracoes/teste-de-avaliacao']);
  }

  public viewTest(test: ValuationTest): void {
    this._router.navigate(['configuracoes/teste-de-avaliacao/repostas/' + test.id]);
  }

  public manageTest(test: ValuationTest): void {
    localStorage.setItem('editingValuationTest', JSON.stringify(test));
    this._router.navigate(['configuracoes/teste-de-avaliacao']);
  }

  public deleteTest(test: ValuationTest) {
    const dialogRef = this._dialog.open(ConfirmDialogComponent, {
      width: '1000px',
      data: { message: 'Tem certeza que deseja remover este teste?' }
    });

    dialogRef.afterClosed().subscribe((result: boolean) => {
      if (result) {
        this._testsService.deleteValuationTest(test.id).subscribe(() => {
          this._loadTests();
          this.notify('Teste deletado com sucesso');

        }, (error) => this.notify(this.getErrorNotification(error)));
      }
    });
  }

  public getAnswersExcel(test: ValuationTest): void {
    this._testsService.getAllValuationTestResponses(
      test.id
    ).subscribe((response) => {
      this._excelService.exportAsExcelFile(
        this._prepareAnswersForExport(response.data),
        test.title
      );

    }, (error) => this.notify( this.getErrorNotification(error) ));
  }

  private _loadTests(): void {
    this._testsService.getValuationTests().subscribe((response) => {
      this.tests = response.data;
    }, (error) => this.notify(this.getErrorNotification(error)));
  }

  private _prepareAnswersForExport(responses: Array<ValuationTestResponse>): Array<ValuationTestExcel> {
    const answers: Array<ValuationTestExcel> = [];

    responses.forEach(response => {
      response.answers.forEach(answer => {
        answers.push({
          aluno: response.userName,
          matricula: response.userRegisterId,
          questao: answer.question,
          answer: answer.answer,
          data: response.createdAt
        });
      });
    });

    return answers;
  }
}
