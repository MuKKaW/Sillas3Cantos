import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { Backoffice } from './pages/backoffice/backoffice';
import { Legal } from './pages/legal/legal';
import { Login } from './pages/login/login';
import { PublicCatalog } from './pages/public-catalog/public-catalog';

export const routes: Routes = [
  {
    path: '',
    component: PublicCatalog
  },
  {
    path: 'login',
    component: Login
  },
  {
    path: 'legal',
    component: Legal
  },
  {
    path: 'backoffice',
    component: Backoffice,
    canActivate: [authGuard]
  },
  {
    path: '**',
    redirectTo: ''
  }
];
