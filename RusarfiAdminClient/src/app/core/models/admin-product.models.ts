export interface AdminProduct {
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

export interface CreateProductRequest {
  name: string;
  categoryId: number;
  description?: string;
  price: number;
  imageUrl?: string;
  stock: number;
}

export interface UpdateProductRequest extends CreateProductRequest {
  isActive: boolean;
}

export interface ProductImageResponse {
  imageUrl: string;
}
