import { UserStatusEnum } from 'src/app/models/enums/user-status.enum';

export class UserTab {
  public count: number;
  public title: string;
  public subTitle: string;
  public color: string;
  public subColor: string;
  public status: UserStatusEnum;

  constructor(
    title: string, subTitle: string, color: string, subColor: string,
    status: UserStatusEnum,
    count: number = 0
  ) {
    this.title = title;
    this.subTitle = subTitle;
    this.color = color;
    this.subColor = subColor;
    this.status = status;
    this.count = count;
  }
}
