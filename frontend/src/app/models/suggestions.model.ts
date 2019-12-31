import { SuggestedProductType } from './enums/suggested-product-type.enum';

export interface Suggestion {
  id: string;
  title: string;
  excerpt: string;
  imageUrl: string;
  type: SuggestedProductType;
  storeUrl?: string;

  hovered?: boolean;
}
