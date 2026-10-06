import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { LucideDynamicIcon } from '@lucide/angular';
import { Avatar } from './components/avatar/avatar';
import { PlaceholderPage } from './components/placeholder-page/placeholder-page';
import { SiteHeader } from './components/site-header/site-header';
import { StarRating } from './components/star-rating/star-rating';
import { WhatsappButton } from './components/whatsapp-button/whatsapp-button';

const COMPONENTS = [Avatar, PlaceholderPage, SiteHeader, StarRating, WhatsappButton];

/**
 * Peças reutilizadas por várias telas. Os módulos de funcionalidade
 * (features) importam este módulo para ter acesso a elas e aos ícones.
 */
@NgModule({
  declarations: COMPONENTS,
  imports: [CommonModule, RouterModule, LucideDynamicIcon],
  exports: [CommonModule, RouterModule, LucideDynamicIcon, ...COMPONENTS],
})
export class SharedModule {}
