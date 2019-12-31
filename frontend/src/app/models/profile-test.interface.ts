export class ProfileTest {
  public id?: string;
  public title: string;
  public questions?: Array<string>;
  public testQuestions?: Array<ProfileTestQuestion>;
  public isDefault?: boolean;

  public checked?: boolean;

  constructor() {
    this.testQuestions = [];
  }
}

export class ProfileTestQuestion {
  public id?: string;
  public testId: string;
  public title: string;
  public percentage: number;
  public type: ProfileTestTypeEnum;
  public options: Array<ProfileTestQuestionOption>;
  public testTitle?: string;
  public answer?: string;

  public newQuestion?: boolean;

  constructor() {
    this.newQuestion = true;
  }
}

export class ProfileTestQuestionOption {
  public text: string;
  public correct: boolean;
}

export interface ProfileTestResponse {
  id: string;
  createdAt: Date;
  createdBy?: string;
  userName: string;
  userRegisterId?: string;
  testId?: string;
  testTitle?: string;
  answers: Array<ProfileTestAnswer>;
  recommended?: boolean;
  eventsInfo?;
  tracksInfo?;
  modulesInfo?;

  finalGrade?: number;
}

export interface ProfileTestAnswer {
  questionId: string;
  question?: string;
  answer: string;
  grade?: number;
  percentage?: number;

  gradeIsSet?: boolean;
}

export enum ProfileTestTypeEnum {
  MultipleChoice = 1,
  Discursive = 2
}

export interface ProfileTestExcel {
  aluno: string;
  matricula: string;
  questao: string;
  answer: string;
  data: Date;
}
