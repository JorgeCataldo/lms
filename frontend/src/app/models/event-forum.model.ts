import { SubjectPreview } from './previews/subject.interface';

export interface EventForum {
  id: string;
  eventId: string;
  eventScheduleId: string;
  eventName: string;
  isInstructor: boolean;
  questions: Array<EventForumQuestion>;
  lastQuestions: Array<EventForumQuestion>;
}

export interface EventForumQuestion {
  id: string;
  title: string;
  description: string;
  likedBy: Array<string>;
  liked: boolean;
  eventId?: string;
  eventScheduleId?: string;
  eventName?: string;
  position?: string;
  answers?: Array<EventForumAnswer>;
}

export interface EventForumAnswer {
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
