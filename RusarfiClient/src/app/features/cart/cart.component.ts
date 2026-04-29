import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, inject, OnInit, OnDestroy } from '@angular/core';
import { RouterModule } from '@angular/router';
import { catchError, of, Subscription } from 'rxjs';
import { Router } from '@angular/router';

import { CartService } from '../../core/services/cart.service';
import { CartItemDto, CartUpdateRequest, CartRemoveRequest } from '../../core/models/cart-item.dto';
import { NotificationService } from '../../core/services/notification';
import { AuthService } from '../../core/services/auth.service';

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
  private readonly notificationService = inject(NotificationService);
  private router = inject(Router);

  protected items: CartItemDto[] = [];
  protected isLoading = true;
  protected errorMessage = '';
  protected total = 0;
  protected currentUserId: number | null = null;
  private userSub: Subscription | null = null;
  private authService = inject(AuthService);

  ngOnInit(): void {
    this.userSub = this.authService.currentUser$.subscribe((u) => {
      this.currentUserId = u?.id ?? null;
      this.loadCart();
    });
  }

  loadCart(): void {
    this.isLoading = true;
    this.errorMessage = '';

    if (!this.currentUserId) {
      this.items = [];
      this.total = 0;
      this.isLoading = false;
      try { this.cdr.detectChanges(); } catch {}
      return;
    }

    this.cartService
      .getCart(this.currentUserId)
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
      userId: this.currentUserId ?? 0,
      productId: item.productId,
      quantity: qty,
    };

    this.cartService.updateQuantity(req).subscribe({
      next: (res) => {
        if (res?.success) {
          this.notificationService.show('success', res.message || 'Cantidad actualizada');
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
    if (!this.currentUserId) {
      this.notificationService.show('info', 'Inicia sesión para modificar el carrito');
      this.router.navigate(['/login']);
      return;
    }

    const req: CartRemoveRequest = { userId: this.currentUserId, productId: item.productId };
    this.cartService.removeProduct(req).subscribe({
      next: (res) => {
        if (res?.success) {
          this.notificationService.show('success', res.message || 'Producto eliminado');
          this.loadCart();
        } else {
          this.errorMessage = res?.message || 'No se pudo eliminar el producto.';
          try { this.cdr.detectChanges(); } catch {}
        }
      },
      error: () => {
        this.errorMessage = 'No se pudo eliminar el producto.';
        try { this.cdr.detectChanges(); } catch {}
        this.notificationService.show('error', 'No se pudo eliminar el producto.');
      },
    });
  }

  computeTotal(): void {
    this.total = this.items.reduce((s, it) => s + (it.subtotal ?? it.price * it.quantity), 0);
  }

  confirmPurchase() {
  this.router.navigate(['/checkout']);
}

  ngOnDestroy(): void {
    try { this.userSub?.unsubscribe(); } catch {}
  }
}
