import { Component, OnInit } from '@angular/core';
import { MatSnackBar } from '@angular/material';
import { NotificationClass } from '../../shared/classes/notification';
import { Router } from '@angular/router';
import { ProfileTestResponse } from 'src/app/models/profile-test.interface';
import { SettingsProfileTestsService } from '../_services/profile-tests.service';

@Component({
  selector: 'app-settings-product-suggestion',
  templateUrl: './product-suggestion.component.html',
  styleUrls: ['./product-suggestion.component.scss']
})
export class ProductSuggestionComponent extends NotificationClass implements OnInit {

  public readonly displayedColumns: string[] = [
    'userName', 'registerId', 'testTitle', 'createdAt', 'grade'
  ];
  public responses: Array<ProfileTestResponse>;
  public itemsCount: number = 0;

  private _currentPage: number = 1;
  private _testId: string = null;

  constructor(
    protected _snackBar: MatSnackBar,
    private _router: Router,
    private _profileTestService: SettingsProfileTestsService
  ) {
    super(_snackBar);
  }

  ngOnInit() {
    this._loadResponses(this._currentPage);
  }

  public goToRecommendation(response: ProfileTestResponse) {
    this._router.navigate([ 'configuracoes/recomendacoes-produtos/' + response.id ]);
  }

  public goToPage(page: number): void {
    if (page !== this._currentPage) {
      this._currentPage = page;
      this._loadResponses(this._currentPage);
    }
  }

  private _loadResponses(page: number): void {
    this._profileTestService.getProfileTestResponses(
      page, 10, this._testId
    ).subscribe((response) => {
      this.responses = this._setFinalGrades( response.data.responses );
      this.itemsCount = response.data.itemsCount;

    }, (error) => this.notify( this.getErrorNotification(error) ));
  }

  private _setFinalGrades(responses: Array<ProfileTestResponse>): Array<ProfileTestResponse> {
    responses.forEach(response => {
      if (response.answers.every(a => a.grade && a.grade > 0)) {
        response.finalGrade = response.answers.reduce(
          (sum, a) => sum + a.grade
        , 0);
      }
    });

    return responses;
  }
}
