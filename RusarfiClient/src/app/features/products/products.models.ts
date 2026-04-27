import { ApiResponse } from '../../core/models/api-response';

export interface ProductDto {
  id: number;
  name: string;
  categoryId: number;
  category: string;
  description: string;
  price: number;
  imageUrl: string;
  stock: number;
  isActive: boolean;
}

export type ProductsResponse = ApiResponse<ProductDto[]>;
export type ProductResponse = ApiResponse<ProductDto>;
