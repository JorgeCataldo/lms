export class Formula {
  public id?: string;
  public createdAt?: Date;
  public title: string;
  public formulaParts: Array<FormulaPart>;
  public type: FormulaType;

  constructor() {
    this.formulaParts = [];
  }
}

export class FormulaPart {
  public order: number;
  public operator?: FormulaOperator;
  public key?: string;
  public integralNumber?: number;

  constructor(
    order: number, operator: FormulaOperator = null,
    key: string = null, integralNumber: number = null
  ) {
    this.order = order;
    this.operator = operator;
    this.key = key;
    this.integralNumber = integralNumber;
  }
}

export interface FormulaVariables {
  type: FormulaType;
  variables: Array<string>;
}

export enum FormulaOperator {
  Plus = 1,
  Minus = 2,
  Times = 3,
  Divided = 4,
  OpenParenthesis = 5,
  CloseParenthesis = 6
}

export enum FormulaType {
  EventsParticipation = 1,
  EventsFinalGrade = 2,
  ModuleGrade = 3
}
