export interface UserSyncPreview {
  id: string;
  importStatus: number;
  importErrors: ImportError[];
  totalUsers: number;
  newUsers: ImportedUsers;
  updatedUsers: ImportedUsers;
  blockedUsers: ImportedUsers;
  createdAt: Date;
}

export interface ImportedUsers {
  users: ImportUser[];
  quantity: number;
}

export interface ImportUser {
  imgUrl: string;
  name: string;
  rank: string;
  responsible: string;
  status: string;
}

export interface ImportError {
  importAction: string;
  name: string;
  username: string;
  cge?: number;
  importErrorString: string;
}
