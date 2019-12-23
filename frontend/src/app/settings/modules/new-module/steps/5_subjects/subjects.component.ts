import { Component, EventEmitter, Output, Input, OnInit } from '@angular/core';
import { NotificationClass } from '../../../../../shared/classes/notification';
import { MatSnackBar } from '@angular/material';
import { Subject as SubjectModel, UserProgress } from '../../../../../models/subject.model';
import { Module } from '../../../../../models/module.model';
import { Level } from '../../../../../models/shared/level.interface';

@Component({
  selector: 'app-new-module-subjects',
  templateUrl: './subjects.component.html',
  styleUrls: ['../new-module-steps.scss', './subjects.component.scss']
})
export class NewModuleSubjectsComponent extends NotificationClass implements OnInit {

  @Input() readonly module: Module;
  @Input() set setLevels (levels: Array<Level>) {
    this.levels = levels;
    this.setUserProgess();
  }
  @Output() addSubjects = new EventEmitter<Array<SubjectModel>>();

  public levels: Array<Level>;
  public newSubject: SubjectModel;

  constructor(protected _snackBar: MatSnackBar) {
    super(_snackBar);
  }

  ngOnInit() {
    this.setUserProgess();
  }

  public getLevelDescription(levelId: number): string {
    if (!this.levels) return '';
    const level = this.levels.find(lev => lev.id === levelId);
    return level ? level.description : '';
  }

  public setProgressPercentage(event, progress: UserProgress) {
    if ( event.target.valueAsNumber === 0 || !event) {
      this.notify('Os valores devem ser diferentes de 0');
      event.target.valueAsNumber = 70;
     }
     progress.percentage = event.target.valueAsNumber / 100;
  }

  public addSubject(): void {
    if (!this.module.subjects)
      this.module.subjects = [];

    this.module.subjects.push(
      new SubjectModel( this.levels )
    );
  }

  public removeSubject(index: number): void {
    this.module.subjects.splice(index, 1);
  }

  public updateConcepts(concepts: Array<string>, subject: SubjectModel) {
    subject.concepts = concepts;
  }

  public nextStep(): void {
    this._checkSubjectsInfo( this.module.subjects ) ?
      this.addSubjects.emit( this.module.subjects ) :
      this.notify('Por favor, preencha todos os campos obrigatórios');
  }

  public setUserProgess() {
    if (this.module && this.module.subjects && this.levels) {
      this.module.subjects.forEach(subj => {
        subj.userProgresses = subj.userProgresses || [];

        this.levels.forEach(level => {
          if (!subj.userProgresses.some(up => up.level === level.id)) {
            subj.userProgresses.push(
              new UserProgress( level )
            );
          }
        });
      });
    }
  }

  private _checkSubjectsInfo(subjects?: Array<SubjectModel>): boolean {
    if (!subjects) return true;
    return !subjects.some(subj => !subj || !subj.title || !subj.excerpt);
  }
}
