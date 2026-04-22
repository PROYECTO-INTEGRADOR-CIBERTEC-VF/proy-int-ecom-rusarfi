import { Injectable, signal } from '@angular/core';

export type NotificationSeverity = 'success' | 'error' | 'info' | 'warn';

export interface NotificationMessage {
  summary: string;
  detail?: string;
  severity: NotificationSeverity;
  id: number;
}

@Injectable({
  providedIn: 'root',
})
export class NotificationService {
  private notificationSignal = signal<NotificationMessage | null>(null);
  public notification = this.notificationSignal.asReadonly();

  private lastId = 0;

  show(severity: NotificationSeverity, summary: string, detail?: string) {
    const id = ++this.lastId;
    this.notificationSignal.set({ summary, detail, severity, id });
  }

  clear() {
    this.notificationSignal.set(null);
  }
}
