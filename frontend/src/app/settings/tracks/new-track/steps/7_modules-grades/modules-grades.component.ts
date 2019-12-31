import { Component, Input, Output, EventEmitter, OnInit } from '@angular/core';
import { MatSnackBar } from '@angular/material';
import { NotificationClass } from '../../../../../shared/classes/notification';
import { Track } from '../../../../../models/track.model';
import { TrackModule } from '../../../../../models/track-module.model';
import { TrackEvent } from '../../../../../models/track-event.model';
import { GradeEvaluationTypeEnum } from 'src/app/models/enums/grade-evaluation-type.enum';

@Component({
  selector: 'app-new-track-modules-grades',
  templateUrl: './modules-grades.component.html',
  styleUrls: ['../new-track-steps.scss', './modules-grades.component.scss']
})
export class NewTrackModulesGradesComponent extends NotificationClass {

  public readonly displayedColumns: string[] = [
    'content', 'weightGrade1', 'grade1', 'weightGrade2', 'grade2'
  ];

  @Input() readonly track: Track;
  @Output() addModulesGradesWeight = new EventEmitter<Array<Array<any>>>();

  public modules: Array<TrackModule> = [];
  public trackModulesEvents: Array<TrackModule> = [];
  public totalWeight: number = 0;

  constructor(
    protected _snackBar: MatSnackBar
  ) {
    super(_snackBar);
  }

  public prepareStep(): void {
    if (this.track && this.track.modulesConfiguration) {
      this.modules = this.track.modulesConfiguration;
      this.modules.forEach(e => {
        e.isEvent = false;
        e.weight = e.weight ? e.weight : 0;
        e.weightGrade1 = e.bdqWeight ? e.bdqWeight : 0;
        e.grade1 = e.bdqWeight ? GradeEvaluationTypeEnum.BDQ : 0;
        e.weightGrade2 = e.evaluationWeight ? e.evaluationWeight : 0;
        e.grade2 = e.evaluationWeight ? GradeEvaluationTypeEnum.Evaluation : 0;
      });
    }

    this.trackModulesEvents = this.getTrackCards();
  }

  public getTrackCards(): Array<TrackModule> {
    let modulesEvents = [... this.modules];
    modulesEvents = modulesEvents.sort((a, b) => {
      return a.order - b.order;
    });
    return modulesEvents;
  }

  public nextStep(): void {
    this.modules.forEach(m => {
      m.bdqWeight = m.grade1 === GradeEvaluationTypeEnum.BDQ ? m.weightGrade1 : m.weightGrade2;
      m.evaluationWeight = m.grade1 === GradeEvaluationTypeEnum.Evaluation ? m.weightGrade1 : m.weightGrade2;
    });

    this.addModulesGradesWeight.emit( [ this.modules ] );
  }

  public setGradeValue(value: number, grade: number, row: TrackModule): void {

    if (grade === GradeEvaluationTypeEnum.BDQ) {
      row.grade1 = value;
    } else {
      row.grade2 = value;
    }
  }

  public setWeightGradeValue(value: number, grade: number, row: TrackModule): void {

    if (grade === GradeEvaluationTypeEnum.BDQ) {
      row.weightGrade1 = value;
    } else {
      row.weightGrade2 = value;
    }
  }

}
