export class EventValuation {
  public title: string;
  public values: EventValuationDescription[];
}

export class EventValuationDescription {
  public title: string;
  public description: string;
  public selected: boolean;
  public value: number;
}

export class EventReaction {
  public eventId: string;
  public eventScheduleId: string;
  public didactic: number;
  public classroomContent: number;
  public studyContent: number;
  public theoryAndPractice: number;
  public usedResources: number;
  public evaluationFormat: number;
  public expectation: number;
  public suggestions: string;
}
