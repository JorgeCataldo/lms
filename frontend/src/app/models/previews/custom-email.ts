export interface CustomEmailPreview {
  id: string;
  title: string;
  text: string;
  usersIds: string[];
  createdAt: Date;
  users: UserInfoPreview[];
}

export interface UserInfoPreview {
  id: string;
  name: string;
  imageUrl: string;
}
