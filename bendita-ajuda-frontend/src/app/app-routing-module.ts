import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

/** Cada funcionalidade é um módulo carregado sob demanda (lazy loading). */
const routes: Routes = [
  {
    path: '',
    pathMatch: 'full',
    loadChildren: () => import('./features/home/home-module').then((m) => m.HomeModule),
  },
  {
    path: 'entrar',
    loadChildren: () => import('./features/auth/auth-module').then((m) => m.AuthModule),
  },
  {
    path: 'prestador',
    loadChildren: () =>
      import('./features/provider-profile/provider-profile-module').then((m) => m.ProviderProfileModule),
  },
  {
    path: 'seja-prestador',
    loadChildren: () =>
      import('./features/become-provider/become-provider-module').then((m) => m.BecomeProviderModule),
  },
  { path: '**', redirectTo: '' },
];

@NgModule({
  imports: [
    RouterModule.forRoot(routes, {
      bindToComponentInputs: true, // :id e ?voltarPara chegam como input() nas páginas
      scrollPositionRestoration: 'enabled',
    }),
  ],
  exports: [RouterModule],
})
export class AppRoutingModule {}
