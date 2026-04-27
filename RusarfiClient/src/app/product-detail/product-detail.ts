import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit, inject } from '@angular/core';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { ProductService } from '../core/services/product.service';
import { ProductDto, ProductResponse } from '../features/products/products.models';

@Component({
  selector: 'app-product-detail',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './product-detail.html',
  styleUrls: ['./product-detail.css']  
})
export class ProductDetailComponent implements OnInit {

  private route = inject(ActivatedRoute);
  private productService = inject(ProductService);
  private cdr = inject(ChangeDetectorRef);

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
}
