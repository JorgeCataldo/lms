import { Component, Input } from '@angular/core';
import { UserSkill, UserEventApplicationItem } from 'src/app/models/user-recommendation.model';
import { Level } from 'src/app/models/shared/level.interface';


@Component({
  selector: 'app-proseek-recommendation',
  templateUrl: './proseek-recommendation.component.html',
  styleUrls: ['./proseek-recommendation.component.scss']
})
export class SettingsProseekRecommendationComponent {
  @Input() userSkills: UserSkill[] = [];
  @Input() userName: string = '';
  @Input() levels: Level[] = [];
  @Input() userId: string = '';
  @Input() baseValues: UserEventApplicationItem[] = [];

  public perfilInfoUnd: boolean = false;
  public perfilInfoDef: boolean = false;

  public qcPerfil: boolean = true;
  public tgPerfil: boolean = true;
  public faPerfil: boolean = true;

  public qcPerfilInfo: boolean = false;
  public tgPerfilInfo: boolean = false;
  public faPerfilInfo: boolean = false;

  public getRadarLabels(userSkill: UserSkill): Array<number> {
    return userSkill.modulesConfiguration.map((m, index) => index + 1);
  }

  public getRadarDataset(userSkill: UserSkill) {
    const modules = userSkill.modulesConfiguration;
    const lemap = modules.map(m => m.studentLevel);
    return [{
      label: 'MÉDIA TURMA',
      data: modules.map(m => m.classLevel),
      backgroundColor: 'rgba(255, 166, 62, 0.35)',
      borderColor: 'transparent',
      pointBackgroundColor: 'rgba(255, 166, 62, 0.35)',
      pointRadius: 8
    }, {
      label: this.userName,
      data: lemap,
      backgroundColor: 'rgba(137, 210, 220, 0.35)',
      borderColor: 'rgb(137, 210, 220)',
      pointBackgroundColor: 'rgb(137, 210, 220)',
      pointRadius: 12
    }];
  }

  public getRadarBarLabels(): Array<string> {
    return ['QC', 'TG', 'FA'];
  }

  public getRadarBarDataset(userEventApplicationItem: UserEventApplicationItem) {
    return [{
      label: 'Nota',
      data: [
        userEventApplicationItem.qcGrade,
        userEventApplicationItem.tgGrade,
        userEventApplicationItem.faGrade
      ],
      borderWidth: 1,
      backgroundColor: '#23BCD1'
    }];
  }

  public getRadarProfileDataset() {
    return [{
      label: 'Nota',
      data: [
        this.getQcGradeNormal(),
        this.getTgGradeNormal(),
        this.getFaGradeNormal()
      ],
      borderWidth: 1,
      backgroundColor: '#23BCD1'
    }];
  }

  public getRadarTitleCallback(tooltipItem, data): string {
    const moduleIndex = data.labels[tooltipItem[0].index] - 1;
    const modules = [].concat.apply([], this.userSkills.map(x => x.modulesConfiguration));
    return modules.find(
      (m, index) => index === moduleIndex
    ).title;
  }

  public getRadarTooltipCallback(tooltipItem, data): string {
    const level = data.datasets[tooltipItem.datasetIndex].data[tooltipItem.index];
    return 'Level ' + level.toFixed(1) + this.getLevelDescription(
      Math.round(level - 1)
    );
  }

  public getProfileRadarTitleCallback(tooltipItem, data): string {
    const titles: string[] = ['Qualidade da Contribuição', 'Trabalho em Grupo', 'Fluência Argumentativa'];
    return titles[tooltipItem[0].index];
  }

  public getProfileRadarTooltipCallback(tooltipItem, data): string {
    const grade = data.datasets[0].data[tooltipItem.index];
    let level = 0;
    if (grade >= 9) {
      level = 2;
    } else if (grade < 9 && grade >= 7) {
      level = 1;
    } else {
      level = 0;
    }
    return 'Nível ' + level;
  }

  public getLevelDescription(level: number): string {
    if (level < 0 || !this.levels) return '';

    const selectedLevel = this.levels.find(l => l.id === level);
    return ' (' + selectedLevel.description + ')';
  }

  public getQcGradeNormal(): number {
    console.log('this.baseValues -> ', this.baseValues);
    if (this.baseValues != null && this.baseValues.length > 0) {
      let returnValue = 0;
      this.baseValues.forEach(baseValue => {
        returnValue += baseValue.qcGrade;
      });
      // returnValue = returnValue / this.baseValues.length;
      console.log('getQcGradeNormal - returnValue -> ', returnValue);
      return returnValue ;
    }
    return 0;
  }

  public getTgGradeNormal(): number {
    if (this.baseValues != null && this.baseValues.length > 0) {
      let returnValue = 0;
      this.baseValues.forEach(baseValue => {
        returnValue += baseValue.tgGrade;
      });
      // returnValue = returnValue / this.baseValues.length;
      console.log('getTgGradeNormal - returnValue -> ', returnValue);
      return returnValue ;
    }
    return 0;
  }

  public getFaGradeNormal(): number {
    if (this.baseValues != null && this.baseValues.length > 0) {
      let returnValue = 0;
      this.baseValues.forEach(baseValue => {
        returnValue += baseValue.faGrade;
      });
      // returnValue = returnValue / this.baseValues.length;
      console.log('getFaGradeNormal - returnValue -> ', returnValue);
      return returnValue ;
    }
    return 0;
  }
}
