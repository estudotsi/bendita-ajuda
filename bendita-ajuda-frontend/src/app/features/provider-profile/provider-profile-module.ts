import { NgModule } from '@angular/core';
import { SharedModule } from '../../shared/shared-module';
import { ProviderProfilePage } from './provider-profile.page';
import { ProviderProfileRoutingModule } from './provider-profile-routing-module';

@NgModule({
  declarations: [ProviderProfilePage],
  imports: [SharedModule, ProviderProfileRoutingModule],
})
export class ProviderProfileModule {}
