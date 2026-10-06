import { NgModule } from '@angular/core';
import { SharedModule } from '../../shared/shared-module';
import { BecomeProviderBanner } from './components/become-provider-banner/become-provider-banner';
import { CategoryGrid } from './components/category-grid/category-grid';
import { LocationChip } from './components/location-chip/location-chip';
import { ProviderCard } from './components/provider-card/provider-card';
import { ProviderCardSkeleton } from './components/provider-card-skeleton/provider-card-skeleton';
import { ProviderList } from './components/provider-list/provider-list';
import { SearchBar } from './components/search-bar/search-bar';
import { HomeRoutingModule } from './home-routing-module';
import { HomePage } from './home.page';

@NgModule({
  declarations: [
    HomePage,
    BecomeProviderBanner,
    CategoryGrid,
    LocationChip,
    ProviderCard,
    ProviderCardSkeleton,
    ProviderList,
    SearchBar,
  ],
  imports: [SharedModule, HomeRoutingModule],
})
export class HomeModule {}
