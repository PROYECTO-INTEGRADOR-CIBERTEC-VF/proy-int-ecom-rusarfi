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
    const msg = { summary, detail, severity, id } as NotificationMessage;

    try { console.debug('[NotificationService] show', msg); } catch {}
    try {
      setTimeout(() => this.notificationSignal.set(msg), 0);
    } catch {
      this.notificationSignal.set(msg);
    }
  }

  clear() {
    try { console.debug('[NotificationService] clear'); } catch {}
    try {
      setTimeout(() => this.notificationSignal.set(null), 0);
    } catch {
      this.notificationSignal.set(null);
    }
  }
}
