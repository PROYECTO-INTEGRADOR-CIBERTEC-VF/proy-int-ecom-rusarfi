import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, inject, OnInit } from '@angular/core';
import { RouterModule } from '@angular/router';
import { catchError, of } from 'rxjs';

import { CartService } from '../../core/services/cart.service';
import { CartItemDto, CartUpdateRequest, CartRemoveRequest } from '../../core/models/cart-item.dto';

@Component({
  selector: 'app-cart',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './cart.component.html',
  styleUrls: ['./cart.component.css'],
})
export class CartComponent implements OnInit {
  private readonly cartService = inject(CartService);
  private readonly cdr = inject(ChangeDetectorRef);

  protected items: CartItemDto[] = [];
  protected isLoading = true;
  protected errorMessage = '';
  protected total = 0;


  private readonly userId = 1;

  ngOnInit(): void {
    this.loadCart();
  }

  loadCart(): void {
    this.isLoading = true;
    this.errorMessage = '';

    this.cartService
      .getCart(this.userId)
      .pipe(
        catchError((err) => {
          console.error('Cart load error', err);
          this.errorMessage = 'No se pudo cargar el carrito.';
          return of(null);
        }),
      )
      .subscribe(
        (res) => {
          if (!res) {
            this.items = [];
            this.computeTotal();
            try { this.cdr.detectChanges(); } catch {}
            return;
          }

          if (!res.success) {
            this.errorMessage = res.message || 'No se pudo cargar el carrito.';
            this.items = [];
            this.computeTotal();
            try { this.cdr.detectChanges(); } catch {}
            return;
          }

          const data: any = res.data ?? {};
          const itemsArr = data?.items ?? data?.Items ?? [];
          const mapped = Array.isArray(itemsArr)
            ? itemsArr.map((it: any, idx: number) => ({
                id: it.productId ?? it.id ?? idx,
                productId: it.productId ?? it.id ?? 0,
                productName: it.productName ?? it.productName ?? it.product_name ?? '',
                price: Number(it.unitPrice ?? it.price ?? 0),
                quantity: Number(it.quantity ?? it.quantity ?? 0),
                subtotal: Number(it.subtotal ?? ( (it.unitPrice ?? it.price ?? 0) * (it.quantity ?? 0) )),
              }))
            : [];

          this.items = mapped;
          this.total = Number(data?.total ?? data?.Total ?? 0) || 0;
          if (!this.total) this.computeTotal();
          try { this.cdr.detectChanges(); } catch {}
        },
        (err) => {
          this.errorMessage = this.errorMessage || 'Error al cargar el carrito.';
        },
        () => {
          this.isLoading = false;
          try { this.cdr.detectChanges(); } catch {}
        }
      );
  }

  onQuantityChange(item: CartItemDto, value: string | number): void {
    const qty = Number(value) || 0;
    if (qty < 1) {
      return;
    }

    const req: CartUpdateRequest = {
      userId: this.userId,
      productId: item.productId,
      quantity: qty,
    };

    this.cartService.updateQuantity(req).subscribe({
      next: (res) => {
        if (res?.success) {
          // reload full cart summary from server to keep data consistent
          this.loadCart();
        } else {
          this.errorMessage = res?.message || 'No se pudo actualizar cantidad.';
          try { this.cdr.detectChanges(); } catch {}
        }
      },
      error: () => {
        this.errorMessage = 'No se pudo actualizar cantidad.';
        try { this.cdr.detectChanges(); } catch {}
      },
    });
  }

  removeItem(item: CartItemDto): void {
    const req: CartRemoveRequest = { userId: this.userId, productId: item.productId };
    this.cartService.removeProduct(req).subscribe({
      next: (res) => {
        if (res?.success) {
          // reload server summary to reflect changes
          this.loadCart();
        } else {
          this.errorMessage = res?.message || 'No se pudo eliminar el producto.';
          try { this.cdr.detectChanges(); } catch {}
        }
      },
      error: () => {
        this.errorMessage = 'No se pudo eliminar el producto.';
        try { this.cdr.detectChanges(); } catch {}
      },
    });
  }

  computeTotal(): void {
    this.total = this.items.reduce((s, it) => s + (it.subtotal ?? it.price * it.quantity), 0);
  }

  confirmPurchase(): void {
    // Placeholder: implement checkout flow (create Order) later
    if (this.items.length === 0) return;
    alert('Confirmar compra - implementar flujo de checkout');
  }
}
