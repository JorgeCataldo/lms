import { of } from 'rxjs';
import { ExcelService } from 'src/app/shared/services/excel.service';
import { SettingsProfileTestsService } from '../../../_services/profile-tests.service';
import { ComponentFixture, async, TestBed, fakeAsync } from '@angular/core/testing';
import { BrowserModule } from '@angular/platform-browser';
import { MaterialComponentsModule } from 'src/app/shared/material.module';
import { RouterTestingModule } from '@angular/router/testing';
import { HttpClientModule } from '@angular/common/http';
import { BackendService, BaseUrlService } from '@tg4/http-infrastructure/dist/src';
import { Router, ActivatedRoute } from '@angular/router';
import { ManageSuggestionComponent } from '../../manage-suggestion/manage-suggestion.component';
import { SuggestionModuleSelectComponent } from '../../manage-suggestion/module-select/module-select.component';
import { SuggestionEventSelectComponent } from '../../manage-suggestion/event-select/event-select.component';
import { SuggestionTrackSelectComponent } from '../../manage-suggestion/track-select/track-select.component';
import { SuggestionAreaToggleComponent } from '../../manage-suggestion/area-toggle/area-toggle.component';
import { FormsModule } from '@angular/forms';
import { ListSearchModule } from 'src/app/shared/components/list-search/list-search.module';
import { SettingsModulesService } from 'src/app/settings/_services/modules.service';
import { SettingsEventsService } from 'src/app/settings/_services/events.service';
import { SettingsTracksService } from 'src/app/settings/_services/tracks.service';
import { modulesMock, eventsMock, tracksMock, responseMock } from '../mocks';
import { MatDialog } from '@angular/material';
import { NotifyDialogComponent } from 'src/app/shared/dialogs/notify/notify.dialog';

describe('[Unit] ManageSuggestionComponent', () => {

  let fixture: ComponentFixture<ManageSuggestionComponent>;
  let component: ManageSuggestionComponent;

  let service: SettingsProfileTestsService;
  let modulesService: SettingsModulesService;
  let eventsService: SettingsEventsService;
  let tracksService: SettingsTracksService;
  let excelService: ExcelService;
  let router: Router;
  let activatedRoute: ActivatedRoute;
  let matDialog: MatDialog;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      imports: [
        BrowserModule,
        MaterialComponentsModule,
        RouterTestingModule.withRoutes([]),
        HttpClientModule,
        FormsModule,
        ListSearchModule
      ],
      declarations: [
        ManageSuggestionComponent,
        SuggestionModuleSelectComponent,
        SuggestionEventSelectComponent,
        SuggestionTrackSelectComponent,
        SuggestionAreaToggleComponent
      ],
      providers: [
        BackendService,
        BaseUrlService,
        ExcelService,
        SettingsProfileTestsService,
        SettingsModulesService,
        SettingsEventsService,
        SettingsTracksService
      ]
    }).compileComponents().then(() => {
      fixture = TestBed.createComponent(ManageSuggestionComponent);
      component = fixture.componentInstance;
      service = TestBed.get(SettingsProfileTestsService);
      modulesService = TestBed.get(SettingsModulesService);
      eventsService = TestBed.get(SettingsEventsService);
      tracksService = TestBed.get(SettingsTracksService);
      excelService = TestBed.get(ExcelService);
      router = TestBed.get(Router);
      activatedRoute = TestBed.get(ActivatedRoute);
      matDialog = TestBed.get(MatDialog);
    });
  }));

  it('should call the endpoint to retrieve the test response from the server', () => {
    const serviceSpy = spyResponseRequest();
    spyNgOnInitRequests();

    component.ngOnInit();

    expect(serviceSpy).toHaveBeenCalled();
    expect(component.response).toBeDefined();
    expect(component.response.id).toBeDefined(responseMock.id);
  });

  it('should call the endpoints to retrieve the products for recommendation from the server', () => {
    spyResponseRequest();

    const productsSpies = spyNgOnInitRequests();
    component.ngOnInit();

    expect(productsSpies[0]).toHaveBeenCalled();
    expect(component.modules.length).toBe(1);
    expect(productsSpies[1]).toHaveBeenCalled();
    expect(component.events.length).toBe(1);
    expect(productsSpies[2]).toHaveBeenCalled();
    expect(component.tracks.length).toBe(1);
  });

  it('should not save the answers grades if they havent been given properly', () => {
    spyResponseRequest();
    spyNgOnInitRequests();
    const gradeAnswersSpy = spyOn(service, 'gradeProfileTestAnswers').and.callFake(() => of({}));

    fixture.detectChanges();
    component.gradeProfileTestAnswers();
    expect(gradeAnswersSpy).not.toHaveBeenCalled();

    component.response.answers.forEach(a => a.grade = (a.percentage + 1));
    component.gradeProfileTestAnswers();
    expect(gradeAnswersSpy).not.toHaveBeenCalled();
    expect(component.response.finalGrade).not.toBeDefined();
  });

  it('should save the answers grades', () => {
    spyResponseRequest();
    spyNgOnInitRequests();
    const gradeAnswersSpy = spyOn(service, 'gradeProfileTestAnswers').and.callFake(() => of({}));

    fixture.detectChanges();
    component.response.answers.forEach(a => a.grade = a.percentage);
    component.gradeProfileTestAnswers();

    expect(gradeAnswersSpy).toHaveBeenCalledWith(
      component.response.id, component.response.answers
    );
    expect(component.response.finalGrade).toBeDefined();
  });

  it('should not allow to suggest repeated products', () => {
    spyResponseRequest();
    spyNgOnInitRequests();
    const dialogSpy = spyOn(matDialog, 'open');
    const recommendSpy = spyOn(service, 'suggestProducts').and.callFake(() => of({}));

    fixture.detectChanges();
    component.selectedModules.push({'id': '5c646f1ce15797206584bd4d'} as any);
    component.filterSugestions();

    expect(recommendSpy).not.toHaveBeenCalled();
    expect(dialogSpy).toHaveBeenCalledWith(
      NotifyDialogComponent, {
        width: '400px',
        data: { message: 'Há produtos recomendados já associados ao aluno; <br><br>Novo Módulo!' }
      }
    );
  });

  it('should call the endpoint to suggest the products', () => {
    spyResponseRequest();
    spyNgOnInitRequests();
    const recommendSpy = spyOn(service, 'suggestProducts').and.callFake(() => of({}));
    const routerSpy = spyOn(router, 'navigate');

    fixture.detectChanges();
    component.selectedModules.push({'id': '5c646f1ce15797277784bd4d'} as any);
    component.filterSugestions();

    expect(recommendSpy).toHaveBeenCalled();
    expect(routerSpy).toHaveBeenCalledWith([ 'configuracoes/recomendacoes-produtos' ]);
  });

  it('should export the test responses to excel', () => {
    const excelServiceSpy = spyOn(excelService, 'exportAsExcelFile');

    component.response = responseMock;
    component.exportAnswers();

    expect(excelServiceSpy).toHaveBeenCalledWith(
      responseMock.answers,
      'Resposta - ' + responseMock.testTitle
    );
  });

  function spyResponseRequest(): jasmine.Spy {
    spyOn(activatedRoute.snapshot.paramMap, 'get').and.callFake(() => '');
    return spyOn(
      service, 'getProfileTestResponseById'
    ).and.callFake(() => of({ data: responseMock }));
  }

  function spyNgOnInitRequests(): Array<jasmine.Spy> {
    const modulesSpy = spyOn(
      modulesService, 'getPagedFilteredModulesList'
    ).and.callFake(() => of({ data: { 'modules': modulesMock } }));

    const eventsSpy = spyOn(
      eventsService, 'getPagedFilteredEventsList'
    ).and.callFake(() => of({ data: { 'events': eventsMock } }));

    const tracksSpy = spyOn(
      tracksService, 'getPagedFilteredTracksList'
    ).and.callFake(() => of({ data: { 'tracks': tracksMock } }));

    return [ modulesSpy, eventsSpy, tracksSpy ];
  }
});
