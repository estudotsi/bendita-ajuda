import { NgModule, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideHttpClient, withFetch } from '@angular/common/http';
import { BrowserModule } from '@angular/platform-browser';
import { provideAppIcons } from './core/icons/app-icons';
import { SharedModule } from './shared/shared-module';
import { AppRoutingModule } from './app-routing-module';
import { App } from './app';

@NgModule({
  declarations: [App],
  imports: [BrowserModule, AppRoutingModule, SharedModule],
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideHttpClient(withFetch()), // pronto para quando os services chamarem a API
    provideAppIcons(),
  ],
  bootstrap: [App],
})
export class AppModule {}
