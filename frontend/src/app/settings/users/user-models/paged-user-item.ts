import { UserRelationalItem } from './user-relational-item';

export interface PagedUserItem {
    id: string;
    imageUrl: string;
    name: string;
    lineManager: string;
    isBlocked: boolean;
    createdAt: Date;
    rank: UserRelationalItem;
}
