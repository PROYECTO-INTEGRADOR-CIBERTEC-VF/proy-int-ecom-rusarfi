import { Routes } from '@angular/router';
import { LoginPage } from './features/auth/login/login.page';
import { RegisterPage } from './features/auth/register/register.page';

export const routes: Routes = [
	{ path: '', pathMatch: 'full', redirectTo: 'login' },
	{ path: 'register', component: RegisterPage },
	{ path: 'login', component: LoginPage },
	{ path: '**', redirectTo: 'login' }
];
