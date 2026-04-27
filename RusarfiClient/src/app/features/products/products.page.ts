import { CommonModule } from '@angular/common';
import { Component, DestroyRef, inject, ChangeDetectorRef } from '@angular/core';
import { RouterModule } from '@angular/router';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import {
  catchError,
  debounceTime,
  distinctUntilChanged,
  finalize,
  of,
  switchMap,
  timer,
} from 'rxjs';

import { ProductService } from '../../core/services/product.service';
import { ProductDto } from './products.models';
import { CartService } from '../../core/services/cart.service';
import { CartAddRequest } from '../../core/models/cart-item.dto';

@Component({
  selector: 'app-products',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './products.page.html',
})
export class ProductsPage {
  private readonly productService = inject(ProductService);
  private readonly cartService = inject(CartService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly cdr = inject(ChangeDetectorRef);

  protected readonly searchControl = new FormControl('', { nonNullable: true });
  protected readonly categoryControl = new FormControl('', { nonNullable: true });

  protected allProducts: ProductDto[] = [];
  protected products: ProductDto[] = [];
  protected categories: Array<{ name: string; count: number }> = [];
  protected isLoading = true;
  protected errorMessage = '';
  protected successMessage = '';
  protected emptyMessage = 'No hay productos disponibles.';

  private apiMessage = 'No hay productos disponibles.';
  private readonly refreshIntervalMs = 15000;

  constructor() {
    timer(0, this.refreshIntervalMs)
      .pipe(
        switchMap(() => {
          this.isLoading = true;
          this.errorMessage = '';

          return this.productService.getAvailableProducts().pipe(
            catchError(() => {
              this.errorMessage = 'No se pudo cargar el catalogo.';
              return of(null);
            }),
              finalize(() => {
                this.isLoading = false;
                try { this.cdr.detectChanges(); } catch {}
              })
          );
        }),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe((response) => {
        if (!response) {
          this.allProducts = [];
          this.products = [];
          this.categories = [];
          this.emptyMessage = this.apiMessage;
          return;
        }

        if (!response.success) {
          this.errorMessage = response.message || 'No se pudo cargar el catalogo.';
          this.allProducts = [];
          this.products = [];
          this.categories = [];
          this.emptyMessage = this.apiMessage;
          return;
        }

        const items = response.data ?? [];
        this.apiMessage = response.message || this.apiMessage;
        this.allProducts = items;
        this.categories = this.buildCategories(items);
        this.applyFilters();
        try { this.cdr.detectChanges(); } catch {}
      });

    this.searchControl.valueChanges
      .pipe(
        debounceTime(300),
        distinctUntilChanged(),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe(() => this.applyFilters());

    this.categoryControl.valueChanges
      .pipe(distinctUntilChanged(), takeUntilDestroyed(this.destroyRef))
      .subscribe(() => this.applyFilters());
  }

  protected get totalCount(): number {
    return this.allProducts.length;
  }

  protected selectCategory(category: string): void {
    this.categoryControl.setValue(category);
  }

  protected clearFilters(): void {
    this.searchControl.setValue('');
    this.categoryControl.setValue('');
  }

  protected trackById(_index: number, product: ProductDto): number {
    return product.id;
  }

  protected addToCart(product: ProductDto): void {
    this.successMessage = '';
    this.errorMessage = '';
    const req: CartAddRequest = { userId: 1, productId: product.id, quantity: 1 };
    this.cartService.addProduct(req).subscribe({
      next: (res) => {
        if (res?.success) {
          this.successMessage = 'Producto agregado al carrito.';
        } else {
          this.errorMessage = res?.message || 'No se pudo agregar al carrito.';
        }
        try { this.cdr.detectChanges(); } catch {}
      },
      error: () => {
        this.errorMessage = 'No se pudo agregar al carrito.';
        try { this.cdr.detectChanges(); } catch {}
      }
    });
  }

  private applyFilters(): void {
    const search = this.searchControl.value.trim().toLowerCase();
    const category = this.categoryControl.value.trim().toLowerCase();

    this.products = this.allProducts.filter((product) => {
      const nameMatch =
        !search || product.name.toLowerCase().includes(search);
      const categoryMatch =
        !category ||
        (product.category || '').toLowerCase().includes(category);

      return nameMatch && categoryMatch;
    });

    if (this.products.length === 0) {
      this.emptyMessage = this.hasActiveFilters()
        ? 'No hay productos para los filtros seleccionados.'
        : this.apiMessage;
    }

    try {
      this.cdr.detectChanges();
    } catch {}
  }

  private hasActiveFilters(): boolean {
    return (
      this.searchControl.value.trim().length > 0 ||
      this.categoryControl.value.trim().length > 0
    );
  }

  private buildCategories(
    products: ProductDto[]
  ): Array<{ name: string; count: number }> {
    const totals = new Map<string, number>();

    for (const product of products) {
      const key = (product.category || 'Sin categoria').trim();
      if (!key) {
        continue;
      }
      totals.set(key, (totals.get(key) ?? 0) + 1);
    }

    return Array.from(totals.entries())
      .map(([name, count]) => ({ name, count }))
      .sort((a, b) => a.name.localeCompare(b.name));
  }
}
