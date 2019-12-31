import { ProfileTestsResultsComponent } from '../../profile-tests-results.component';
import { ComponentFixture, async, TestBed } from '@angular/core/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { BrowserModule, By } from '@angular/platform-browser';
import { profileTestsMock } from '../mocks';
import { MaterialComponentsModule } from 'src/app/shared/material.module';
import { SettingsTestResultsCardComponent } from '../../test-results-card/test-results-card.component';
import { SettingsProfileTestsService } from 'src/app/settings/_services/profile-tests.service';
import { BackendService, BaseUrlService } from '@tg4/http-infrastructure/dist/src';
import { HttpClientModule } from '@angular/common/http';
import { ExcelService } from 'src/app/shared/services/excel.service';

describe('[Integration] ProfileTestsResultsComponent', () => {

  /*let fixture: ComponentFixture<ProfileTestsComponent>;
  let component: ProfileTestsComponent;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      imports: [
        BrowserModule,
        MaterialComponentsModule,
        RouterTestingModule,
        HttpClientModule
      ],
      declarations: [
        ProfileTestsComponent,
        SettingsTestCardComponent
      ],
      providers: [
        BackendService,
        BaseUrlService,
        ExcelService,
        SettingsProfileTestsService
      ]
    }).compileComponents().then(() => {
      fixture = TestBed.createComponent(ProfileTestsComponent);
      component = fixture.componentInstance;
    });
  }));

  it('should have a new profile test button', () => {
    const createNewEventSpy = spyOn(component, 'createNewTest');
    component.tests = profileTestsMock;

    fixture.detectChanges();
    const newEventButton = fixture.nativeElement.querySelector('div.header .actions button');
    newEventButton.dispatchEvent(new Event('click'));

    expect(createNewEventSpy).toHaveBeenCalled();
  });

  it('should display the loaded tests', () => {
    component.tests = profileTestsMock;

    fixture.detectChanges();
    const testsCards = fixture.debugElement.queryAll(
      By.css('app-settings-test-card')
    );

    expect(testsCards.length).toBe(1);
  });

  it('should present a message if no test was created yet', () => {
    component.tests = [];

    fixture.detectChanges();
    const noTestMessage = fixture.nativeElement.querySelector('p');

    expect(noTestMessage).toBeDefined();
    expect(noTestMessage.textContent).toContain('Ainda não há nenhum Teste de Perfil criado.');
  });*/
});
