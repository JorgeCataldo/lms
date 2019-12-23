import { UserPreview } from './previews/user.interface';

export class UserSync {
  public status: boolean;
  public date: Date;
  public usersCount: number;
  public newUsersCount: number;
  public updatedUsers: number;
  public blockedUsers: number;
  public users: Array<UserPreview>;
}
