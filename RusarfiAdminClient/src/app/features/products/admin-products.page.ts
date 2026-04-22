import { CommonModule } from '@angular/common';
import { Component, DestroyRef, inject, ChangeDetectorRef } from '@angular/core'; // 🔹 importamos ChangeDetectorRef
import {
  FormBuilder,
  FormControl,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import {
  catchError,
  debounceTime,
  distinctUntilChanged,
  finalize,
  of,
  switchMap
} from 'rxjs';

import {
  AdminProduct,
  CreateProductRequest,
  UpdateProductRequest
} from '../../core/models/admin-product.models';
import { CategoryOption } from '../../core/models/category.models';
import { AdminCategoryService } from '../../core/services/admin-category.service';
import { AdminProductService } from '../../core/services/admin-product.service';
import { NotificationService } from '../../core/services/notification.service';
import { ProductImageService } from '../../core/services/product-image.service';

@Component({
  selector: 'app-admin-products',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './admin-products.page.html',
  styleUrl: './admin-products.page.css'
})
export class AdminProductsPage {
  private readonly productService = inject(AdminProductService);
  private readonly categoryService = inject(AdminCategoryService);
  private readonly imageService = inject(ProductImageService);
  private readonly notificationService = inject(NotificationService);
  private readonly formBuilder = inject(FormBuilder);
  private readonly destroyRef = inject(DestroyRef);
  private readonly cdr = inject(ChangeDetectorRef); // 🔹 agregado

  protected readonly searchControl = new FormControl('', { nonNullable: true });

  protected readonly form = this.formBuilder.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(150)]],
    categoryId: [0, [Validators.required, Validators.min(1)]],
    description: ['', [Validators.maxLength(500)]],
    price: [0, [Validators.required, Validators.min(0.01)]],
    stock: [0, [Validators.required, Validators.min(0)]],
    imageUrl: ['', [Validators.maxLength(500)]],
    isActive: [true]
  });

  protected products: AdminProduct[] = [];
  protected categories: CategoryOption[] = [];
  protected isLoading = true;
  protected isSaving = false;
  protected isUploading = false;
  protected isModalOpen = false;
  protected submitted = false;
  protected errorMessage = '';

  protected editingProductId: number | null = null;

  constructor() {
    this.loadCategories().subscribe();
    this.loadProducts().subscribe();

    this.searchControl.valueChanges
      .pipe(
        debounceTime(350),
        distinctUntilChanged(),
        switchMap(() => this.loadProducts()),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe();
  }

  protected get activeCount(): number {
    return this.products.filter((product) => product.isActive).length;
  }

  protected get outOfStockCount(): number {
    return this.products.filter((product) => product.stock <= 0).length;
  }

  protected openCreateModal(): void {
    this.editingProductId = null;
    this.submitted = false;
    this.form.reset({
      name: '',
      categoryId: 0,
      description: '',
      price: 0,
      stock: 0,
      imageUrl: '',
      isActive: true
    });
    this.isModalOpen = true;
  }

  protected openEditModal(product: AdminProduct): void {
    this.editingProductId = product.id;
    this.submitted = false;
    this.form.reset({
      name: product.name,
      categoryId: product.categoryId,
      description: product.description,
      price: product.price,
      stock: product.stock,
      imageUrl: product.imageUrl,
      isActive: product.isActive
    });
    this.isModalOpen = true;
  }

  protected closeModal(): void {
    this.isModalOpen = false;
    this.submitted = false;
  }

  protected submitForm(): void {
    this.submitted = true;

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      this.notificationService.show(
        'error',
        'Formulario incompleto',
        'Completa los campos obligatorios.'
      );
      return;
    }

    const formValue = this.form.getRawValue();
    const payload: CreateProductRequest = {
      name: formValue.name.trim(),
      categoryId: Number(formValue.categoryId),
      description: formValue.description?.trim() ?? '',
      price: Number(formValue.price),
      stock: Number(formValue.stock),
      imageUrl: formValue.imageUrl?.trim() ?? ''
    };

    this.isSaving = true;

    const request$ = this.editingProductId
      ? this.productService.updateProduct(this.editingProductId, {
          ...payload,
          isActive: Boolean(formValue.isActive)
        } as UpdateProductRequest)
      : this.productService.createProduct(payload);

    request$
      .pipe(
        finalize(() => {
          this.isSaving = false;
        })
      )
      .subscribe({
        next: (response) => {
          if (!response.success) {
            this.notificationService.show('error', 'No se pudo guardar el producto', response.message);
            return;
          }

          this.notificationService.show(
            'success',
            this.editingProductId ? 'Producto actualizado' : 'Producto creado',
            response.message
          );
          this.isModalOpen = false;
          this.loadProducts().subscribe();
        },
        error: () => {
          this.notificationService.show('error', 'No se pudo guardar el producto');
        }
      });
  }

  protected confirmDelete(product: AdminProduct): void {
    const accepted = window.confirm(`¿Deseas eliminar el producto "${product.name}"?`);
    if (!accepted) {
      return;
    }

    this.productService.deleteProduct(product.id).subscribe({
      next: (response) => {
        if (!response.success) {
          this.notificationService.show('error', 'No se pudo eliminar el producto', response.message);
          return;
        }

        this.notificationService.show('success', 'Producto eliminado', response.message);
        this.loadProducts().subscribe();
      },
      error: () => {
        this.notificationService.show('error', 'No se pudo eliminar el producto');
      }
    });
  }

  protected onImageSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.item(0);
    if (!file) {
      return;
    }

    this.isUploading = true;
    this.imageService
      .uploadImage(file)
      .pipe(
        finalize(() => {
          this.isUploading = false;
        })
      )
      .subscribe({
        next: (response) => {
          if (!response.success || !response.data?.imageUrl) {
            this.notificationService.show('error', 'No se pudo subir la imagen', response.message);
            return;
          }

          this.form.patchValue({ imageUrl: response.data.imageUrl });
          this.notificationService.show('success', 'Imagen cargada', response.message);
        },
        error: () => {
          this.notificationService.show('error', 'No se pudo subir la imagen');
        }
      });
  }

  protected stockIndicatorClass(product: AdminProduct): string {
    if (product.stock <= 0) {
      return 'bg-error';
    }

    if (product.stock <= 5) {
      return 'bg-primary-fixed';
    }

    return 'bg-tertiary';
  }

  protected trackById(_index: number, product: AdminProduct): number {
    return product.id;
  }

  protected fieldError(controlName: string): string | null {
    const control = this.form.get(controlName);

    if (!control || !control.errors || (!control.touched && !this.submitted)) {
      return null;
    }

    if (control.errors['required']) {
      return 'Este campo es obligatorio.';
    }

    if (control.errors['maxlength']) {
      return 'Has excedido el limite permitido.';
    }

    if (control.errors['min']) {
      return 'El valor debe ser mayor a cero.';
    }

    return 'Campo invalido.';
  }

  private loadProducts() {
    const searchValue = this.searchControl.value.trim();

    this.isLoading = true;
    this.errorMessage = '';

    return this.productService
      .getProducts({ search: searchValue || undefined })
      .pipe(
        switchMap((response) => {
          if (!response.success) {
            this.errorMessage = response.message || 'No se pudieron cargar los productos.';
            this.products = [];
            this.cdr.detectChanges(); 
            return of(null);
          }

          this.products = response.data ?? [];
          this.cdr.detectChanges(); // 🔹 fuerza actualización
          return of(response);
        }),
        catchError(() => {
          this.errorMessage = 'No se pudieron cargar los productos.';
          this.products = [];
          this.cdr.detectChanges(); // 🔹 fuerza actualización
          return of(null);
        }),
        finalize(() => {
          this.isLoading = false;
          this.cdr.detectChanges(); // 🔹 fuerza actualización
        })
      );
    }

    private loadCategories() {
      return this.categoryService.getCategories().pipe(
        switchMap((response) => {
          if (!response.success) {
            this.categories = [];
            this.notificationService.show('error', 'No se pudieron cargar las categorías', response.message);
            return of(null);
          }

          this.categories = response.data ?? [];
          return of(response);
        }),
        catchError(() => {
          this.categories = [];
          this.notificationService.show('error', 'No se pudieron cargar las categorías');
          return of(null);
        })
      );
    }
}
