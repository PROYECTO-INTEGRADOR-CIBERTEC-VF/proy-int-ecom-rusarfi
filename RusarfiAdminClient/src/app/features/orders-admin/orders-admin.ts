import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-orders-admin',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './orders-admin.html',
  styleUrls: ['./orders-admin.css'],
})
export class OrdersAdmin {

  orders = [
    {
      id: 1,
      customer: "Juan Perez",
      total: 150,
      status: "Pendiente"
    },
    {
      id: 2,
      customer: "Maria Lopez",
      total: 300,
      status: "Pagado"
    }
  ];

}