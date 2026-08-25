import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';

import { ClienteHomeComponent } from './cliente-home/cliente-home.component';
import { ClienteRoutingModule } from './cliente-routing-module';

@NgModule({
  declarations: [ClienteHomeComponent],
  imports: [CommonModule, ClienteRoutingModule],
})
export class ClienteModule {}
