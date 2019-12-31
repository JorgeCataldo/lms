import { Employment } from '../employment.interface';

export interface UserJobApplication {
  userId: string;
  imageUrl: string;
  userName: string;
  createdAt: Date;
  approved?: boolean;
  accepted: boolean;
}

export interface JobPosition {
  id: string;
  title: string;
  dueTo: Date;
  priority: JobPositionPriorityEnum;
  status: JobPositionStatusEnum;
  candidatesCount?: number;
  employment?: Employment;
}

export interface JobApplication {
  id: string;
  title: string;
  dueTo: Date;
  priority: JobPositionPriorityEnum;
  status: JobPositionStatusEnum;
  candidates: UserJobApplication[];
  talentsTop: Array<AvailableCandidate>;

}

export interface AvailableCandidate {
  imageUrl: string;
  name: string;
  isFavorite: boolean;
}

export enum JobPositionPriorityEnum {
  High = 1,
  Medium = 2,
  Low = 3
}

export enum JobPositionStatusEnum {
  Open = 1,
  Closed = 2
}

export interface JobListItem {
  jobPositionId: string;
  jobTitle: string;
  recruitingCompanyName: string;
  recruitingCompanyLogoUrl: string;
  employment: Employment;
  applied: boolean;
}

export interface UserJobPositionById {
  jobPositionId: string;
  title: string;
  employment?: Employment;
  applied: boolean;
}
