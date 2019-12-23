import { async, TestBed, ComponentFixture } from '@angular/core/testing';
import { BrowserModule, By } from '@angular/platform-browser';
import { ErrorComponent } from '../error.component';
import { RouterTestingModule } from '@angular/router/testing';
import { Router } from '@angular/router';

describe('[Integration] ErrorComponent', () => {

  let fixture: ComponentFixture<ErrorComponent>;
  let component: ErrorComponent;
  let router: Router;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      imports: [
        BrowserModule,
        RouterTestingModule.withRoutes([])
      ],
      declarations: [
        ErrorComponent
      ]
    }).compileComponents().then(() => {
      fixture = TestBed.createComponent(ErrorComponent);
      component = fixture.componentInstance;
      router = TestBed.get(Router);
    });
  }));

  it('should display the route with error', async () => {
    component.route = 'https://www.website.com.br/modulo/idDoModulo';
    fixture.detectChanges();

    const routeInfoEl = fixture.nativeElement.querySelector('div.content p.route');
    expect(routeInfoEl.textContent).toContain('https://www.website.com.br/modulo/idDoModulo');
  });

  it('should navigate to Messages', async () => {
    const routerSpy = spyOn(router, 'navigate');

    const editTestImage = fixture.nativeElement.querySelector('div.content p:last-child a:first-child');
    editTestImage.dispatchEvent(new Event('click'));

    expect(routerSpy).toHaveBeenCalledWith([ 'atendimento' ]);
  });

  it('should navigate to Home', async () => {
    const routerSpy = spyOn(router, 'navigate');

    const editTestImage = fixture.nativeElement.querySelector('div.content p:last-child a:last-child');
    editTestImage.dispatchEvent(new Event('click'));

    expect(routerSpy).toHaveBeenCalledWith([ 'home' ]);
  });

});
