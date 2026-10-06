import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { BecomeProviderPage } from './become-provider.page';

const routes: Routes = [{ path: '', component: BecomeProviderPage, title: 'Seja um prestador — Bendita Ajuda' }];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class BecomeProviderRoutingModule {}
