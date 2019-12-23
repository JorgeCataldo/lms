import { Component, OnInit } from '@angular/core';
import { MatSnackBar, MatDialog } from '@angular/material';
import { ActivatedRoute, Router } from '@angular/router';
import { NotificationClass } from 'src/app/shared/classes/notification';
import { SettingsProfileTestsService } from '../../_services/profile-tests.service';
import { ProfileTest, ProfileTestQuestion, ProfileTestTypeEnum } from 'src/app/models/profile-test.interface';
import { ManageProfileQuestionDialogComponent } from './manage-question/manage-question.dialog';
import { ConfirmDialogComponent } from 'src/app/shared/dialogs/confirm/confirm.dialog';
import { ExcelService } from 'src/app/shared/services/excel.service';

@Component({
  selector: 'app-settings-manage-test',
  templateUrl: './manage-test.component.html',
  styleUrls: ['./manage-test.component.scss']
})
export class SettingsManageTestComponent extends NotificationClass implements OnInit {

  public test: ProfileTest;

  constructor(
    protected _snackBar: MatSnackBar,
    private _activatedRoute: ActivatedRoute,
    private _testsService: SettingsProfileTestsService,
    private _router: Router,
    private _dialog: MatDialog,
    private _excelService: ExcelService
  ) {
    super(_snackBar);
  }

  ngOnInit() {
    const testId = this._activatedRoute.snapshot.paramMap.get('testId');
    testId === '0' ?
      this.test = new ProfileTest() :
      this._loadTest(testId);
  }

  public openQuestionDialog(question: ProfileTestQuestion = null) {
    if (!question)
      question = new ProfileTestQuestion();

    question.testTitle = this.test.title;
    question.testId = this.test.id;

    const dialogRef = this._dialog.open(ManageProfileQuestionDialogComponent, {
      width: '1000px',
      data: question
    });

    dialogRef.afterClosed().subscribe((editedQuestion: ProfileTestQuestion) => {
      if (editedQuestion) {
        question = editedQuestion;
        if (!question.id && question.newQuestion) {
          this.test.testQuestions.push(question);
          question.newQuestion = false;
        }
      }
    });
  }

  public confirmRemoveQuestion(index: number): void {
    const dialogRef = this._dialog.open(ConfirmDialogComponent, {
      width: '400px',
      data: { message: 'Tem certeza que deseja remover esta pergunta?' }
    });

    dialogRef.afterClosed().subscribe((result: boolean) => {
      if (result)
        this._removeQuestion(index);
    });
  }

  public openFileUpload(): void {
    (document.getElementById('inputFile') as HTMLElement).click();
  }

  public setDocumentFile(event) {
    if (event.target && event.target.files && event.target.files.length > 0) {
      const file = event.target.files[0];
      const extension = file.name.split('.').pop();

      if (extension !== 'xls' && extension !== 'xlsx' && extension !== 'csv') {
        this.notify('Tipo de arquivo inválido. Apenas \'xls\', \'xlsx\' e \'csv\' são permitidos.');
        return;
      }

      this._readExcel( file );
    }
  }

  public importExcel(file): void {
    const questions = this._excelService.getExcelContentAsJson(file);

    console.log('questions -> ', questions);

    for (let index = 0; index < questions.length; index++) {
      const question = questions[index];

      if (!question.title || question.title.trim() === '') {
        this.notify('Erro ao importar planilha: Linha ' + (index + 1) + ' não possui enunciado');
        break;
      }

      if (
        !question.type || (
        question.type !== ProfileTestTypeEnum.MultipleChoice &&
        question.type !== ProfileTestTypeEnum.Discursive
      )) {
        this.notify('Erro ao importar planilha: Linha ' + (index + 1) + ' possui um tipo inválido');
        break;
      }

      if (question.type === ProfileTestTypeEnum.MultipleChoice) {
        if (!question.options) {
          this.notify('Erro ao importar planilha: Linha ' + (index + 1) + ' requer opções de resposta (múltipla-escolha)');
          break;
        }

        const sheetOptions: Array<string> = (question.options as any).split(',');
        question.options = [];

        sheetOptions.forEach(option => {
          question.options.push({
            'text': option.trim(),
            'correct': false
          });
        });

        if (question.options.length === 0) {
          this.notify('Erro ao importar planilha: Linha ' + (index + 1) + ' requer opções de resposta (múltipla-escolha)');
          break;
        } else if (question.options.length === 1) {
          this.notify('Erro ao importar planilha: Linha ' + (index + 1) + ' requer mais de uma opção de resposta (múltipla-escolha)');
          break;
        }
      }

      question.testTitle = this.test.title;
      question.testId = this.test.id;
    }

    this.notify('Perguntas importadas com sucesso!');
    this.test.testQuestions = questions as ProfileTestQuestion[];
  }

  private _readExcel(file) {
    const callback = this.importExcel.bind(this);
    const reader = new FileReader();
    reader.onloadend = function (e) {
      let binary = '';
      const bytes = new Uint8Array(this.result as any);

      for (let i = 0; i < bytes.byteLength; i++)
        binary += String.fromCharCode(bytes[i]);

      callback( binary );
    };
    reader.readAsArrayBuffer(file);
  }

  public save(): void {
    if (this._checkTest(this.test)) {
      this._testsService.manageProfileTest(
        this.test
      ).subscribe(() => {
        this.notify('Salvo com sucesso!');
        this._router.navigate([ 'configuracoes/pesquisa-na-base' ]);

      }, (error) => this.notify( this.getErrorNotification(error) ));
    }
  }

  private _loadTest(testId: string): void {
    this._testsService.getProfileTestById(
      testId
    ).subscribe((response) => {
      this.test = response.data;

    }, (error) => this.notify( this.getErrorNotification(error) ));
  }

  private _removeQuestion(index: number): void {
    this.test.testQuestions.splice(index, 1);
  }

  private _checkTest(test: ProfileTest): boolean {
    const sumPercentages = test.testQuestions.reduce(
      (sum, q) => sum + q.percentage
    , 0);

    if (sumPercentages !== 100) {
      this.notify('A soma dos pesos das questões deve ser 100');
      return false;
    }

    return true;
  }
}
