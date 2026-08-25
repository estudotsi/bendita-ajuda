import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { ClienteHomeComponent } from './cliente-home/cliente-home.component';

const routes: Routes = [
  {
    path: '',
    component: ClienteHomeComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class ClienteRoutingModule {}
