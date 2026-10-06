import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { ProviderProfilePage } from './provider-profile.page';

const routes: Routes = [{ path: ':id', component: ProviderProfilePage, title: 'Profissional — Bendita Ajuda' }];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class ProviderProfileRoutingModule {}
