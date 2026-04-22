import { Routes } from '@angular/router';

export const routes: Routes = [
	{ path: '', pathMatch: 'full', redirectTo: 'products' },
	{
		path: 'register',
		loadComponent: () =>
			import('./features/auth/register/register.page').then(
				(m) => m.RegisterPage
			)
	},
	{
		path: 'login',
		loadComponent: () =>
			import('./features/auth/login/login.page').then((m) => m.LoginPage)
	},
	{
		path: 'products',
		loadComponent: () =>
			import('./features/products/products.page').then(
				(m) => m.ProductsPage
			)
	},
	{ path: '**', redirectTo: 'products' }
];
