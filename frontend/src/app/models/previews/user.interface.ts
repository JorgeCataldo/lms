import { UserSyncStatusEnum } from '../enums/user-sync-status.enum';
import { UserRelationalItem } from 'src/app/settings/users/user-models/user-relational-item';
import { Address } from 'src/app/settings/users/user-models/user';

export interface UserPreview {
  id: string;
  imageUrl: string;
  name: string;
  userName: string;
  email: string;
  registrationId: string;
  lineManager: string;
  isBlocked: boolean;
  createdAt: Date;
  rank: UserRelationalItem;
  status?: UserSyncStatusEnum;
  checked?: boolean;
  dateBorn?: Date;
  address: Address;

}
