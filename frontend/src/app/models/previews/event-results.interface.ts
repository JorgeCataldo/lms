export interface EventResults {
  eventName: string;
  eventDate: Date;
  classroomContent: RatingResult;
  didactic: RatingResult;
  evaluationFormat: RatingResult;
  studyContent: RatingResult;
  theoryAndPractice: RatingResult;
  usedResources: RatingResult;
  expectation: ExpectationResult;
  studentsCount: number;
  itemsCount: number;
  suggestions: Array<Suggestions>;
  canApprove: boolean;
}

export interface RatingResult {
  bad: number;
  excelent: number;
  good: number;
  satisfactory: number;
  unsatisfactory: number;
}

export interface ExpectationResult {
  asExpected: number;
  belowExpectation: number;
  exceedExpectation: number;
}

export interface Suggestions {
  eventReactionId: string;
  suggestion: string;
  approved: boolean;
}
