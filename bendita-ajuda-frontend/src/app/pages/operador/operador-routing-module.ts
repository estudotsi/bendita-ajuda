import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { OperadorHomeComponent } from './operador-home/operador-home.component';

const routes: Routes = [
  {
    path: '',
    component: OperadorHomeComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class OperadorRoutingModule {}
