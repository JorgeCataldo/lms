import { SuggestedProductType } from '../enums/suggested-product-type.enum';

export interface SuggestedProduct {
  productId: string;
  type: SuggestedProductType;
}
