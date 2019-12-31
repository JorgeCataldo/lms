import { TrackModule } from './track-module.model';
import { TrackEvent } from './track-event.model';
import { User } from './user.model';
import { ContentTypeEnum } from './enums/content-type.enum';
import { UserFile } from './user-file.interface';

export interface TrackOverview {
  id: string;
  title: string;
  studentsCount: number;
  eventsConfiguration: Array<TrackEvent>;
  modulesConfiguration: Array<TrackModule>;
  students: Array<User>;
  duration: number;
  wrongConcepts: Array<WrongConcept>;
  isStudent: boolean;
  bottomPerformants: Array<StudentPerformance>;
  topPerformants: Array<StudentPerformance>;
  lateStudents: Array<LateStudent>;
  storeUrl: string;
  ecommerceUrl: string;
}

export interface TrackStudentOverview {
  id: string;
  title: string;
  studentsCount: number;
  student: StudentOverview;
  eventsConfiguration?: Array<TrackEvent>;
  modulesConfiguration: Array<TrackModule>;
  warnings: Array<Warning>;
  duration: number;
  conclusion?: number;
  certificateUrl: string;
  videoUrl?: string;
  videoDuration?: number;
  videoId?: string;
  mandatoryCourseVideo?: boolean;
  courseVideoUrl?: string;
  courseVideoDuration?: number;
  blockedByUserCareer: boolean;
  AllowedPercentageWithoutCareerInfo?: number;
  trackInfo: any;
  calendarEvents?: Array<TrackEvent>;
}

export interface TrackModuleOverview {
  id: string;
  title: string;
  moduleTitle: string;
  studentsCount: number;
  duration: number;
  wrongConcepts: Array<WrongConcept>;
  studentsProgress: Array<StudentsProgress>;
  students: Array<StudentProgress>;
  viewedContents: Array<ViewedContent>;
  subjectsProgress: Array<SubjectProgress>;
  modulesConfiguration: Array<TrackModule>;
}

export interface StudentOverview {
  id: string;
  imageUrl: string;
  name: string;
  attendedEvents: Array<AttendedEvent>;
  unachievedGoals: number;
  achievedGoals: number;
  wrongConcepts: Array<WrongConcept>;
  points: number;
}

export interface AttendedEvent {
  eventScheduleId: string;
  title: string;
  eventDate: Date;
  presence: boolean;
  finalGrade: number;
}

export interface WrongConcept {
  concept: string;
  moduleName: string;
  count: number;
}

export interface StudentsProgress {
  count: number;
  level: number;
}

export interface StudentProgress {
  id: string;
  imageUrl: string;
  name: string;
  level: number;
  objective: number;
  finished: boolean;
  userFiles: UserFile;
}

export interface ViewedContent {
  contentTitle: string;
  contentType: ContentTypeEnum;
  count: number;
}

export interface SubjectProgress {
  subjectTitle: string;
  level: number;
}

export interface Warning {
  text: string;
  dueTo?: Date;
  redirectTo: string;
}

export interface StudentPerformance {
  id: string;
  imageUrl: string;
  name: string;
  points: string | number;
  businessGroup?: string;
  businessUnit?: string;
  segment?: string;
}

export interface LateStudent {
  studentId: string;
  progress: number;
  lateEvents: Array<string>;
  eventsTotal: number;
}
