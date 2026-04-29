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
        return 'bg-tertiary/90 text-white';
      case 'error':
        return 'bg-error/90 text-white';
      case 'warn':
        return 'bg-yellow-700 text-white';
      case 'info':
        return 'bg-blue-600 text-white';
      default:
        return 'bg-gray-900 text-white';
    }
  }
}
