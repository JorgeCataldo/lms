import { ComponentFixture, async, TestBed } from '@angular/core/testing';
import { BrowserModule } from '@angular/platform-browser';
import { RouterTestingModule } from '@angular/router/testing';
import { Router } from '@angular/router';
import { requirementsMock } from '../mocks';
import { RequiredModuleComponent } from '../../required-module/required-module.component';
import { PipesModule } from 'src/app/shared/pipes/pipes.module';

const requirement = requirementsMock[0];

describe('[Integration] RequiredModuleComponent', () => {

  let fixture: ComponentFixture<RequiredModuleComponent>;
  let component: RequiredModuleComponent;
  let router: Router;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      imports: [
        BrowserModule,
        RouterTestingModule.withRoutes([]),
        PipesModule
      ],
      declarations: [
        RequiredModuleComponent
      ]
    }).compileComponents().then(() => {
      fixture = TestBed.createComponent(RequiredModuleComponent);
      component = fixture.componentInstance;
      router = TestBed.get(Router);
      component.requirement = requirement;
    });
  }));

  it('should display the required module Title', async () => {
    fixture.detectChanges();
    const titleElement = fixture.nativeElement.querySelector('h4');
    expect(titleElement.textContent).toContain(requirement.title);
  });

  it('should display the required module level requirement', async () => {
    const levelDescription = component.getLevelDescription( requirement.level );
    fixture.detectChanges();

    const reqElement = fixture.nativeElement.querySelector('div.content p:first-child');
    expect(reqElement.textContent).toContain(levelDescription);
  });

  it('should navigate to the required module page', async () => {
    const routerSpy = spyOn(router, 'navigate');

    fixture.detectChanges();
    const editTestImage = fixture.nativeElement.querySelector('div.required-module');
    editTestImage.dispatchEvent(new Event('click'));

    expect(routerSpy).toHaveBeenCalledWith([ '/modulo/' + requirement.moduleId ]);
  });

  it('should display the required module empty badge if there is no progress', async () => {
    const badgeSrc = component.getBadgesProgressImageSrc( requirement.progress );
    fixture.detectChanges();

    const imgElement = fixture.nativeElement.querySelector('div.content div.badge img');
    expect(imgElement.src).toContain(
      badgeSrc.split('./').join('')
    );
    cleanChanges();
  });

  it('should display the required module current badge if there is progress', async () => {
    component.requirement.progress = {
      level: 1, progress: 0.5,
      moduleId: '5c13cfc4ab1c6871c59062ad',
    };
    const badgeSrc = component.getBadgesProgressImageSrc( requirement.progress );
    fixture.detectChanges();

    const imgElement = fixture.nativeElement.querySelector('div.content div.badge img');
    expect(imgElement.src).toContain(
      badgeSrc.split('./').join('')
    );
    cleanChanges();
  });

  it('should not display the progress if there is not any', async () => {
    fixture.detectChanges();
    const levelElement = fixture.nativeElement.querySelector('p.level');
    expect(levelElement).toBeNull();
  });

  it('should display the progress if there is any', async () => {
    const levelDescription = component.getLevelDescription(1);
    component.requirement.progress = {
      level: 1, progress: 0.5,
      moduleId: '5c13cfc4ab1c6871c59062ad',
    };
    fixture.detectChanges();

    const levelElement = fixture.nativeElement.querySelector('p.level');
    expect(levelElement).not.toBeNull();
    expect(levelElement.textContent).toContain(levelDescription);
    expect(levelElement.textContent).toContain('50%');
    cleanChanges();
  });

  function cleanChanges() {
    component.requirement.progress = null;
  }

});
