import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { AuthGuard } from './core/guards/auth.guard';

const routes: Routes = [
  {
    path: '',
    loadChildren: () =>
      import('./pages/principal/principal-module').then((m) => m.PrincipalModule),
  },
  {
    path: '',
    loadChildren: () => import('./pages/auth/auth-module').then((m) => m.AuthModule),
  },
  {
    path: 'home',
    canActivate: [AuthGuard],
    loadChildren: () => import('./pages/home/home-module').then((m) => m.HomeModule),
  },
  {
    path: 'admin',
    canActivate: [AuthGuard],
    data: { roles: ['Admin'] },
    loadChildren: () => import('./pages/admin/admin-module').then((m) => m.AdminModule),
  },
  {
    path: 'operador',
    canActivate: [AuthGuard],
    data: { roles: ['Operador'] },
    loadChildren: () =>
      import('./pages/operador/operador-module').then((m) => m.OperadorModule),
  },
  {
    path: 'prestador',
    canActivate: [AuthGuard],
    data: { roles: ['Prestador'] },
    loadChildren: () =>
      import('./pages/prestador/prestador-module').then((m) => m.PrestadorModule),
  },
  {
    path: 'cliente',
    canActivate: [AuthGuard],
    data: { roles: ['Cliente', 'Prestador'] },
    loadChildren: () => import('./pages/cliente/cliente-module').then((m) => m.ClienteModule),
  },
  {
    path: '**',
    redirectTo: '',
  },
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule],
})
export class AppRoutingModule {}
