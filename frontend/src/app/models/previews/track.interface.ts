import { EcommerceProduct } from '../ecommerce-product.model';

export interface TrackPreview {
  id?: string;
  title: string;
  description: string;
  imageUrl: string;
  eventCount: number;
  moduleCount: number;
  recommended: boolean;
  checked?: boolean;
  duration?: number;
  published?: boolean;
  subordinate?: boolean;
  instructor?: boolean;
  ecommerceProducts?: Array<EcommerceProduct>;
}
