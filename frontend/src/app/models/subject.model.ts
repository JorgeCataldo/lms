import { Content } from './content.model';
import { ProgressTypeEnum } from './enums/progress-type.enum';
import { Level } from './shared/level.interface';

export class SubjectRequirement {
  public level: string;
  public percentage: number;
}

export class UserProgress {
  public level: number;
  public percentage: number;
  public progress?: number;
  public progressType?: ProgressTypeEnum;

  constructor(level: Level, progressType = ProgressTypeEnum.SubjectProgress) {
    this.level = level.id;
    this.percentage = 0.7;
    this.progressType = progressType;
  }
}
export class Subject {
  public id?: string;
  public title: string;
  public excerpt: string;
  public requirements?: Array<SubjectRequirement>;
  public concepts: Array<string>;
  public contents: Array<Content>;
  public userProgresses?: Array<UserProgress>;
  public hasQuestions?: boolean;

  public duration?: number;

  constructor(levels: Array<Level> = []) {
    if (levels && levels.length > 0) {
      levels.forEach(level => {
        this.userProgresses = this.userProgresses || [];
        this.userProgresses.push(
          new UserProgress(level)
        );
      });
    }
  }
}
