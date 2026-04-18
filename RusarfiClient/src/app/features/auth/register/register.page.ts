import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import {
  AbstractControl,
  FormBuilder,
  ReactiveFormsModule,
  ValidationErrors,
  Validators
} from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs/operators';
import { AuthService } from '../auth.service';
import { RegisterRequest } from '../auth.models';
import { NotificationService } from '../../../core/services/notification';

const passwordMatchValidator = (control: AbstractControl): ValidationErrors | null => {
  const password = control.get('password')?.value as string | undefined;
  const confirmPassword = control.get('confirmPassword')?.value as string | undefined;

  if (!password || !confirmPassword) {
    return null;
  }

  return password === confirmPassword ? null : { passwordMismatch: true };
};

import { FormFieldComponent } from '../../../core/components/form-field/form-field';

import { ButtonComponent } from '../../../core/components/button/button';

@Component({
  selector: 'app-register-page',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterLink,
    FormFieldComponent,
    ButtonComponent,
  ],
  templateUrl: './register.page.html',
  styleUrl: './register.page.css'
})
export class RegisterPage {
  private readonly authService = inject(AuthService);
  private readonly formBuilder = inject(FormBuilder);
  private readonly router = inject(Router);
  private readonly notificationService = inject(NotificationService);

  protected readonly form = this.formBuilder.group(
    {
      name: ['', [Validators.required]],
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(8)]],
      confirmPassword: ['', [Validators.required]]
    },
    { validators: passwordMatchValidator }
  );

  protected isSubmitting = false;
  protected submitted = false;

  protected submit(): void {
    this.submitted = true;
    this.notificationService.clear();

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      this.notificationService.show(
        'error',
        'Formulario Inválido',
        'Por favor, revisa los campos e inténtalo de nuevo.'
      );
      return;
    }

    const payload = this.form.getRawValue() as RegisterRequest;

    this.isSubmitting = true;
    this.authService
      .register(payload)
      .pipe(
        finalize(() => {
          this.isSubmitting = false;
        })
      )
      .subscribe({
        next: (response) => {
          this.notificationService.show(
            'success',
            response.message || 'Registro exitoso'
          );
          this.form.reset();
          this.submitted = false;

          setTimeout(() => {
            this.router.navigateByUrl('/login');
          }, 1200);
        },
        error: (error) => {
          const response = error?.error;
          const message = response?.message as string | undefined;
          this.notificationService.show(
            'error',
            message || 'No se pudo completar el registro'
          );
        },
      });
  }

  protected fieldError(controlName: string): string | null {
    const control = this.form.get(controlName);

    if (!control || !control.errors || (!control.touched && !this.submitted)) {
      return null;
    }

    const errors = control.errors;

    if (errors['required']) {
      return 'Este campo es obligatorio.';
    }
    if (errors['email']) {
      return 'Por favor, ingresa un correo electrónico válido.';
    }
    if (errors['minlength']) {
      const requiredLength = errors['minlength'].requiredLength;
      return `La contraseña debe tener al menos ${requiredLength} caracteres.`;
    }
    if (this.form.hasError('passwordMismatch') && controlName === 'confirmPassword') {
      return 'Las contraseñas no coinciden.';
    }

    return 'Campo inválido.';
  }
}
