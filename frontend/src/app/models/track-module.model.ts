export class TrackModule {
  public title: string;
  public moduleId: string;
  public level: number;
  public percentage: number;
  public order: number;
  public studentPercentage?: number;
  public studentFinished?: boolean;
  public studentLevel?: number;
  public classLevel?: number;
  public weight: number;
  public cutOffDate?: Date;

  public hovered: boolean = false;
  public isEvent?: boolean = false;
  public completeStudents?: number;
  public students: any[];
  public weightGrade1: number;
  public grade1: number;
  public weightGrade2: number;
  public grade2: number;
  public bdqWeight: number;
  public evaluationWeight: number;

  public alwaysAvailable?: boolean;
  public openDate?: Date;
  public valuationDate?: Date;
  public blocked?: boolean;


  constructor(order: number = 0, title: string = '', moduleId: string = null, level: number = 1,
    percentage: number = 10) {
      this.order = order;
      this.title = title;
      this.moduleId = moduleId;
      this.level = level;
      this.percentage = percentage;
      this.isEvent = false;
  }
}
