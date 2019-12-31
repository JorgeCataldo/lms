import { Question } from './question.model';
import { Content } from './content.model';

export interface Exam {
  contentId: string;
  content?: Content;
  title: string;
  description: string;
  questions: Array<Question>;
}
