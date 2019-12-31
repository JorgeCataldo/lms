import { Requirement } from 'src/app/settings/modules/new-module/models/new-requirement.model';

export interface ModulePreview {
  id?: string;
  title: string;
  excerpt: string;
  instructorId?: string;
  instructor: string;
  initialDate?: Date;
  subscriptionEndDate?: Date;
  imageUrl: string;
  progress?: number;
  level?: string;
  requirements: Array<Requirement>;
  subjects?: Array<ModuleSubjectPreview>;
  tags?: Array<string>;
  hasUserProgess: boolean;
  published?: boolean;
  deletedAt?: Date | string;
  deletedBy?: string;
  isDraft?: boolean;
  moduleId?: string;

  checked?: boolean;
}
export interface ModuleSubjectPreview {
  id: string;
  contents: Array<SubjectContentPreview>;
  title?: string;
}
export interface SubjectContentPreview {
  id: string;
}
