import { Address } from '../settings/users/user-models/user';

export class RecruiterCompany {
  socialName: string;
  businessName: string;
  cnpj?: string;
  address?: Address;
  humanResourcesResponsible?: CompanyContact;
  operationsResponsible?: CompanyContact;
  companySize?: number;
  businessActivity?: string;
  yearlyHiring?: number;
  profileMeasuringTool?: string;
  authLogo?: string;
  logoUrl?: string;
}

export interface CompanyContact {
  name: string;
  email: string;
  phone: string;
}
