import { NgModule } from '@angular/core';
import { SharedModule } from '../../shared/shared-module';
import { AdminRoutingModule } from './admin-routing-module';
import { AdminSuggestionsPage } from './admin-suggestions.page';

@NgModule({
  declarations: [AdminSuggestionsPage],
  imports: [SharedModule, AdminRoutingModule],
})
export class AdminModule {}
