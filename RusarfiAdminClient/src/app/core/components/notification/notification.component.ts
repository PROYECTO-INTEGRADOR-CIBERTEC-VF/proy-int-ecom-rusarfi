import { Component, effect, signal, WritableSignal } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  NotificationMessage,
  NotificationService
} from '../../services/notification.service';

@Component({
  selector: 'app-notification',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './notification.component.html',
  styleUrl: './notification.component.css'
})
export class NotificationComponent {
  notification: WritableSignal<NotificationMessage | null> = signal(null);

  constructor(private notificationService: NotificationService) {
    effect(() => {
      this.notification.set(this.notificationService.notification());
    });
  }

  close() {
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
