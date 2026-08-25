import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';

import { OperadorHomeComponent } from './operador-home/operador-home.component';
import { OperadorRoutingModule } from './operador-routing-module';

@NgModule({
  declarations: [OperadorHomeComponent],
  imports: [CommonModule, OperadorRoutingModule],
})
export class OperadorModule {}
