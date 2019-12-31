import { ValuationTestTypeEnum, ValuationTestModuleTypeEnum } from './enums/valuation-test-type-enum';

export class ValuationTest {
  public id?: string;
  public type: ValuationTestTypeEnum;
  public title: string;
  public moduleIds?: string[];
  public trackIds?: string[];
  public testModules: TestModule[];
  public testTracks: TestTrack[];
  public questions?: string[];
  public testQuestions?: ValuationTestQuestion[];

  public checked?: boolean;

  constructor() {
    this.testQuestions = [];
    this.testModules = [];
  }
}

export class ValuationTestQuestion {
  public id?: string;
  public testId: string;
  public title: string;
  public percentage: number;
  public type: ValuationTestQuestionTypeEnum;
  public options: ValuationTestQuestionOption[];
  public testTitle?: string;
  public answer?: string;

  public newQuestion?: boolean;

  constructor() {
    this.newQuestion = true;
  }
}

export class ValuationTestQuestionOption {
  public text: string;
  public correct: boolean;
}

export interface ValuationTestResponse {
  id: string;
  createdAt: Date;
  createdBy?: string;
  userName: string;
  userRegisterId?: string;
  testId?: string;
  testTitle?: string;
  answers: ValuationTestAnswer[];
  recommended?: boolean;
  eventsInfo?;
  tracksInfo?;
  modulesInfo?;

  finalGrade?: number;
}

export interface ValuationTestAnswer {
  questionId: string;
  question?: string;
  answer: string;
  grade?: number;
  percentage?: number;

  gradeIsSet?: boolean;
}

export enum ValuationTestQuestionTypeEnum {
  MultipleChoice = 1,
  Discursive = 2
}

export interface ValuationTestExcel {
  aluno: string;
  matricula: string;
  questao: string;
  answer: string;
  data: Date;
}

export interface TestModule {
  id: string;
  title: string;
  checked?: boolean;
  toggled?: boolean;
  percent?: number;
  type?: ValuationTestModuleTypeEnum;
}

export interface TestTrack {
  id: string;
  title: string;
  checked?: boolean;
  toggled?: boolean;
  percent?: number;
  order?: number;
}
