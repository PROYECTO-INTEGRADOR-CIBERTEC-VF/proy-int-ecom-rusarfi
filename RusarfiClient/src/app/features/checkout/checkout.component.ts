import { Component, inject, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { ChangeDetectorRef } from '@angular/core';
import { finalize } from 'rxjs';
import { Subscription } from 'rxjs';
import { CartService } from '../../core/services/cart.service';
import { OrderService } from '../../core/services/order.service';
import { AuthService } from '../../core/services/auth.service';
import { NotificationService } from '../../core/services/notification';

@Component({
  selector: 'app-checkout',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './checkout.component.html',
  styleUrls: ['./checkout.component.css']
})
export class CheckoutComponent implements OnInit {

  private cartService = inject(CartService);
  private orderService = inject(OrderService);
  private fb = inject(FormBuilder);
  private router = inject(Router);
  private cdr = inject(ChangeDetectorRef);

  userId: number | null = null;
  private userSub: Subscription | null = null;
  private authService = inject(AuthService);
  private notificationService = inject(NotificationService);

  items: any[] = [];
  total = 0;

  isLoading = true;
  errorMessage = '';

  showModal = false;
  orderId: number | null = null;

  isProcessing = false;

  form = this.fb.group({
  name: ['', [Validators.required, Validators.minLength(3)]],
  phone: ['', [Validators.required, Validators.pattern(/^[0-9]{9}$/)]], 
  address: ['', [Validators.required, Validators.minLength(5)]],
  paymentMethod: ['card', Validators.required]
});

  ngOnInit(): void {
    this.userSub = this.authService.currentUser$.subscribe((u) => {
      this.userId = u?.id ?? null;
      this.loadCart();
    });
  }


  loadCart() {
  if (!this.userId) {
    this.items = [];
    this.total = 0;
    this.isLoading = false;
    try { this.cdr.detectChanges(); } catch {}
    return;
  }

  this.cartService.getCart(this.userId).subscribe({
    next: (res) => {
      console.log('Respuesta carrito:', res);

      if (!res.success) {
        this.errorMessage = res.message;
        this.isLoading = false;
        this.cdr.detectChanges(); 
        return;
      }

      const data = res.data;

      this.items = data?.items ?? [];
      this.total = data?.total ?? 0;

      this.isLoading = false;

      this.cdr.detectChanges(); 
    },
    error: (err) => {
      console.error('Error:', err);
      this.errorMessage = 'Error cargando carrito';
      this.isLoading = false;

      this.cdr.detectChanges(); 
    }
  });
}


submitted = false;

finalizePurchase() {

  this.submitted = true;
  this.form.markAllAsTouched(); 

  if (this.form.invalid || this.isProcessing) return;

  this.isProcessing = true;

  if (!this.userId) {
    this.notificationService.show('info', 'Inicia sesión para finalizar la compra');
    this.router.navigate(['/login']);
    return;
  }

  this.orderService.confirmOrder({ userId: this.userId })
    .pipe(
      finalize(() => {
        this.isProcessing = false;
        this.cdr.detectChanges();
      })
    )
    .subscribe({
      next: (res: any) => {
        if (!res?.success) {
          alert(res?.message || 'No se pudo generar la orden');
          return;
        }

        this.orderId = res.data?.orderId;
        this.showModal = true;
        this.cdr.detectChanges();
      },
      error: () => {
        alert('Error al generar la orden');
      }
    });
}


getFieldError(field: string): string | null {
  const control = this.form.get(field);

  if (!control) return null;

  
  if (control.valid) return null;

  if (!control.touched && !this.submitted) return null;

  if (control.errors?.['required']) return 'Este campo es obligatorio';
  if (control.errors?.['minlength']) return 'Debe tener más caracteres';
  if (control.errors?.['pattern']) return 'Número inválido';

  return null; 
}

  closeModal() {
    this.showModal = false;
    this.router.navigate(['/']);
  }

  ngOnDestroy(): void {
    try { this.userSub?.unsubscribe(); } catch {}
  }
}