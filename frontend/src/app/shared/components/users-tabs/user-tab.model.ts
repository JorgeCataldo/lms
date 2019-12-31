import { UserStatusEnum } from '../../../models/enums/user-status.enum';

export class UserTab {
  public count: number;
  public title: string;
  public color: string;
  public status: UserStatusEnum;

  constructor(
    title: string, color: string,
    status: UserStatusEnum,
    count: number = 0
  ) {
    this.title = title;
    this.color = color;
    this.status = status;
    this.count = count;
  }
}
