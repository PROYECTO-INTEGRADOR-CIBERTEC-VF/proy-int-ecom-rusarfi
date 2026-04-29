import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import {
  FormBuilder,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs/operators';

import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification';

import { FormFieldComponent } from '../../../core/components/form-field/form-field';
import { ButtonComponent } from '../../../core/components/button/button';

@Component({
  selector: 'app-login-page',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterLink,
    FormFieldComponent,
    ButtonComponent
  ],
  templateUrl: './login.page.html',
  styleUrl: './login.page.css'
})
export class LoginPage {
  private readonly authService = inject(AuthService);
  private readonly formBuilder = inject(FormBuilder);
  private readonly router = inject(Router);
  private readonly notificationService = inject(NotificationService);

  protected readonly form = this.formBuilder.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required]]
  });

  protected isSubmitting = false;
  protected submitted = false;

  protected submit(): void {
    this.submitted = true;
    this.notificationService.clear();

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      this.notificationService.show(
        'error',
        'Formulario inválido',
        'Revisa tus credenciales.'
      );
      return;
    }

    const payload = this.form.getRawValue() as {
      email: string;
      password: string;
    };

    this.isSubmitting = true;

    this.authService
      .login(payload)
      .pipe(finalize(() => (this.isSubmitting = false)))
      .subscribe({
        next: (res) => {
          if (res?.success) {
            this.notificationService.show('success', res.message || 'Login exitoso');
            this.router.navigateByUrl('/');
            return;
          }

          this.notificationService.show('error', res?.message || 'Credenciales incorrectas');
        },
        error: () => {
          this.notificationService.show('error', 'Credenciales incorrectas');
        }
      });
  }

  protected fieldError(controlName: string): string | null {
    const control = this.form.get(controlName);

    if (!control || !control.errors || (!control.touched && !this.submitted)) {
      return null;
    }

    if (control.errors['required']) {
      return 'Este campo es obligatorio.';
    }

    if (control.errors['email']) {
      return 'Correo inválido.';
    }

    return 'Campo inválido.';
  }
}