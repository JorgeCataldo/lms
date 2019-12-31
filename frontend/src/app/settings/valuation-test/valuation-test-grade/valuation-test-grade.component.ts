import { Component, OnInit } from '@angular/core';
import { MatSnackBar } from '@angular/material';
import { Router, ActivatedRoute } from '@angular/router';
import { ProfileTestResponse } from 'src/app/models/profile-test.interface';
import { NotificationClass } from 'src/app/shared/classes/notification';
import { ExcelService } from 'src/app/shared/services/excel.service';
import { SettingsValuationTestsService } from '../../_services/valuation-tests.service';

@Component({
  selector: 'app-settings-valuation-test-grade',
  templateUrl: './valuation-test-grade.component.html',
  styleUrls: ['./valuation-test-grade.component.scss']
})
export class SettingsValuationTestGradeComponent extends NotificationClass implements OnInit {

  public response: ProfileTestResponse;

  private _responseId: string;
  private _testId: string;

  constructor(
    protected _snackBar: MatSnackBar,
    private _activatedRoute: ActivatedRoute,
    private _router: Router,
    private _valuationTestService: SettingsValuationTestsService,
    private _excelService: ExcelService
  ) {
    super(_snackBar);
  }

  ngOnInit() {
    this._testId = this._activatedRoute.snapshot.paramMap.get('testId');
    this._responseId = this._activatedRoute.snapshot.paramMap.get('responseId');
    this._loadResponse(this._responseId);
  }

  public gradeProfileTestAnswers(): void {
    if (this._checkGrades( this.response )) {
      this._valuationTestService.gradeValuationTestAnswers(
        this.response.id, this.response.answers
      ).subscribe(() => {
        this.notify('Notas atribuídas com sucesso!');
        this._router.navigate([ 'configuracoes/teste-de-avaliacao/repostas/' + this._testId ]);
      }, (err) => this.notify( this.getErrorNotification(err) ) );
    }
  }

  private _checkGrades(response: ProfileTestResponse): boolean {
    if (response.answers.some(a => a.grade === null)) {
      this.notify('Atribua todas as notas para continuar');
      return false;
    }

    const hasInvalidGrades = response.answers.some(a =>
      a.grade < 0 || a.grade > 100 || a.grade > a.percentage
    );

    if (hasInvalidGrades) {
      this.notify('As notas devem ser valores positivos e menores ou iguais ao valor da questão');
      return false;
    }

    return true;
  }

  public exportAnswers(): void {
    this._excelService.exportAsExcelFile(
      this.response.answers, 'Resposta - ' + this.response.testTitle
    );
  }

  private _loadResponse(responseId: string): void {
    this._valuationTestService.getValuationTestResponseById(
      responseId
    ).subscribe((response) => {
      this.response = this._setFinalGrade(response.data);
    }, (error) => this.notify( this.getErrorNotification(error) ));
  }

  private _setFinalGrade(response: ProfileTestResponse): ProfileTestResponse {
    response.answers.forEach(a => {
      a.gradeIsSet = a.grade !== null;
    });

    if (response.answers.every(a => a.grade && a.grade > 0)) {
      response.finalGrade = response.answers.reduce(
        (sum, a) => sum + a.grade
      , 0);
    }
    return response;
  }
}
