import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit, inject } from '@angular/core';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { ProductService } from '../app/core/services/product.service';
import { ProductDto, ProductResponse } from '../app/features/products/products.models';
import { CartService } from '../app/core/services/cart.service';
import { CartAddRequest } from '../app/core/models/cart-item.dto';

@Component({
  selector: 'app-product-detail',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: '../app/product-detail/product-detail.html',
  styleUrls: ['../app/product-detail/product-detail.css']  
})
export class ProductDetailComponent implements OnInit {

  private route = inject(ActivatedRoute);
  private productService = inject(ProductService);
  private cdr = inject(ChangeDetectorRef);
  private cartService = inject(CartService);
  protected successMessage = '';

  product: ProductDto | null = null;
  errorMessage = '';
  isLoading = true;
  images: string[] = [];
  selectedImage: string | null = null;

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));

    if (!Number.isFinite(id) || id <= 0) {
      this.errorMessage = 'Producto no valido.';
      this.isLoading = false;
      return;
    }

    this.productService.getProductById(id).subscribe({
      next: (res: ProductResponse) => {
        console.log('ProductDetail: API response', res);
        if (res?.success && res.data) {
          this.product = res.data;
          const imgVal: any = (res.data as any)?.imageUrl;
          if (Array.isArray(imgVal)) {
            this.images = imgVal.filter(Boolean);
          } else if (typeof imgVal === 'string' && imgVal.trim().length) {
            this.images = [imgVal];
          } else {
            this.images = [];
          }
          this.selectedImage = this.images[0] ?? null;
        } else {
          this.errorMessage = res?.message || 'Producto no encontrado.';
        }
        this.isLoading = false;

        try { this.cdr.detectChanges(); } catch {}
      },
      error: () => {
        console.error('ProductDetail: API error');
        this.errorMessage = 'Error al cargar el producto';
        this.isLoading = false;
        try { this.cdr.detectChanges(); } catch {}
      }
    });
  }

  selectImage(url: string): void {
    this.selectedImage = url;
    try { this.cdr.detectChanges(); } catch {}
  }

  addToCart(): void {
    if (!this.product) return;
    this.successMessage = '';
    const req: CartAddRequest = { userId: 1, productId: this.product.id, quantity: 1 };
    this.cartService.addProduct(req).subscribe({
      next: (res) => {
        if (res?.success) {
          this.successMessage = 'Producto agregado al carrito.';
        } else {
          this.successMessage = res?.message || 'No se pudo agregar al carrito.';
        }
        try { this.cdr.detectChanges(); } catch {}
      },
      error: () => {
        this.successMessage = 'No se pudo agregar al carrito.';
        try { this.cdr.detectChanges(); } catch {}
      }
    });
  }
}
