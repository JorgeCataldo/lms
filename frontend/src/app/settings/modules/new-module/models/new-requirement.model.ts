import { ModulePreview } from '../../../../models/previews/module.interface';

export class Requirement {
  public moduleId: string;
  public module?: ModulePreview;
  public title?: string;
  public level: number;
  public percentage: number;
  public optional: boolean;
  public requirementValue;

  public editing?: boolean;
  public progress?: RequirementProgress;

  constructor(
    moduleId: string, level: number, optional: boolean = false,
    percentage: number = null, editing: boolean = false
  ) {
    this.moduleId = moduleId;
    this.level = level;
    this.optional = optional;
    this.percentage = percentage;
    this.editing = editing;
  }
}

export interface RequirementProgress {
  moduleId: string;
  level: number;
  progress: number;
}
