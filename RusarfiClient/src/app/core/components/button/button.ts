import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

type ButtonVariant = 'primary' | 'secondary' | 'ghost';

@Component({
  selector: 'app-button',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './button.html',
})
export class ButtonComponent {
  @Input() type: 'button' | 'submit' | 'reset' = 'button';
  @Input() variant: ButtonVariant = 'primary';
  @Input() disabled = false;

  get buttonClasses(): string[] {
    const baseClasses = [
      'w-full',
      'font-body',
      'font-bold',
      'py-5',
      'rounded-lg',
      'shadow-lg',
      'hover:bg-primary-container',
      'transition-all',
      'active:scale-[0.98]',
      'uppercase',
      'tracking-[0.2em]',
      'text-xs',
      'disabled:opacity-60',
    ];

    const variantClasses: Record<ButtonVariant, string[]> = {
      primary: ['bg-primary-fixed', 'text-on-primary'],
      secondary: [
        'bg-surface-container-lowest',
        'text-on-surface',
        'border',
        'border-surface-dim',
        'hover:bg-white',
      ],
      ghost: ['bg-transparent', 'text-on-surface', 'shadow-none'],
    };

    return [...baseClasses, ...variantClasses[this.variant]];
  }
}

