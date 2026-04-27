import { Component, effect, signal, WritableSignal } from '@angular/core';
import {
  NotificationService,
  NotificationMessage,
} from '../../services/notification';
import { CommonModule } from '@angular/common';
import { animate, style, transition, trigger } from '@angular/animations';

@Component({
  selector: 'app-notification',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './notification.html',
  styleUrls: ['./notification.css'],
  animations: [
    trigger('fade', [
      transition(':enter', [
        style({ opacity: 0, transform: 'translateY(-20px)' }),
        animate(
          '300ms ease-out',
          style({ opacity: 1, transform: 'translateY(0)' })
        ),
      ]),
      transition(':leave', [
        animate(
          '300ms ease-in',
          style({ opacity: 0, transform: 'translateY(-20px)' })
        ),
      ]),
    ]),
  ],
})
export class NotificationComponent {
  notification: WritableSignal<NotificationMessage | null> = signal(null);
  private timeoutId?: number;

  constructor(private notificationService: NotificationService) {
    effect(() => {
      const currentNotification = this.notificationService.notification();
      
      try { console.debug('[NotificationComponent] effect currentNotification', currentNotification); } catch {}
      this.notification.set(currentNotification);

      if (this.timeoutId) {
        window.clearTimeout(this.timeoutId);
      }

      if (currentNotification) {
        this.timeoutId = window.setTimeout(() => {
          this.close();
        }, 5000);
      }
    });
  }

  close() {
    if (this.timeoutId) {
      window.clearTimeout(this.timeoutId);
      this.timeoutId = undefined;
    }
    this.notificationService.clear();
  }

  get icon(): string {
    switch (this.notification()?.severity) {
      case 'success':
        return 'check_circle';
      case 'error':
        return 'error';
      case 'warn':
        return 'warning';
      case 'info':
        return 'info';
      default:
        return 'notifications';
    }
  }

  get containerClasses(): string {
    switch (this.notification()?.severity) {
      case 'success':
        return 'bg-tertiary/10 border-tertiary text-tertiary-dim';
      case 'error':
        return 'bg-error/10 border-error text-error-dim';
      case 'warn':
        return 'bg-yellow-500/10 border-yellow-500 text-yellow-600';
      case 'info':
        return 'bg-blue-500/10 border-blue-500 text-blue-600';
      default:
        return 'bg-gray-500/10 border-gray-500 text-gray-600';
    }
  }
}

