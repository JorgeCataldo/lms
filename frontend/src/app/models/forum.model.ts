import { SubjectPreview } from './previews/subject.interface';

export interface Forum {
  id: string;
  moduleId: string;
  moduleName: string;
  isInstructor: boolean;
  questions: Array<ForumQuestion>;
  subjects: Array<SubjectPreview>;
  lastQuestions: Array<ForumQuestion>;
}

export interface ForumQuestion {
  id: string;
  createdAt?: Date;
  title: string;
  description: string;
  likedBy: Array<string>;
  liked: boolean;
  moduleId?: string;
  moduleName?: string;
  subjectId?: string;
  subjectName?: string;
  contentId?: string;
  contentName?: string;
  position?: string;
  answers?: Array<ForumAnswer>;
  userName?: string;
  userImgUrl?: string;
}

export interface ForumAnswer {
  id?: string;
  questionId: string;
  text: string;
  createdAt?: Date;
  createdBy?: string;
  userName?: string;
  userImgUrl?: string;
  likedBy?: Array<string>;
  liked?: boolean;
}
