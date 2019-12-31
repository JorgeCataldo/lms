import { Component, OnInit } from '@angular/core';
import { MatSnackBar } from '@angular/material';
import { NotificationClass } from '../../shared/classes/notification';
import { Router, ActivatedRoute } from '@angular/router';
import { ProfileTestResponse } from 'src/app/models/profile-test.interface';
import { SettingsProfileTestsService } from '../_services/profile-tests.service';
import { SettingsValuationTestsService } from '../_services/valuation-tests.service';

@Component({
  selector: 'app-settings-valuation-test',
  templateUrl: './valuation-test.component.html',
  styleUrls: ['./valuation-test.component.scss']
})
export class SettingsValuationTestComponent extends NotificationClass implements OnInit {

  public readonly displayedColumns: string[] = [
    'userName', 'registerId', 'createdAt', 'grade'
  ];
  public responses: Array<ProfileTestResponse>;
  public itemsCount: number = 0;

  private _currentPage: number = 1;
  private _testId: string = null;
  public testTitle: string = '';

  constructor(
    protected _snackBar: MatSnackBar,
    private _router: Router,
    private _valuationTestService: SettingsValuationTestsService,
    private _activatedRoute: ActivatedRoute
  ) {
    super(_snackBar);
  }

  ngOnInit() {
    this._testId = this._activatedRoute.snapshot.paramMap.get('testId');
    this._loadResponses(this._currentPage);
  }

  public goToRecommendation(response: ProfileTestResponse) {
    this._router.navigate([ 'configuracoes/testes-de-avaliacao/repostas/' + this._testId + '/' + response.id ]);
  }

  public goToPage(page: number): void {
    if (page !== this._currentPage) {
      this._currentPage = page;
      this._loadResponses(this._currentPage);
    }
  }

  private _loadResponses(page: number): void {
    this._valuationTestService.getValuationTestResponses(
      this._testId, page
    ).subscribe((response) => {
      this.responses = this._setFinalGrades(response.data.responses);
      this.itemsCount = response.data.itemsCount;
      this.testTitle = response.data.testTilte;

    }, (error) => this.notify( this.getErrorNotification(error) ));
  }

  private _setFinalGrades(responses: Array<ProfileTestResponse>): Array<ProfileTestResponse> {
    responses.forEach(response => {
      if (response.answers.every(a => a.grade != null)) {
        response.finalGrade = response.answers.reduce(
          (sum, a) => sum + a.grade
        , 0);
      }
    });

    return responses;
  }
}
