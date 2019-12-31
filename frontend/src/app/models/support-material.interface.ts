export interface SupportMaterial {
  id?: string;
  title: string;
  description: string;
  downloadLink: string;
  type?: SupportMaterialType;
}

export enum SupportMaterialType {
  Link = 1,
  File = 2
}
