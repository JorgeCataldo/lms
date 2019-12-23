import { Component, OnInit } from '@angular/core';
import { MatSnackBar } from '@angular/material';
import { Router } from '@angular/router';
import { NotificationClass } from 'src/app/shared/classes/notification';
import { SettingsValuationTestsService } from '../../_services/valuation-tests.service';
import { ValuationTest } from 'src/app/models/valuation-test.interface';
import { ValuationTestTypeEnum } from 'src/app/models/enums/valuation-test-type-enum';

@Component({
  selector: 'app-settings-manage-valuation-test-release',
  templateUrl: './manage-valuation-test-release.component.html',
  styleUrls: ['./manage-valuation-test-release.component.scss']
})
export class SettingsManageValuationTestReleaseComponent extends NotificationClass implements OnInit {

  public test: ValuationTest;

  constructor(
    protected _snackBar: MatSnackBar,
    private _testsService: SettingsValuationTestsService,
    private _router: Router
  ) {
    super(_snackBar);
  }

  ngOnInit() {
    const valuationStr = localStorage.getItem('editingValuationTest');
    if (valuationStr && valuationStr.trim() !== '') {
      this.test = JSON.parse(valuationStr);
    } else {
      this._router.navigate([ 'configuracoes/testes-de-avaliacao' ]);
    }
  }

  public save(): void {
    if (this._checkTest()) {
      this._testsService.manageValuationTest(
        this.test
      ).subscribe(() => {
        this.notify('Salvo com sucesso!');
        this._router.navigate([ 'configuracoes/testes-de-avaliacao' ]);
      }, (error) => this.notify( this.getErrorNotification(error) ));
    }
  }

  private _checkTest(): boolean {
    let returnValue: boolean = true;
    if (this.test.type === ValuationTestTypeEnum.Percentile) {
      this.test.testModules.forEach(testmod => {
        if (testmod.percent === undefined || testmod.percent === null) {
          this.notify('O módulo ' + testmod.title + ' não esta configurado corretamente');
          returnValue = false;
          return false;
        }
      });
      this.test.testTracks.forEach(testtra => {
        if (testtra.percent === undefined || testtra.percent === null) {
          this.notify('A trilha ' + testtra.title + ' não esta configurada corretamente');
          returnValue = false;
          return false;
        }
      });
    } else if (this.test.type === ValuationTestTypeEnum.Free) {
      this.test.testModules.forEach(testmod => {
        if (testmod.type === undefined || testmod.type === null) {
          this.notify('O módulo ' + testmod.title + ' não esta configurado corretamente');
          returnValue = false;
          return false;
        }
      });
      this.test.testTracks.forEach(testtra => {
        if (testtra.order === undefined || testtra.order === null) {
          this.notify('A trilha ' + testtra.title + ' não esta configurada corretamente');
          returnValue = false;
          return false;
        }
      });
    }
    return returnValue;
  }
}
