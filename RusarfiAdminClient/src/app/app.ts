import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { NotificationComponent } from './core/components/notification/notification.component';
import { AdminNavbarComponent } from './layout/admin-navbar/admin-navbar.component';

@Component({
  selector: 'app-root',
  imports: [
    RouterOutlet,
    NotificationComponent,
    AdminNavbarComponent,
  ],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
}
