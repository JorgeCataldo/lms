import { UserSummaryProgress } from './user-summary-progress';
import { UserRelationalItem } from './user-relational-item';

export class UserSummary {
    public id: string;
    public name: string;
    public rank: UserRelationalItem;
    public registrationId: string;
    public info: string;
    public imageUrl: string;
    public modulesInfo: UserSummaryProgress[];
    public tracksInfo: UserSummaryProgress[];
    public eventsInfo: UserSummaryProgress[];
    public acquiredKnowledge: string[];
    public isBlocked: boolean;
}
