import { Component, OnInit } from '@angular/core';
import { NotificationClass } from '../../shared/classes/notification';
import { MatSnackBar } from '@angular/material';
import { FormGroup } from '@angular/forms';
import { ContactArea } from 'src/app/models/previews/contact-area.interface';
import { SettingsProfileTestsService } from 'src/app/settings/_services/profile-tests.service';
import { ActivatedRoute, Router } from '@angular/router';
import { ProfileTest } from 'src/app/models/profile-test.interface';

@Component({
  selector: 'app-profile-test',
  templateUrl: './profile-test.component.html',
  styleUrls: ['./profile-test.component.scss']
})
export class ProfileTestComponent extends NotificationClass implements OnInit {

  public test: ProfileTest;

  constructor(
    protected _snackBar: MatSnackBar,
    private _activatedRoute: ActivatedRoute,
    private _router: Router,
    private _testService: SettingsProfileTestsService
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

    this._testService.saveProfileTestResponse(
      this.test
    ).subscribe(() => {
      this.notify('Resposta salva com sucesso!');
      this._router.navigate([ 'home' ]);

    }, (error) => this.notify( this.getErrorNotification(error) ));
  }

  private _loadTest(testId: string) {
    this._testService.getProfileTestById(
      testId
    ).subscribe((response) => {
      this.test = response.data;

    }, (error) => this.notify( this.getErrorNotification(error) ));
  }
}
