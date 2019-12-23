import { Component, OnInit } from '@angular/core';
import { MatSnackBar, MatDialog } from '@angular/material';
import { ActivatedRoute, Router } from '@angular/router';
import { NotificationClass } from 'src/app/shared/classes/notification';
import { SettingsValuationTestsService } from '../../_services/valuation-tests.service';
// tslint:disable-next-line: max-line-length
import { ValuationTest, ValuationTestQuestion, ValuationTestQuestionTypeEnum, TestModule, TestTrack } from 'src/app/models/valuation-test.interface';
import { ManageValuationQuestionDialogComponent } from './manage-valuation-question/manage-valuation-question.dialog';
import { ConfirmDialogComponent } from 'src/app/shared/dialogs/confirm/confirm.dialog';
import { ExcelService } from 'src/app/shared/services/excel.service';
import { SettingsModulesService } from '../../_services/modules.service';
import { SettingsTracksService } from '../../_services/tracks.service';
import { ModulePreview } from 'src/app/models/previews/module.interface';
import { TrackPreview } from 'src/app/models/previews/track.interface';

@Component({
  selector: 'app-settings-manage-valuation-test',
  templateUrl: './manage-valuation-test.component.html',
  styleUrls: ['./manage-valuation-test.component.scss']
})
export class SettingsManageValuationTestComponent extends NotificationClass implements OnInit {

  public test: ValuationTest;
  public modules: Array<TestModule> = [];
  public tracks: Array<TestTrack> = [];
  public selectedModules: Array<TestModule> = [];
  public selectedTracks: Array<TestTrack> = [];

  constructor(
    protected _snackBar: MatSnackBar,
    private _activatedRoute: ActivatedRoute,
    private _testsService: SettingsValuationTestsService,
    private _router: Router,
    private _dialog: MatDialog,
    private _excelService: ExcelService,
    private _modulesService: SettingsModulesService,
    private _tracksService: SettingsTracksService
  ) {
    super(_snackBar);
  }

  ngOnInit() {
    this._loadModules();
    this._loadTracks();
    const valuationStr = localStorage.getItem('editingValuationTest');
    if (valuationStr && valuationStr.trim() !== '') {
      this.test = JSON.parse(valuationStr);
      this.selectedModules = this.test.testModules;
      this.selectedTracks = this.test.testTracks;
      this.updateModules();
      this.updateTracks();
    } else {
     this.test = new ValuationTest();
    }
  }

  public openQuestionDialog(question: ValuationTestQuestion = null) {
    if (!question)
      question = new ValuationTestQuestion();

    question.testTitle = this.test.title;
    question.testId = this.test.id;

    const dialogRef = this._dialog.open(ManageValuationQuestionDialogComponent, {
      width: '1000px',
      data: question
    });

    dialogRef.afterClosed().subscribe((editedQuestion: ValuationTestQuestion) => {
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

    for (let index = 0; index < questions.length; index++) {
      const question = questions[index];

      if (!question.title || question.title.trim() === '') {
        this.notify('Erro ao importar planilha: Linha ' + (index + 1) + ' não possui enunciado');
        break;
      }

      if (
        !question.type || (
        (question.type as number) !== (ValuationTestQuestionTypeEnum.MultipleChoice as number) &&
        (question.type as number) !== (ValuationTestQuestionTypeEnum.Discursive as number)
      )) {
        this.notify('Erro ao importar planilha: Linha ' + (index + 1) + ' possui um tipo inválido');
        break;
      }

      if ((question.type as number) === (ValuationTestQuestionTypeEnum.MultipleChoice as number)) {
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
    this.test.testQuestions = questions as any[];
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

  public next(): void {
    if (this._checkTest(this.test)) {
      localStorage.setItem('editingValuationTest', JSON.stringify(this.test));
      this._router.navigate([ 'configuracoes/teste-de-avaliacao/liberacao' ]);
    }
  }

  /*private _loadTest(testId: string): void {
    this._testsService.getValuationTestById(
      testId
    ).subscribe((response) => {
      const test = response.data;
      this.selectedModules = test.testModules;
      this.selectedTracks = test.testTracks;
      this.updateModules();
      this.updateTracks();

    }, (error) => this.notify( this.getErrorNotification(error) ));
  }*/

  private _removeQuestion(index: number): void {
    this.test.testQuestions.splice(index, 1);
  }

  private _checkTest(test: ValuationTest): boolean {
    const sumPercentages = test.testQuestions.reduce(
      (sum, q) => sum + q.percentage
    , 0);

    if (sumPercentages !== 100) {
      this.notify('A soma dos pesos das questões deve ser 100');
      return false;
    }
    this.test.moduleIds = this.selectedModules.map(x => x.id);
    this.test.trackIds = this.selectedTracks.map(x => x.id);
    this.test.testModules = this.selectedModules;
    this.test.testTracks = this.selectedTracks;
    this.test.testModules.forEach(mod => { mod.toggled = true; });
    this.test.testTracks.forEach(tra => { tra.toggled = true; });
    return true;
  }

  private _loadModules(searchValue: string = ''): void {
    this._modulesService.getPagedFilteredModulesList(
      1, 4, searchValue
    ).subscribe(response => {
      response.data.modules.forEach((mod: ModulePreview) => {
        mod.checked = this.selectedModules.find(t => t.id === mod.id) && true;
      });
      this.modules = response.data.modules;
    });
  }

  private _loadTracks(searchValue: string = ''): void {
    this._tracksService.getPagedFilteredTracksList(
      1, 4, searchValue
    ).subscribe(response => {
      response.data.tracks.forEach((tck: TrackPreview) => {
        tck.checked = this.selectedTracks.find(t => t.id === tck.id) && true;
      });
      this.tracks = response.data.tracks;
    });
  }

  public updateModules(): void {
    const prevSelected = this.selectedModules.filter(x =>
      !this.modules.find(t => t.id ===  x.id)
    );
    const selectedModules = this.modules.filter(x =>
      x.checked
    );
    this.selectedModules = [ ...prevSelected, ...selectedModules];
  }

  public updateTracks(): void {
    const prevSelected = this.selectedTracks.filter(x =>
      !this.tracks.find(t => t.id ===  x.id)
    );
    const selectedTracks = this.tracks.filter(x =>
      x.checked
    );
    this.selectedTracks = [ ...prevSelected, ...selectedTracks];
  }

  public triggerModuleSearch(searchValue: string) {
    this._loadModules(searchValue);
  }

  public triggerTrackSearch(searchValue: string) {
    this._loadTracks(searchValue);
  }

  public removeSelectedModule(id: string): void {
    const selectedIndex = this.selectedModules.findIndex(x => x.id === id);
    if (selectedIndex !== -1) {
      this.selectedModules.splice(selectedIndex , 1);
    }

    const moduleIndex = this.modules.findIndex(x => x.id === id);
    if (moduleIndex !== -1) {
      this.modules[moduleIndex].checked = false;
    }
  }

  public removeSelectedTrack(id: string) {
    const selectedTrackIndex = this.selectedTracks.findIndex(x => x.id === id);
    this.selectedTracks.splice(selectedTrackIndex , 1);

    const trackIndex = this.tracks.findIndex(x => x.id === id);
    this.tracks[trackIndex].checked = false;
  }
}
