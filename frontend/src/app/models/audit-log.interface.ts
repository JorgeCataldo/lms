import { EntityActionEnum } from './enums/audit-log-entity-action.enum';

export interface AuditLog {
  Id: string;
  UserId: string;
  ImpersonatedBy: string;
  Date: Date;
  Action: EntityActionEnum;
  EntityType: string;
  EntityId: string;
  OldValues: string;
  NewValues: string;
  ActionDescription: string;
}
