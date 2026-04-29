import { Component, inject, OnInit, OnDestroy } from '@angular/core';
import { Observable, Subscription } from 'rxjs';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { CommonModule } from '@angular/common';
import { CartService } from '../../core/services/cart.service';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [RouterLink, RouterLinkActive, CommonModule],
  templateUrl: './navbar.component.html',
})
export class NavbarComponent implements OnInit, OnDestroy {
  protected readonly cartService: CartService = inject(CartService);
  protected readonly cartCount$: Observable<number> = this.cartService.cartCount$;
  private readonly authService = inject(AuthService);
  private userSub: Subscription | null = null;

  ngOnInit(): void {
    this.userSub = this.authService.currentUser$.subscribe((u) => {
      if (u && u.id) this.cartService.refreshCount(u.id);
      else this.cartService.clearCount();
    });
  }

  ngOnDestroy(): void {
    try { this.userSub?.unsubscribe(); } catch {}
  }
}
