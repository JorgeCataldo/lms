export interface MenuSection {
  title: string;
  items: Array<MenuItem>;
  checkDependents?: boolean;
  checkAccess?: Array<string>;
  permittedRoles?: Array<string>;
  blockAccess?: Array<string>;
}

export interface MenuItem {
  url: string;
  iconClass: string;
  title: string;
  checkAccess?: Array<string>;
  blockAccess?: Array<string>;
  permittedRoles: Array<string>;
  checkProfileTest: boolean;
  checkFormulas: boolean;
  isRunningFeature: boolean;
}
