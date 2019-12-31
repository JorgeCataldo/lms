import { UserCareer } from '../settings/users/user-models/user-career';
import { UserRelationalItem } from '../settings/users/user-models/user-relational-item';
import { UserProfile } from '../settings/users/user-models/user';
import { Address } from './company.model';


export interface UserRecommendation {
  userInfo: UserRecommendationInfo;
  userCareer: UserCareer;
  userEventApplications: UserEventApplicationItem[];
  currentUser: boolean;
  canFavorite: boolean;
  isFavorite: boolean;
}

export interface UserRecommendationInfo {
  dateBorn?: Date;
  imageUrl: string;
  name: string;
  info: string;
  address: Address;
  email: string;
  phone: string;
  phone2: string;
  linkedIn: string;
  profile: UserProfile;
  allowRecommendation?: boolean;
  secretaryAllowRecommendation?: boolean;
}

export class UserSkill {
  id: string;
  title: string;
  modulesConfiguration: UserSkillTrackModuleItem[];
  studentsCount: number;
  topPerformants: StudentPerformanceItem[];
}

export class UserSkillTrackModuleItem {
  moduleId: string;
  title: string;
  level: number;
  percentage: number;
  studentLevel: number;
  classLevel: number;
}

export class StudentPerformanceItem {
  id: string;
  points: number;
}

export class UserEventApplicationItem {
  eventId: string;
  eventName: string;
  userGradeBaseValues: BaseValue[];
  qcGrade: number;
  tgGrade: number;
  faGrade: number;
}

export interface BaseValue {
  key: string;
  value: string;
}
