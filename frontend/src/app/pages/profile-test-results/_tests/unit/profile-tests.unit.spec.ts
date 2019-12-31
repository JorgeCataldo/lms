import { of } from 'rxjs';
import { ExcelService } from 'src/app/shared/services/excel.service';
import { ProfileTestsResultsComponent } from '../../profile-tests-results.component';
import { profileTestsMock, profileTestResponsesMock } from '../mocks';
import { ComponentFixture, async, TestBed } from '@angular/core/testing';
import { BrowserModule } from '@angular/platform-browser';
import { MaterialComponentsModule } from 'src/app/shared/material.module';
import { RouterTestingModule } from '@angular/router/testing';
import { HttpClientModule } from '@angular/common/http';
import { SettingsTestResultsCardComponent } from '../../test-results-card/test-results-card.component';
import { BackendService, BaseUrlService } from '@tg4/http-infrastructure/dist/src';
import { Router } from '@angular/router';

const profileTests = profileTestsMock;

describe('[Unit] ProfileTestsComponent', () => {

  /*let fixture: ComponentFixture<ProfileTestsComponent>;
  let component: ProfileTestsComponent;

  let service: SettingsProfileTestsService;
  let excelService: ExcelService;
  let router: Router;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      imports: [
        BrowserModule,
        MaterialComponentsModule,
        RouterTestingModule.withRoutes([]),
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
      service = TestBed.get(SettingsProfileTestsService);
      excelService = TestBed.get(ExcelService);
      router = TestBed.get(Router);
    });
  }));

  it('should call the endpoint to retrieve the list of profile tests from the server', () => {
    const serviceSpy = spyOn(
      service, 'getProfileTests'
    ).and.callFake(() => of({ data: profileTests }));

    component.ngOnInit();

    expect(serviceSpy).toHaveBeenCalled();
    expect(component.tests).toBeDefined();
    expect(component.tests.length).toBe(1);
  });

  it('should navigate to create a new profile test', () => {
    const routerSpy = spyOn(router, 'navigate');

    component.createNewTest();

    expect(routerSpy).toHaveBeenCalledWith([ 'configuracoes/pesquisa-na-base/0' ]);
  });

  it('should navigate to edit the profile test', () => {
    const routerSpy = spyOn(router, 'navigate');

    component.manageTest( profileTestsMock[0] );

    expect(routerSpy).toHaveBeenCalledWith(
      [ 'configuracoes/pesquisa-na-base/' + profileTestsMock[0].id ]
    );
  });

  it('should export the test responses to excel', () => {
    const serviceSpy = spyOn(
      service, 'getAllProfileTestResponses'
    ).and.callFake(() => of({ data: profileTestResponsesMock }));

    const excelServiceSpy = spyOn(excelService, 'exportAsExcelFile');

    component.getAnswersExcel( profileTestsMock[0] );

    expect(serviceSpy).toHaveBeenCalledWith( profileTestsMock[0].id );
    expect(excelServiceSpy).toHaveBeenCalled();
  });

  it('should prepare the answers to excel export correctly', () => {
    const preparedData = component['_prepareAnswersForExport']( profileTestResponsesMock );

    expect(preparedData.length).toBe(2);
    expect(preparedData.every(d => d.hasOwnProperty('aluno'))).toBeTruthy();
    expect(preparedData.every(d => d.hasOwnProperty('matricula'))).toBeTruthy();
    expect(preparedData.every(d => d.hasOwnProperty('questao'))).toBeTruthy();
    expect(preparedData.every(d => d.hasOwnProperty('answer'))).toBeTruthy();
    expect(preparedData.every(d => d.hasOwnProperty('data'))).toBeTruthy();
  });*/

});
