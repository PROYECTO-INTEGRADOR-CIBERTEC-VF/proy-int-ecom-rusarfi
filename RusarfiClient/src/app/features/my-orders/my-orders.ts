import { Component } from '@angular/core';

@Component({
  selector: 'app-my-orders',
  imports: [],
  templateUrl: './my-orders.html',
  styleUrl: './my-orders.css',
})
export class MyOrders {

  orders = [
    {
      product: "Casaca Oversize",
      total: 120,
      status: "Entregado"
    },
    {
      product: "Pantalón Cargo",
      total: 180,
      status: "En camino"
    }
  ];

}
