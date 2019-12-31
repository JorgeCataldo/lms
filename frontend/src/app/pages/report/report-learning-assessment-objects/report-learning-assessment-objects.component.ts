import { Component, Input } from '@angular/core';
import { Requirement, RequirementProgress } from '../../../settings/modules/new-module/models/new-requirement.model';
import { Level } from '../../../models/shared/level.interface';
import { Router } from '@angular/router';
import { ExcelService } from 'src/app/shared/services/excel.service';
import { SettingsModulesService } from 'src/app/settings/_services/modules.service';
import { SettingsTracksService } from 'src/app/settings/_services/tracks.service';
import { TestTrack, TestModule } from 'src/app/models/valuation-test.interface';
import { MatSnackBar } from '@angular/material';
import { NotificationClass } from 'src/app/shared/classes/notification';
import { ModulePreview } from 'src/app/models/previews/module.interface';

@Component({
  selector: 'app-report-learning-assessment-objects',
  templateUrl: './report-learning-assessment-objects.component.html',
  styleUrls: ['./report-learning-assessment-objects.component.scss']
})
export class ReportLearningAssessmentObjectsComponent extends NotificationClass {

  @Input() requirement: Requirement;
  @Input() levels: Array<Level>;
  @Input() last: boolean = false;

  public modules: Array<TestTrack> = [];
  public selectedModules: Array<TestTrack> = [];

  constructor(
    private _router: Router,
    private _excelService: ExcelService,
    private _moduleService: SettingsModulesService,
    private _trackService: SettingsTracksService,
    protected _snackBar: MatSnackBar,
  ) {
    super(_snackBar);
  }

  public triggerModuleSearch(searchValue: string) {
    this._loadModules(searchValue);
  }

  public removeSelectedModule(id: string) {
    this._removeFromCollection(
      this.modules, this.selectedModules, id
    );
  }

  private _removeFromCollection(collection, selected, id: string): void {
    const selectedModulesIndex = selected.findIndex(x => x.id === id);
    selected.splice(selectedModulesIndex , 1);

    const moduleIndex = collection.findIndex(x => x.id === id);
    collection[moduleIndex].checked = false;
  }

  private _loadModules(searchValue: string = ''): void {
    if (searchValue === '') {
      this.modules = [];
      return;
    }

    this._trackService.getPagedFilteredTracksList(
      1, 0, searchValue
    ).subscribe(response => {
      response.data.tracks.forEach((tck: ModulePreview) => {
        tck.checked = this.selectedModules.find(t => t.id === tck.id) && true;
      });
      this.modules = response.data.tracks;
    });
  }

  public updateModules(): void {
    this.selectedModules = this._updateCollection(
      this.modules, this.selectedModules
    );
  }

  private _updateCollection(collection, selected): Array<any> {
    const prevSelected = selected.filter(x =>
      !collection.find(t => t.id === x.id)
    );
    const selectedColl = collection.filter(module =>
      module.checked
    );
    return [ ...prevSelected, ...selectedColl];
  }

   public exportBdqStatistics() {
    if (this.selectedModules === null || this.selectedModules.length === 0) {
       this.notify('Selecione uma trilha para exportar os dados.');
      return;
    } else  {
       this.notify('A exportação pode levar alguns minutos');
    }

    const selectedModuleIds = this.selectedModules.map(function(item) {
      return item.id;
    });

    this._trackService.getTrackAnswers('', selectedModuleIds.join(',')).subscribe(res => {
      const bdqStatistic = res.data;

      if (bdqStatistic) {
        const excelBdq = [];

        for (let i = 0; i < bdqStatistic.length; i++) {
          const user = bdqStatistic[i];

          excelBdq.push({
            'Id Módulo': user.moduleId,
            'Assunto Id': user.subjectId,
            'Questão Id': user.answerId,
            'Id do usuário': user.userId,
            'Nome do Módulo': user.moduleName,
            'Nome do Assunto': user.subjectName,
            'Nome do Usuário': user.userName,
            'Questões': user.question,
            'Conceitos das questões': user.questionConcepts,
            'Respostas': user.answer,
            'Respostas com Conceitos Errados': user.answerWrongConcepts,
            'Grupos Empresarias': user.businessGroup,
            'Unidades de Negócio': user.businessUnit,
            'Segmentos': user.segment,
            'Level das questões': user.questionLevel,
            'Respostas Corretas': user.correctAnswer,
            'Datas das respostas': user.subjectId,
            'Total DB Questões': user.totalDbQuestionNumber,
            'Total de Conceitos': user.totalConceptNumber,
            'Porcentagem do Level': user.levelPercent,
            'Total de Questões': user.totalQuestionNumber,
            'Limite de respostas do Módulo': user.moduleQuestionLimit,
            'Pontos Máximo': user.maxPoints,
            'Total de Respostas': user.totalAnswer,
            'Inicialização da Janela': user.initWindow,
            'Finalização da Janela': user.endWindow,
            'Total de pontos responsável': user.totalAccountablePoints,
            'Respondeu Todas As Respostas Corretamente': user.hasAnsweredAllLevelQuestionsCorrectly,
            'Total de Pontos Aplicáveis': user.totalApplicablePoints,
            'Progresso Absoluto': user.absoluteProgress,
            'Level Original': user.originalLevel,
            'Level Final': user.finalLevel
          });
        }
      this._excelService.exportAsExcelFile(excelBdq, 'EstatísticaBdq');
      }
    }, err => { this.notify('Ocorreu um erro ao exportar o relatório.'); });

  }

  public exportAtypicalMovements() {
    if (this.selectedModules === null || this.selectedModules.length === 0) {
       this.notify('Selecione uma trilha para exportar os dados.');
      return;
    } else  {
       this.notify('A exportação pode levar alguns minutos');
    }

    const selectedTrackIds = this.selectedModules.map(function(item) {
      return item.id;
    });

    this._trackService.getAtypicalMovements(selectedTrackIds.join(',')).subscribe(res => {
      console.log('res -> ', res);

      const excelAtypicalMovement = [];

      for (let index = 0; index < res.data.length; index++) {
        const answer = res.data[index];
        excelAtypicalMovement.push({
          'Módulo': answer.moduleName,
          'Usuário': answer.userName,
          // 'Pergunta': answer.question,
          // 'Resposta': answer.answer,
          'Data da Resposta Anterior': answer.formatedPreviousAnswerDate,
          'Data da Resposta': answer.formatedAnswerDate,
          'Diferença Entre Respostas': answer.answerTimeDifference,
        });
      }

      this._excelService.exportAsExcelFile(excelAtypicalMovement,
        'Movimentação-Atípica'
      );
    }, err => { this.notify('Ocorreu um erro ao exportar o relatório.'); });

  }

}
