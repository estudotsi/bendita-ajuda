import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { LandingPageComponent } from './landing-page/landing-page.component';
import { PrestadorAgendaPublicaComponent } from './prestador-agenda-publica/prestador-agenda-publica.component';
import { PrestadoresPorServicoComponent } from './prestadores-por-servico/prestadores-por-servico.component';
import { PrincipalLayoutComponent } from './principal-layout/principal-layout.component';

const routes: Routes = [
  {
    path: '',
    component: PrincipalLayoutComponent,
    children: [
      {
        path: '',
        component: LandingPageComponent,
      },
      {
        path: 'prestadores',
        component: PrestadoresPorServicoComponent,
      },
      {
        path: 'prestadores/:prestadorId/agenda',
        component: PrestadorAgendaPublicaComponent,
      },
    ],
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class PrincipalRoutingModule {}
