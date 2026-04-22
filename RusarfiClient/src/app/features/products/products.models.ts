import { ApiResponse } from '../../core/models/api-response';

export interface ProductDto {
  id: number;
  name: string;
  category: string;
  price: number;
  imageUrl: string;
}

export type ProductsResponse = ApiResponse<ProductDto[]>;
