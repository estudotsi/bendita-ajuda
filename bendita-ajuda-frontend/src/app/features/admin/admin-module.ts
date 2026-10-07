import { NgModule } from '@angular/core';
import { SharedModule } from '../../shared/shared-module';
import { AdminTabs } from './admin-tabs/admin-tabs';
import { AdminRoutingModule } from './admin-routing-module';
import { AdminSearchesPage } from './admin-searches.page';
import { AdminSuggestionsPage } from './admin-suggestions.page';

@NgModule({
  declarations: [AdminSuggestionsPage, AdminSearchesPage, AdminTabs],
  imports: [SharedModule, AdminRoutingModule],
})
export class AdminModule {}
