import { Component, inject, AfterViewInit } from '@angular/core';
import { Observable } from 'rxjs';
import { RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { CartService } from '../../core/services/cart.service';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [RouterLink, CommonModule],
  templateUrl: './navbar.component.html',
})
export class NavbarComponent implements AfterViewInit {
  protected readonly cartService: CartService = inject(CartService);
  protected readonly cartCount$: Observable<number> = this.cartService.cartCount$;
  private readonly userId = 1; 

  ngAfterViewInit(): void {
    setTimeout(() => this.cartService.refreshCount(this.userId));
  }
}
