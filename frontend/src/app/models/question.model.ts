import { Level } from './shared/level.interface';

export class Question {
  public id?: string;
  public subjectId?: string;
  public moduleId?: string;
  public text: string;
  public answers: Array<Answer>;
  public concepts: Array<string>;
  public level: number;
  public duration: number;
  public questionId?: string;

  public index?: number;

  constructor(subjectId: string, concepts: Array<string> = [], index: number = null) {
    this.subjectId = subjectId;
    this.answers = [];
    this.concepts = concepts;
    this.index = index;
  }
}

export class ConceptAnswer {
  public concept: string;
  public isRight: boolean;

  constructor(concept: string) {
    this.concept = concept;
    this.isRight = false;
  }
}

export class Answer {
  public id: string;
  public questionId?: string;
  public description: string = '';
  public concepts: Array<ConceptAnswer>;
  public isRight: boolean;
  public points: number;

  constructor(questionId: string, concepts: Array<string>) {
    this.questionId = questionId;
    this.concepts = concepts.map(concept => new ConceptAnswer(concept));
  }
}

export class InvalidSubjectItem {
  public subjectId: string;
  public subjectTitle?: string;
  public missingLevels: Array<Level>;
}

export class QuestionExcel {
  public subjectId: string;
  public subjectTitle: string;
  public level: number;
  public text: string;
  public duration: number;
  public answer1: string;
  public answer1Points: number;
  public answer1Concepts: string;
  public answer2: string;
  public answer2Points: number;
  public answer2Concepts: string;
  public answer3: string;
  public answer3Points: number;
  public answer3Concepts: string;
  public answer4: string;
  public answer4Points: number;
  public answer4Concepts: string;
  public answer5: string;
  public answer5Points: number;
  public answer5Concepts: string;
}

export class UpdatedQuestionExcel {
  public subjectId: string;
  public subjectTitle: string;
  public oldLevel: number;
  public newLevel: number;
  public oldText: string;
  public newText: string;
  public oldDuration: number;
  public newDuration: number;
  public oldAnswer1: string;
  public newAnswer1: string;
  public oldAnswer1Points: number;
  public newAnswer1Points: number;
  public oldAnswer1Concepts: string;
  public newAnswer1Concepts: string;
  public oldAnswer2: string;
  public newAnswer2: string;
  public oldAnswer2Points: number;
  public newAnswer2Points: number;
  public oldAnswer2Concepts: string;
  public newAnswer2Concepts: string;
  public oldAnswer3: string;
  public newAnswer3: string;
  public oldAnswer3Points: number;
  public newAnswer3Points: number;
  public oldAnswer3Concepts: string;
  public newAnswer3Concepts: string;
  public oldAnswer4: string;
  public newAnswer4: string;
  public oldAnswer4Points: number;
  public newAnswer4Points: number;
  public oldAnswer4Concepts: string;
  public newAnswer4Concepts: string;
  public oldAnswer5: string;
  public newAnswer5: string;
  public oldAnswer5Points: number;
  public newAnswer5Points: number;
  public oldAnswer5Concepts: string;
  public newAnswer5Concepts: string;
}
