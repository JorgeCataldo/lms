import { ContentPreview } from './content.interface';

export interface SubjectPreview {
  id: string;
  title: string;
  contents?: Array<ContentPreview>;
}
