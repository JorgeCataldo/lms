import { Component, Input } from '@angular/core';
import { Requirement, RequirementProgress } from '../../../settings/modules/new-module/models/new-requirement.model';
import { Level } from '../../../models/shared/level.interface';
import { Router } from '@angular/router';
import { ExcelService } from 'src/app/shared/services/excel.service';
import { SettingsModulesService } from 'src/app/settings/_services/modules.service';
import { SettingsTracksService } from 'src/app/settings/_services/tracks.service';
import { TestTrack } from 'src/app/models/valuation-test.interface';
import { TrackPreview } from 'src/app/models/previews/track.interface';
import { MatSnackBar } from '@angular/material';
import { NotificationClass } from 'src/app/shared/classes/notification';
import { ReportsService } from 'src/app/settings/_services/reports.service';

@Component({
  selector: 'app-report-program-execution',
  templateUrl: './report-program-execution.component.html',
  styleUrls: ['./report-program-execution.component.scss']
})
export class ReportProgramExecutionComponent extends NotificationClass {

  @Input() requirement: Requirement;
  @Input() levels: Array<Level>;
  @Input() last: boolean = false;

  public tracks: Array<TestTrack> = [];
  public selectedTracks: Array<TestTrack> = [];

  constructor(
    private _router: Router,
    private _excelService: ExcelService,
    private _reportsService: ReportsService,
    private _modulesService: SettingsModulesService,
    private _tracksService: SettingsTracksService,
    private _reportService: ReportsService,
    protected _snackBar: MatSnackBar,
  ) {
    super(_snackBar);
   }

  public checkTrackSelectedEffectiveness() {
    if (this.selectedTracks === null || this.selectedTracks.length === 0) {
      this.notify('Selecione uma trilha para exportar os dados.');
      return;
    } else {
      this.notify('A exportação pode levar alguns minutos');
    }

    this.exportEffectiveness();
  }

  public exportEffectiveness() {
    const selectedTrackIds = this.selectedTracks.map(function(item) {
      return item.id;
    });

    this._modulesService.getEffectivenessIndicators(selectedTrackIds.join(',')).subscribe(res => {
      console.log('res -> ', res);
      this._excelService.exportAsExcelFile(
        this._excelService.buildExportUsersEffectiveness(res),
        'Ranking-Efetividade'
      );
    }, err => { console.log(err) ; });
  }

  public triggerTrackSearch(searchValue: string) {
    this._loadTracks(searchValue);
  }

  public removeSelectedTrack(id: string) {
    this._removeFromCollection(
      this.tracks, this.selectedTracks, id
    );
  }

  private _removeFromCollection(collection, selected, id: string): void {
    const selectedTrackIndex = selected.findIndex(x => x.id === id);
    selected.splice(selectedTrackIndex , 1);

    const trackIndex = collection.findIndex(x => x.id === id);
    collection[trackIndex].checked = false;
  }

  private _loadTracks(searchValue: string = ''): void {
    if (searchValue === '') {
      this.tracks = [];
      return;
    }

    this._tracksService.getPagedFilteredTracksList(
      1, 20, searchValue
    ).subscribe(response => {
      response.data.tracks.forEach((tck: TrackPreview) => {
        tck.checked = this.selectedTracks.find(t => t.id === tck.id) && true;
      });
      this.tracks = response.data.tracks;
    });
  }

  public updateTracks(): void {
    this.selectedTracks = this._updateCollection(
      this.tracks, this.selectedTracks
    );
  }

  private _updateCollection(collection, selected): Array<any> {
    const prevSelected = selected.filter(x =>
      !collection.find(t => t.id === x.id)
    );
    const selectedColl = collection.filter(track =>
      track.checked
    );
    return [ ...prevSelected, ...selectedColl];
  }

  public exportGrades() {

    if (this.selectedTracks === null || this.selectedTracks.length === 0) {
      this.notify('Selecione uma trilha para exportar os dados.');
      return;
    } else {
      this.notify('A exportação pode levar alguns minutos');
    }

    const selectedTrackIds = this.selectedTracks.map(function(item) {
      return item.id;
    });

    this._reportsService.getTracksGrades(selectedTrackIds.join(',')).subscribe(res => {
      console.log('getTracksGrades -> ', res.data);
      const exportArray: any[] = [];
      const moduleArray: any[] = [];
      const eventArray: any[] = [];
      const eventPresenceArray: any[] = [];
      for (let i = 0; i < res.data.length; i++) {
        exportArray.push([]);
        exportArray[i]['Alunos'] = res.data[i].name;
      }
      for (let i = 0; i < res.data.length; i++) {
        exportArray.push([]);
        exportArray[i]['CPF'] = res.data[i].cpf;
      }
      for (let i = 0; i < res.data.length; i++) {
        exportArray.push([]);
        exportArray[i]['Trilha'] = res.data[i].track;
      }
      for (let i = 0; i < res.data.length; i++) {
        exportArray.push([]);
        exportArray[i]['Total de Pontos'] = res.data[i].totalPoints;
      }
      for (let i = 0; i < res.data.length; i++) {
        exportArray.push([]);
        exportArray[i]['Nota Total'] = res.data[i].totalGrades;
      }
      for (let i = 0; i < res.data.length; i++) {
        exportArray.push([]);
        exportArray[i]['Ranking'] = res.data[i].ranking;
      }
      for (let i = 0; i < res.data.length; i++) {
        for (let j = 0; j < res.data[i].moduleGrade.length; j++) {
          if (!moduleArray.includes(res.data[i].moduleGrade[j].moduleName)) {
            moduleArray.push('Módulo: ' + res.data[i].moduleGrade[j].moduleName);
            moduleArray.push('Módulo-Peso: ' + res.data[i].moduleGrade[j].moduleName);
          }
        }
        for (let k = 0; k < res.data[i].eventGrade.length; k++) {
          if (!eventArray.includes(res.data[i].eventGrade[k].eventName)) {
            eventArray.push('Evento: ' + res.data[i].eventGrade[k].eventName);
            eventArray.push('Evento-Peso: ' + res.data[i].eventGrade[k].eventName);
          }
        }
        for (let l = 0; l < res.data[i].eventPresence.length; l++) {
          if (!eventPresenceArray.includes(res.data[i].eventPresence[l].eventName)) {
            eventPresenceArray.push(res.data[i].eventPresence[l].eventName);
          }
        }
      }
      for (let i = 0; i < res.data.length; i++) {
        for (let j = 0; j < moduleArray.length; j++) {
          exportArray[i][moduleArray[j]] = '';
        }
        for (let j = 0; j < res.data[i].moduleGrade.length; j++) {
          exportArray[i]['Módulo: ' + res.data[i].moduleGrade[j].moduleName] = res.data[i].moduleGrade[j].grade;
          exportArray[i]['Módulo-Peso: ' + res.data[i].moduleGrade[j].moduleName] = res.data[i].moduleGrade[j].weightGrade;
        }
      }
      for (let i = 0; i < res.data.length; i++) {
        for (let k = 0; k < eventArray.length; k++) {
          exportArray[i][eventArray[k]] = '';
        }
        for (let k = 0; k < res.data[i].eventGrade.length; k++) {
          exportArray[i]['Evento: ' + res.data[i].eventGrade[k].eventName] = res.data[i].eventGrade[k].finalGrade;
          exportArray[i]['Evento-Peso: ' + res.data[i].eventGrade[k].eventName] = res.data[i].eventGrade[k].weightGrade;
        }
      }
      for (let i = 0; i < res.data.length; i++) {
        for (let l = 0; l < eventPresenceArray.length; l++) {
          exportArray[i][eventPresenceArray[l]] = '';
        }
        for (let l = 0; l < res.data[i].eventPresence.length; l++) {
          exportArray[i][res.data[i].eventPresence[l].eventName] = res.data[i].eventPresence[l].userPresence;
        }
      }
      this._excelService.exportAsExcelFile(exportArray, 'Ranking-Notas');
    }, err => { console.log(err); });
  }

  public getUserProgresses() {

    if (this.selectedTracks === null || this.selectedTracks.length === 0) {
      this.notify('Selecione uma trilha para exportar os dados.');
      return;
    }

    const selectedTrackIds = this.selectedTracks.map(function(item) {
      return item.id;
    });

    this._reportService.getUserProgresses(selectedTrackIds.join(',')).subscribe(res => {
      res.data.forEach(trackReport => {
        this._excelService.exportAsExcelFile(
          this._excelService.buildExportUserProgressReport(trackReport),
          'RelatorioDeProgresso'
        );
      });
    }, err => { this.notify(this.getErrorNotification(err)); });
  }

}
