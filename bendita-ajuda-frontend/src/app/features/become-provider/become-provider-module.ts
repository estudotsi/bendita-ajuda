import { NgModule } from '@angular/core';
import { SharedModule } from '../../shared/shared-module';
import { BecomeProviderPage } from './become-provider.page';
import { BecomeProviderRoutingModule } from './become-provider-routing-module';

@NgModule({
  declarations: [BecomeProviderPage],
  imports: [SharedModule, BecomeProviderRoutingModule],
})
export class BecomeProviderModule {}
