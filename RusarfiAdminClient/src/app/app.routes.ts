import { Routes } from '@angular/router';

export const routes: Routes = [
	{
		path: '',
		pathMatch: 'full',
		redirectTo: 'productos'
	},
	{
		path: 'productos',
		loadComponent: () =>
			import('./features/products/admin-products.page').then(
				(m) => m.AdminProductsPage
			)
	},
	{ path: '**', redirectTo: 'productos' }
];
