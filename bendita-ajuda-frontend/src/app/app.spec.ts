import { TestBed } from '@angular/core/testing';
import { AppModule } from './app-module';
import { App } from './app';

describe('App', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AppModule],
    }).compileComponents();
  });

  it('should create the app', () => {
    const fixture = TestBed.createComponent(App);
    expect(fixture.componentInstance).toBeTruthy();
  });

  it('should render the header with the app name and "Entrar"', async () => {
    const fixture = TestBed.createComponent(App);
    await fixture.whenStable();
    const header = (fixture.nativeElement as HTMLElement).querySelector('header');
    expect(header?.textContent).toContain('Bendita Ajuda');
    expect(header?.textContent).toContain('Entrar');
  });
});
