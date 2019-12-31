import * as FileSaver from 'file-saver';
import * as XLSX from 'xlsx';
import { Injectable } from '@angular/core';
import { ProfileTestQuestion } from 'src/app/models/profile-test.interface';
import { ValuationTestQuestion } from 'src/app/models/valuation-test.interface';

const EXCEL_TYPE = 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet;charset=UTF-8';
const EXCEL_EXTENSION = '.xlsx';

@Injectable()
export class ExcelService {
  constructor() { }

  public exportAsExcelFile(json: any[], excelFileName: string): void {
    const worksheet: XLSX.WorkSheet = XLSX.utils.json_to_sheet(json);
    const workbook: XLSX.WorkBook = { Sheets: { 'data': worksheet }, SheetNames: ['data'] };
    const excelBuffer: any = XLSX.write(workbook, { bookType: 'xlsx', type: 'array' });
    this.saveAsExcelFile(excelBuffer, excelFileName);
  }

  public getExcelContentAsJson(excelFile: any): Array<ProfileTestQuestion | ValuationTestQuestion> {
    const workbook = XLSX.read(excelFile, { type: 'binary' });
    return XLSX.utils.sheet_to_json(
      workbook.Sheets[
        workbook.SheetNames[0]
      ], {
        raw: true,
        header: ['title', 'type', 'options', 'percentage']
      }
    );
  }

  private saveAsExcelFile(buffer: any, fileName: string): void {
    const data: Blob = new Blob([buffer], { type: EXCEL_TYPE });
    FileSaver.saveAs(data, fileName + new Date().getTime() + EXCEL_EXTENSION);
  }

  public buildExportUsersGrade(res: any): any[] {
    const exportArray: any[] = [];
    const moduleArray: any[] = [];
    const eventArray: any[] = [];
    const eventPresenceArray: any[] = [];
    for (let i = 0; i < res.data.length; i++) {
      exportArray.push([]);
      exportArray[i]['Alunos'] = res.data[i].name;
      exportArray[i]['Emails'] = res.data[i].email;
    }
    for (let i = 0; i < res.data.length; i++) {
      for (let j = 0; j < res.data[i].moduleGrade.length; j++) {
        if (!moduleArray.includes(res.data[i].moduleGrade[j].moduleName))
          moduleArray.push(res.data[i].moduleGrade[j].moduleName);
      }
      for (let k = 0; k < res.data[i].eventGrade.length; k++) {
        if (!eventArray.includes(res.data[i].eventGrade[k].eventName))
          eventArray.push(res.data[i].eventGrade[k].eventName);
      }
      for (let l = 0; l < res.data[i].eventPresence.length; l++) {
        if (!eventPresenceArray.includes(res.data[i].eventPresence[l].eventName))
          eventPresenceArray.push(res.data[i].eventPresence[l].eventName);
      }
    }
    for (let i = 0; i < res.data.length; i++) {
      for (let j = 0; j < moduleArray.length; j++) {
        exportArray[i][moduleArray[j]] = '';
      }
      for (let j = 0; j < res.data[i].moduleGrade.length; j++) {
        exportArray[i][res.data[i].moduleGrade[j].moduleName] = res.data[i].moduleGrade[j].points;
      }
    }
    for (let i = 0; i < res.data.length; i++) {
      for (let k = 0; k < eventArray.length; k++) {
        exportArray[i][eventArray[k]] = '';
      }
      for (let k = 0; k < res.data[i].eventGrade.length; k++) {
        exportArray[i][res.data[i].eventGrade[k].eventName] = res.data[i].eventGrade[k].finalGrade;
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
    return exportArray;
  }

  public buildExportUsersEffectiveness(res: any): any[] {
    const exportArray: any[] = [];
    for (let i = 0; i < res.data.length; i++) {
      exportArray.push([]);
      exportArray[i]['Aluno'] = res.data[i].userName;
    }
    for (let i = 0; i < res.data.length; i++) {
      for (let j = 0; j < res.data[i].modulesInfos.length; j++) {
        const currentModule =  res.data[i].modulesInfos[j];
        const moduleNumbe = (j + 1).toString();
        exportArray[i]['Módulo ' + moduleNumbe] = currentModule.moduleName;
        exportArray[i]['BDQ Certas ' + moduleNumbe] = currentModule.bdqPerformances.correctQuestions;
        exportArray[i]['BDQ Respondidas ' + moduleNumbe] = currentModule.bdqPerformances.answeredQuestions;
        exportArray[i]['BDQ Efetividade ' + moduleNumbe + '%'] = currentModule.bdqPerformances.effectiveness;
        exportArray[i]['Consumo Vídeo Início ' + moduleNumbe] = currentModule.videoConsummation.started;
        exportArray[i]['Consumo Vídeo Fim ' + moduleNumbe] = currentModule.videoConsummation.finished;
        exportArray[i]['Conceitos Obtidos ' + moduleNumbe] = currentModule.conceptPerformances.acquiredConcepts;
        exportArray[i]['Conceitos Módulo ' + moduleNumbe] = currentModule.conceptPerformances.moduleConcepts;
        exportArray[i]['Conceitos Cobertura ' + moduleNumbe + '%'] = currentModule.conceptPerformances.effectiveness;
      }
    }
    return exportArray;
  }

  public buildExportUserProgressReport(res: any): any[] {
    const exportArray: any[] = [];
    for (let i = 0; i < res.length; i++) {
      exportArray.push([]);
      exportArray[i]['Aluno'] = res[i].name;
      exportArray[i]['Dias apos matrícula'] = res[i].fromStartDays;
      exportArray[i]['Prazo restante'] = res[i].remainingDays;
      exportArray[i]['Progresso'] = res[i].trackProgress;
      exportArray[i]['Progresso esperado'] = res[i].trackExpectedProgress;
      exportArray[i]['Desvio padrão'] = res[i].standardDeviation;
      for (let j = 0; j < res[i].eventDetailItems.length; j++) {
        const currentEventDetailItem =  res[i].eventDetailItems[j];
        exportArray[i]['Evento' + j] = currentEventDetailItem.name;
        exportArray[i]['Evento Prazo restante' + j] = currentEventDetailItem.remainingDays;
      }
      for (let j = 0; j < res[i].moduleDetailItems.length; j++) {
        const currentModuleDetailItem =  res[i].moduleDetailItems[j];
        exportArray[i]['Módulo' + j] = currentModuleDetailItem.name;
        exportArray[i]['Módulo Prazo restante' + j] = currentModuleDetailItem.remainingDays;
        exportArray[i]['Dias apos 1º interação módulo' + j] = currentModuleDetailItem.firstInteractionDays;
        exportArray[i]['Dias apos 1º interação bdq' + j] = currentModuleDetailItem.bdqFirstInteractionDays;
      }
    }
    return exportArray;
  }

  public buildExportNpsReport(res: any): any[] {
    const exportArray: any[] = [];
      for (let i = 0; i < res.length; i++) {
        exportArray.push([]);
        exportArray[i]['Name'] = res[i].name;
        exportArray[i]['CPF'] = res[i].cpf;
        exportArray[i]['E-mail'] = res[i].email;
        exportArray[i]['Data'] = res[i].date;
        if (res[i].tracksInfo != null) {
          for (let j = 0; j < res[i].tracksInfo.length; j++) {
            const currentTrackInfoDetail =  res[i].tracksInfo[j];
            exportArray[i]['Informação das trilhas'] = currentTrackInfoDetail.name;
          }
        } else {
          exportArray[i]['Informação das trilhas'] = '';
        }
        if (res[i].modulesInfo != null) {
          for (let j = 0; j < res[i].modulesInfo.length; j++) {
            const currentModuleInfoDetail =  res[i].modulesInfo[j];
            exportArray[i]['Informação dos Módulos'] = currentModuleInfoDetail.name;
          }
        } else {
          exportArray[i]['Informação dos Módulos'] = '';
        }
        if (res[i].eventsInfo != null) {
          for (let j = 0; j < res[i].eventsInfo.length; j++) {
            const currentEventsInfoDetail =  res[i].eventsInfo[j];
            exportArray[i]['Informação dos Eventos'] = currentEventsInfoDetail.name;
          }
        } else {
          exportArray[i]['Informação dos Eventos'] = '';
        }
      }
      return exportArray;
  }

  public flattenObject(obj): any {
    const toReturn = {};

    for (const i in obj) {
        if (!obj.hasOwnProperty(i)) continue;

        if ((typeof obj[i]) === 'object' && obj[i] !== null) {
            const flatObject = this.flattenObject(obj[i]);
            for (const x in flatObject) {
                if (!flatObject.hasOwnProperty(x)) continue;

                toReturn[i + '.' + x] = flatObject[x];
            }
        } else {
            toReturn[i] = obj[i];
        }
    }
    return toReturn;
  }

  public flattenObjectOwnProperty(obj): any {
    const toReturn = {};

    for (const i in obj) {
        if (!obj.hasOwnProperty(i)) continue;

        if ((typeof obj[i]) === 'object' && obj[i] !== null) {
            const flatObject = this.flattenObject(obj[i]);
            for (const x in flatObject) {
                if (!flatObject.hasOwnProperty(x)) continue;

                toReturn[x] = flatObject[x];
            }
        } else {
            toReturn[i] = obj[i];
        }
    }
    return toReturn;
  }
}
