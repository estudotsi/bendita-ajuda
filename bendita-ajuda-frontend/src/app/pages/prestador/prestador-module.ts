import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';

import { CompletarCadastroComponent } from './completar-cadastro/completar-cadastro.component';
import { PrestadorAgendaComponent } from './prestador-agenda/prestador-agenda.component';
import { PrestadorLayoutComponent } from './prestador-layout/prestador-layout.component';
import { PrestadorHomeComponent } from './prestador-home/prestador-home.component';
import { PrestadorRoutingModule } from './prestador-routing-module';
import { PrestadorService } from './services/prestador.service';

@NgModule({
  declarations: [
    PrestadorLayoutComponent,
    PrestadorHomeComponent,
    CompletarCadastroComponent,
    PrestadorAgendaComponent,
  ],
  imports: [CommonModule, FormsModule, ReactiveFormsModule, PrestadorRoutingModule],
  providers: [PrestadorService],
})
export class PrestadorModule {}
