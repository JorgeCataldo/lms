import { UserRelationalItem } from '../settings/users/user-models/user-relational-item';

export class EventSchedule {
  public id?: string;
  public eventId: string;
  public eventScheduleId: string;
  public eventDate: Date;
  public subscriptionStartDate: Date;
  public subscriptionEndDate: Date;
  public forumStartDate: Date;
  public forumEndDate: Date;
  public duration: number;
  public published: boolean;
  public usersTotal: number;
  public approvedUsersTotal: number;
  public rejectedUsersTotal: number;
  public finishedAt?: Date;
  public finishedBy?: string;
  public sentReactionEvaluationEmails: boolean;
  public webinarUrl?: string;
  public location?: UserRelationalItem;
  public applicationLimit?: number;

  public eventTitle?: string;
  public startHour?: string;
}
