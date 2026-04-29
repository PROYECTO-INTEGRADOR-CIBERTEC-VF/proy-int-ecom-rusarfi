import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, inject, OnInit, OnDestroy } from '@angular/core';
import { Subscription } from 'rxjs';
import { Router } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { OrderService } from '../../core/services/order.service';
import { NotificationService } from '../../core/services/notification';
import { OrderDto } from '../../core/models/order.models';
import { UserDto } from '../auth/auth.models';

@Component({
  selector: 'app-my-orders',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './my-orders.html',
  styleUrls: ['./my-orders.css'],
})
export class MyOrders implements OnInit, OnDestroy {
  private orderService = inject(OrderService);
  private notificationService = inject(NotificationService);
  private cdr = inject(ChangeDetectorRef);
  private authService = inject(AuthService);
  private router = inject(Router);

  protected orders: OrderDto[] = [];
  protected isLoading = true;
  protected errorMessage = '';
  protected user: UserDto | null = null;
  private userSub: Subscription | null = null;

  protected logout(): void {
    this.notificationService.clear();
    this.authService.logout();
    this.notificationService.show('info', 'Sesión cerrada');
    this.router.navigateByUrl('/login');
  }

  ngOnInit(): void {
    this.userSub = this.authService.currentUser$.subscribe((u) => {
      if (!u) {
        this.router.navigateByUrl('/login');
        return;
      }

      this.user = u;
      this.loadOrders(u.id);
    });
  }

  ngOnDestroy(): void {
    try { this.userSub?.unsubscribe(); } catch {}
  }

  private loadOrders(userId: number): void {
    this.isLoading = true;
    this.errorMessage = '';
    this.orderService.getOrdersByUser(userId).subscribe({
      next: (res) => {
        if (res?.success && Array.isArray(res.data)) {
          this.orders = (res.data as any).map((o: any) => ({
            orderId: o.orderId ?? o.OrderId,
            userId: o.userId ?? o.UserId,
            total: Number(o.total ?? o.Total ?? 0),
            status: o.status ?? o.Status ?? '',
            createdAtUtc: o.createdAtUtc ?? o.CreatedAtUtc,
            items: (o.items ?? o.Items ?? []).map((i: any) => ({
              productId: i.productId ?? i.ProductId,
              productName: i.productName ?? i.ProductName,
              imageUrl: i.imageUrl ?? i.ImageUrl,
              quantity: Number(i.quantity ?? i.Quantity ?? 0),
              unitPrice: Number(i.unitPrice ?? i.UnitPrice ?? 0),
              subtotal: Number(i.subtotal ?? i.Subtotal ?? 0),
            })),
          }));
        } else {
          this.orders = [];
          this.errorMessage = res?.message || 'No se encontraron pedidos.';
          this.notificationService.show('info', this.errorMessage);
        }
      },
      error: (err) => {
        this.errorMessage = err?.error?.message || 'Error al cargar pedidos.';
        this.notificationService.show('error', this.errorMessage);
      },
      complete: () => {
        this.isLoading = false;
        try { this.cdr.detectChanges(); } catch {}
      }
    });
  }
}
