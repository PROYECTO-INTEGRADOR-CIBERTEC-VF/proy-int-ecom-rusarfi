import { Component } from '@angular/core';

@Component({
  selector: 'app-orders-admin',
  imports: [],
  templateUrl: './orders-admin.html',
  styleUrl: './orders-admin.css',
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