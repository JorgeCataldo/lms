export interface ActionInfo {
  id?: string;
  page: ActionInfoPageEnum;
  description: string;
  type: ActionInfoTypeEnum;
  moduleId?: string;
  eventId?: string;
  subjectId?: string;
  contentId?: string;
  concept?: string;
  supportMaterialId?: string;
  questionId?: string;
}

export enum ActionInfoPageEnum {
  Module = 1,
  Content = 2,
  Exam = 3,
  Event = 4,
  Subject = 5
}

export enum ActionInfoTypeEnum {
  Click = 1,
  Access = 2,
  Finish = 3,
  CloseTab = 4,
  VideoPlay = 5,
  LevelUp = 6
}
