import { ActivationTypeEnum } from './enums/activation-status.enum';

export interface Activation {
  id: string;
  type: ActivationTypeEnum;
  active: boolean;
  title: string;
  text: string;
  percentage?: number;
}
