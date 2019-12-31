import { UserRelationalItem } from 'src/app/settings/users/user-models/user-relational-item';

export interface EventPreview {
  id?: string;
  title: string;
  excerpt?: string;
  instructorId?: string;
  instructor?: string;
  date?: Date;
  imageUrl: string;
  subscriptionDue?: Date;
  status?: string;
  nextSchedule: NextSchedule;
  published?: boolean;
  hasUserProgess?: boolean;
  schedules?: any;
  requirements?: any;
  moduleProgressInfo?: any;
  isDraft?: boolean;
  location?: UserRelationalItem;

  checked?: boolean;
}

export interface NextSchedule {
  id: string;
  duration: number;
  eventDate: string | Date;
  published: boolean;
  subscriptionEndDate: string | Date;
  subscriptionStartDate: string | Date;
}
