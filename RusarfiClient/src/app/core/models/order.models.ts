export interface OrderItemDto {
  productId: number;
  productName: string;
  imageUrl?: string;
  quantity: number;
  unitPrice: number;
  subtotal: number;
}

export interface OrderDto {
  orderId: number;
  userId: number;
  total: number;
  status: string;
  createdAtUtc: string;
  items: OrderItemDto[];
}
