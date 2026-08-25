import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { CompletarCadastroComponent } from './completar-cadastro/completar-cadastro.component';
import { PrestadorAgendaComponent } from './prestador-agenda/prestador-agenda.component';
import { PrestadorLayoutComponent } from './prestador-layout/prestador-layout.component';
import { PrestadorHomeComponent } from './prestador-home/prestador-home.component';

const routes: Routes = [
  {
    path: '',
    component: PrestadorLayoutComponent,
    children: [
      {
        path: '',
        redirectTo: 'home',
        pathMatch: 'full',
      },
      {
        path: 'home',
        component: PrestadorHomeComponent,
      },
      {
        path: 'completar-cadastro',
        component: CompletarCadastroComponent,
      },
      {
        path: 'agenda',
        component: PrestadorAgendaComponent,
      },
    ],
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class PrestadorRoutingModule {}
