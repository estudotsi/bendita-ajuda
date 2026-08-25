import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

import { LandingPageComponent } from './landing-page/landing-page.component';
import { PrestadorAgendaPublicaComponent } from './prestador-agenda-publica/prestador-agenda-publica.component';
import { PrestadoresPorServicoComponent } from './prestadores-por-servico/prestadores-por-servico.component';
import { PrincipalLayoutComponent } from './principal-layout/principal-layout.component';
import { PrincipalRoutingModule } from './principal-routing-module';
import { GoogleContactsService } from './services/google-contacts.service';
import { PrincipalService } from './services/principal.service';

@NgModule({
  declarations: [
    PrincipalLayoutComponent,
    LandingPageComponent,
    PrestadoresPorServicoComponent,
    PrestadorAgendaPublicaComponent,
  ],
  imports: [CommonModule, FormsModule, PrincipalRoutingModule],
  providers: [PrincipalService, GoogleContactsService],
})
export class PrincipalModule {}
