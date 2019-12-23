import { Component, OnInit } from '@angular/core';
import { MatSnackBar, MatDialog } from '@angular/material';
import { NotificationClass } from '../../shared/classes/notification';
import { Router } from '@angular/router';
import { ConfirmDialogComponent } from 'src/app/shared/dialogs/confirm/confirm.dialog';
import { SettingsProfileTestsService } from '../../settings/_services/profile-tests.service';
import { ProfileTest, ProfileTestExcel, ProfileTestResponse } from 'src/app/models/profile-test.interface';
import { ExcelService } from 'src/app/shared/services/excel.service';
import { ActivationsService } from '../../settings/_services/activations.service';
import { Activation } from 'src/app/models/activation.model';
import { ActivationEditDialogComponent } from './activation-edit-dialog/activation-edit.dialog';
import { ActivationTypeEnum } from 'src/app/models/enums/activation-status.enum';

@Component({
  selector: 'app-settings-profile-tests-results',
  templateUrl: './profile-tests-results.component.html',
  styleUrls: ['./profile-tests-results.component.scss']
})
export class ProfileTestsResultsComponent extends NotificationClass implements OnInit {

  public tests: Array<ProfileTest>;
  public researchs: Array<ProfileTest>;
  public isNps: boolean = false;

  constructor(
    protected _snackBar: MatSnackBar,
    private _router: Router,
    private _dialog: MatDialog,
    private _testsService: SettingsProfileTestsService,
    private _activationsService: ActivationsService,
    private _excelService: ExcelService
  ) {
    super(_snackBar);
  }

  ngOnInit() {
    const path: string = this._router.url;
    if (path.includes('nps')) {
      this.isNps = true;
    }
    if (this.isNps) {
      this._loadResearch();
    } else {
      this._loadTests();
    }
  }

  public createNewTest(): void {
    this._router.navigate(['configuracoes/pesquisa-na-base/0']);
  }

  public manageTest(test: ProfileTest): void {
    this._router.navigate(['configuracoes/pesquisa-na-base/' + test.id]);
  }

  public deleteTest(test: ProfileTest) {
    const dialogRef = this._dialog.open(ConfirmDialogComponent, {
      width: '1000px',
      data: { message: 'Tem certeza que deseja remover este teste?' }
    });

    dialogRef.afterClosed().subscribe((result: boolean) => {
      if (result) {
        this._testsService.deleteProfileTest(test.id).subscribe(() => {
          this._loadTests();
          this.notify('Teste deletado com sucesso');

        }, (error) => this.notify(this.getErrorNotification(error)));
      }
    });
  }

  public getAnswersExcel(test: ProfileTest): void {
    this._testsService.getAllProfileTestResponses(
      test.id
    ).subscribe((response) => {
      this._excelService.exportAsExcelFile(
        this._prepareAnswersForExport(response.data),
        test.title
      );

    }, (error) => this.notify( this.getErrorNotification(error) ));
  }

  private _loadTests(): void {
    this._testsService.getProfileTests().subscribe((response) => {
      this.tests = response.data;

    }, (error) => this.notify(this.getErrorNotification(error)));
  }

  private _prepareAnswersForExport(responses: Array<ProfileTestResponse>): Array<ProfileTestExcel> {
    const answers: Array<ProfileTestExcel> = [];

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

  private _loadResearch(): void {
    this._activationsService.getActivations().subscribe((response) => {
      this.researchs = response.data;
    }, (error) => this.notify(this.getErrorNotification(error)));
  }

  public viewResearch(act: Activation): void {
    if (act.type === ActivationTypeEnum.Nps) {
      this._router.navigate(['configuracoes/nps']);
    } else {
      this.notify('Visualização ainda não disponivel para pesquisas padrão');
    }
  }

  public createNewResearch(): void {
    const act: Activation = { id: '', type: ActivationTypeEnum.Custom, active: false, title: '', text: '', percentage: 0};
    const dialogRef = this._dialog.open(ActivationEditDialogComponent, {
      width: '400px',
      data: act
    });

    dialogRef.afterClosed().subscribe((result: Activation) => {
      if (result) {
        this._activationsService.createCustomActivation(result).subscribe(() => {
          this._loadResearch();
        }, () => this.notify('Ocorreu um erro, por favor tente novamente mais tarde'));
      }
    });
  }

  public manageResearch(act: Activation) {
    const dialogRef = this._dialog.open(ActivationEditDialogComponent, {
      width: '400px',
      data: act
    });

    dialogRef.afterClosed().subscribe((result: Activation) => {
      if (result) {
        this._activationsService.updateCustomActivation(result).subscribe(() => {
          this._loadResearch();
        }, () => this.notify('Ocorreu um erro, por favor tente novamente mais tarde'));
      }
    });
  }

  public deleteResearch(act: Activation) {
    const dialogRef = this._dialog.open(ConfirmDialogComponent, {
      width: '1000px',
      data: { message: 'Tem certeza que deseja remover esta pesquisa?' }
    });

    dialogRef.afterClosed().subscribe((result: boolean) => {
      if (result) {
        this._activationsService.deleteActivation(act.id).subscribe(() => {
          this._loadResearch();
          this.notify('Pesquisa deletada com sucesso');

        }, (error) => this.notify(this.getErrorNotification(error)));
      }
    });
  }
}
