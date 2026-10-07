import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AdminSuggestionsPage } from './admin-suggestions.page';

const routes: Routes = [{ path: '', component: AdminSuggestionsPage, title: 'Serviços sugeridos — Bendita Ajuda' }];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class AdminRoutingModule {}
