import { Component, OnInit } from '@angular/core';
import { NotificationClass } from '../../shared/classes/notification';
import { MatSnackBar } from '@angular/material';
import { SettingsValuationTestsService } from 'src/app/settings/_services/valuation-tests.service';
import { ActivatedRoute, Router } from '@angular/router';
import { ValuationTest } from 'src/app/models/valuation-test.interface';

@Component({
  selector: 'app-valuation-test',
  templateUrl: './valuation-test.component.html',
  styleUrls: ['./valuation-test.component.scss']
})
export class ValuationTestComponent extends NotificationClass implements OnInit {

  public test: ValuationTest;

  constructor(
    protected _snackBar: MatSnackBar,
    private _activatedRoute: ActivatedRoute,
    private _router: Router,
    private _testService: SettingsValuationTestsService
  ) {
    super(_snackBar);
  }

  ngOnInit() {
    const testId = this._activatedRoute.snapshot.paramMap.get('testId');
    this._loadTest( testId );
  }

  public checkFullyAnswered(): boolean {
    return this.test.testQuestions.every(tQ =>
      tQ.answer && tQ.answer.trim() !== ''
    );
  }

  public sendResponse(): void {
    const isFullyAnswered = this.checkFullyAnswered();

    if (!isFullyAnswered) {
      this.notify('Por favor, responda todas as perguntas para continuar');
      return;
    }

    this._testService.saveValuationTestResponse(
      this.test
    ).subscribe(() => {
      this.notify('Resposta salva com sucesso!');
      window.history.back();
    }, (error) => {
      this.notify( this.getErrorNotification(error));
      this._router.navigate([ 'home' ]);
    });
  }

  private _loadTest(testId: string) {
    this._testService.getValuationTestById(
      testId
    ).subscribe((response) => {
      this.test = response.data;
    }, (error) => {
      this.notify( this.getErrorNotification(error));
      this._router.navigate([ 'home' ]);
    });
  }
}
