export interface CartItemDto {
  id: number;
  productId: number;
  productName: string;
  price: number;
  quantity: number;
  subtotal: number;
}

export interface CartSummaryDto {
  userId: number;
  items: any[];
  total: number;
}

export interface CartAddRequest {
  userId: number;
  productId: number;
  quantity: number;
}

export interface CartUpdateRequest {
  userId: number;
  productId: number;
  quantity: number;
}

export interface CartRemoveRequest {
  userId: number;
  productId: number;
}
