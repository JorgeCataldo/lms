import { UserPreview } from './previews/user.interface';
import { ApplicationStatus } from './enums/application-status';

export class EventApplication {
  public id?: string;
  public userId: string;
  public eventId: string;
  public scheduleId: string;
  public user: UserPreview;
  public answers: Array<string>;
  public approved?: boolean;
  public applicationStatus: ApplicationStatus;
  public requestDate: Date;
  public resolutionDate: Date;
  public prepQuiz?: any;
  public prepQuizAnswers?: Array<string>;
  public prepQuizAnswersList?: Array<PrepQuizAnswer>;
  public grade: string;
  public filledEventReaction?: boolean;
  public finalGrade?: number;
  public userPresence?: boolean;
  public eventRequirement?: boolean;
  public organicGrade?: number;
  public inorganicGrade?: number;
  public forumGrade?: number;
  public gradeBaseValues: BaseValue[];
  public customEventGradeValues: CustomEventGradeValue[];
  public transcribedParticipation: string;

  // Front variables
  public grading: boolean = false;
  public forumGrading: boolean = false;
}

export interface BaseValue {
    key: string;
    value: string;
}

export interface CustomEventGradeValue {
    key: string;
    grade: number;
}

export interface PrepQuizAnswer {
  answer: string;
  fileAsAnswer: boolean;
}
