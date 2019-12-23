export class TrackEvent {
  public eventId: string;
  public eventScheduleId: string;
  public eventDate?: Date;
  public duration?: number | string;
  public startHour?: string;
  public title: string;
  public order?: number;
  public studentFinished?: boolean;
  public incompleteRequirementStudents?: Array<string>;
  public hasTakenPart?: boolean;
  public weight: number;

  public hovered?: boolean = false;
  public isEvent?: boolean = true;
  public trackId?: string;

  public alwaysAvailable?: boolean;
  public openDate?: Date;
  public valuationDate?: Date;
  public applications: any[];
  public keys: string[];
  public blocked?: boolean;
  public cutOffDate?: Date;

  constructor(order: number = 0, title: string = '') {
    this.order = order;
    this.title = title;
    this.isEvent = true;
  }
}
