import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';

import { AdminRoutingModule } from './admin-routing-module';
import { AdminHomeComponent } from './admin-home/admin-home.component';
import { AdminLayoutComponent } from './admin-layout/admin-layout.component';
import { AdminServicosService } from './services/admin-servicos.service';

@NgModule({
  declarations: [AdminLayoutComponent, AdminHomeComponent],
  imports: [CommonModule, FormsModule, ReactiveFormsModule, AdminRoutingModule],
  providers: [AdminServicosService],
})
export class AdminModule {}
