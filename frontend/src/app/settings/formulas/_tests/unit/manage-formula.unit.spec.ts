import { of } from 'rxjs';
import { ExcelService } from 'src/app/shared/services/excel.service';
import { ComponentFixture, async, TestBed, tick, fakeAsync } from '@angular/core/testing';
import { BrowserModule } from '@angular/platform-browser';
import { MaterialComponentsModule } from 'src/app/shared/material.module';
import { RouterTestingModule } from '@angular/router/testing';
import { HttpClientModule } from '@angular/common/http';
import { BackendService, BaseUrlService } from '@tg4/http-infrastructure/dist/src';
import { Router, ActivatedRoute } from '@angular/router';
import { ManageFormulaComponent } from '../../manage-formula/manage-formula.component';
import { SettingsFormulasService } from 'src/app/settings/_services/formulas.service';
import { formulas } from '../mocks';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { FormulaOperator, Formula } from 'src/app/models/formula.model';

describe('[Unit] ManageFormulaComponent', () => {

  let fixture: ComponentFixture<ManageFormulaComponent>;
  let component: ManageFormulaComponent;

  let service: SettingsFormulasService;
  let router: Router;
  let activatedRoute: ActivatedRoute;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      imports: [
        BrowserModule,
        MaterialComponentsModule,
        RouterTestingModule.withRoutes([]),
        HttpClientModule,
        FormsModule,
        ReactiveFormsModule
      ],
      declarations: [
        ManageFormulaComponent
      ],
      providers: [
        BackendService,
        BaseUrlService,
        ExcelService,
        SettingsFormulasService
      ]
    }).compileComponents().then(() => {
      fixture = TestBed.createComponent(ManageFormulaComponent);
      component = fixture.componentInstance;
      service = TestBed.get(SettingsFormulasService);
      router = TestBed.get(Router);
      activatedRoute = TestBed.get(ActivatedRoute);
    });
  }));

  it('should call the endpoint to retrieve the formula from the server', () => {
    spyOn(activatedRoute.snapshot.paramMap, 'get').and.callFake(() => '5c9280ee297c5b1a0cd6a924');
    const serviceSpy = spyOn(
      service, 'getFormulaById'
    ).and.callFake(() => of({ data: formulas[0] }));

    component.ngOnInit();

    expect(serviceSpy).toHaveBeenCalledWith('5c9280ee297c5b1a0cd6a924');
    expect(component.formula).toBeDefined();
    expect(component.formula.id).toBe('5c9280ee297c5b1a0cd6a924');
    expect(component.formula.formulaParts).toBeDefined();
  });

  it('should create a new formula instead of retrieve it from the server', () => {
    spyOn(activatedRoute.snapshot.paramMap, 'get').and.callFake(() => '0');
    const serviceSpy = spyOn(
      service, 'getFormulaById'
    ).and.callFake(() => of({ data: formulas[0] }));

    component.ngOnInit();

    expect(serviceSpy).not.toHaveBeenCalled();
    expect(component.formula).toBeDefined();
    expect(component.formula.formulaParts).toBeDefined();
  });

  // Testing Variables
  it('should add a variable to an empty formula', () => {
    createFormula();

    component.variable = 'CC';
    component.addVariable('CC');

    fixture.detectChanges();

    expect(component.formula.formulaParts.length).toBe(1);
    expect(component.formula.formulaParts[0].key).toBe('CC');
  });

  it('should add a variable after an operator to the formula', () => {
    createFormula();

    component.formula.formulaParts.push({
      order: 1, operator: FormulaOperator.Times
    });

    component.variable = 'CC';
    component.addVariable('CC');

    fixture.detectChanges();

    expect(component.formula.formulaParts.length).toBe(2);
    expect(component.formula.formulaParts[1].key).toBe('CC');
  });

  it('should not be able to add a variable after a number to the formula', () => {
    createFormula();

    component.formula.formulaParts.push({
      order: 1, integralNumber: 2
    });

    component.variable = 'CC';
    component.addVariable('CC');

    fixture.detectChanges();

    expect(component.formula.formulaParts.length).toBe(1);
  });

  it('should not be able to add a variable after a variable to the formula', () => {
    createFormula();

    component.formula.formulaParts.push({
      order: 1, key: 'CC'
    });

    component.variable = 'CC';
    component.addVariable('CC');

    fixture.detectChanges();

    expect(component.formula.formulaParts.length).toBe(1);
  });

  it('should not be able to add a variable after a close parenthesis to the formula', () => {
    createFormula();

    component.formula.formulaParts.push({
      order: 1, operator: FormulaOperator.CloseParenthesis
    });

    component.variable = 'CC';
    component.addVariable('CC');

    fixture.detectChanges();

    expect(component.formula.formulaParts.length).toBe(1);
  });

  // Testing Integral Numbers
  it('should add an integral number to an empty formula', () => {
    createFormula();

    component.integralNumber = 2;
    component.addIntegralNumber(2);

    fixture.detectChanges();

    expect(component.formula.formulaParts.length).toBe(1);
    expect(component.formula.formulaParts[0].integralNumber).toBe(2);
  });

  it('should add an integral number after an integral number to the formula', () => {
    createFormula();

    component.formula.formulaParts.push({
      order: 1, integralNumber: 2
    });

    component.integralNumber = 0;
    component.addIntegralNumber(0);

    fixture.detectChanges();

    expect(component.formula.formulaParts.length).toBe(2);
    expect(component.formula.formulaParts[1].integralNumber).toBe(0);
  });

  it('should add an integral number after an operator to the formula', () => {
    createFormula();

    component.formula.formulaParts.push({
      order: 1, operator: FormulaOperator.Plus
    });

    component.integralNumber = 4;
    component.addIntegralNumber(4);

    fixture.detectChanges();

    expect(component.formula.formulaParts.length).toBe(2);
    expect(component.formula.formulaParts[1].integralNumber).toBe(4);
  });

  it('should not be able to add an integral number after a variable to the formula', () => {
    createFormula();

    component.formula.formulaParts.push({
      order: 1, key: 'CC'
    });

    component.integralNumber = 1;
    component.addIntegralNumber(1);

    fixture.detectChanges();

    expect(component.formula.formulaParts.length).toBe(1);
  });

  it('should not be able to add an integral number after a close parenthesis to the formula', () => {
    createFormula();

    component.formula.formulaParts.push({
      order: 1, operator: FormulaOperator.CloseParenthesis
    });

    component.integralNumber = 3;
    component.addIntegralNumber(3);

    fixture.detectChanges();

    expect(component.formula.formulaParts.length).toBe(1);
  });

  // Testing Operators
  it('should not add an operator to an empty formula', () => {
    createFormula();

    component.operator = FormulaOperator.Plus;
    component.addOperator(FormulaOperator.Plus);

    fixture.detectChanges();

    expect(component.formula.formulaParts.length).toBe(0);
  });

  it('should not be able to add an operator after an open parenthesis to the formula', () => {
    createFormula();

    component.formula.formulaParts.push({
      order: 1, operator: FormulaOperator.OpenParenthesis
    });

    component.operator = FormulaOperator.Minus;
    component.addOperator(FormulaOperator.Minus);

    fixture.detectChanges();

    expect(component.formula.formulaParts.length).toBe(1);
  });

  it('should be able to add an operator after a close parenthesis to the formula', () => {
    createFormula();

    component.formula.formulaParts.push({
      order: 1, operator: FormulaOperator.CloseParenthesis
    });

    component.operator = FormulaOperator.Minus;
    component.addOperator(FormulaOperator.Minus);

    fixture.detectChanges();

    expect(component.formula.formulaParts.length).toBe(2);
    expect(component.formula.formulaParts[1].operator).toBe(FormulaOperator.Minus);
  });

  it('should be able to add an operator after an integral number to the formula', () => {
    createFormula();

    component.formula.formulaParts.push({
      order: 1, integralNumber: 2
    });

    component.operator = FormulaOperator.Divided;
    component.addOperator(FormulaOperator.Divided);

    fixture.detectChanges();

    expect(component.formula.formulaParts.length).toBe(2);
    expect(component.formula.formulaParts[1].operator).toBe(FormulaOperator.Divided);
  });

  it('should not be able to add an operator after an operator to the formula', () => {
    createFormula();

    component.formula.formulaParts.push({
      order: 1, operator: FormulaOperator.Divided
    });

    component.operator = FormulaOperator.Times;
    component.addOperator(FormulaOperator.Times);

    fixture.detectChanges();

    expect(component.formula.formulaParts.length).toBe(1);
  });

  it('should be able to add an operator after a variable to the formula', () => {
    createFormula();

    component.formula.formulaParts.push({
      order: 1, key: 'CC'
    });

    component.operator = FormulaOperator.Plus;
    component.addOperator(FormulaOperator.Plus);

    fixture.detectChanges();

    expect(component.formula.formulaParts.length).toBe(2);
    expect(component.formula.formulaParts[1].operator).toBe(FormulaOperator.Plus);
  });

  // Testing Formula
  it('should remove the last part of the formula', () => {
    createFormula();
    component.formula.formulaParts.push({
      order: 1, key: 'CC'
    });

    component.removeOperator();
    fixture.detectChanges();

    expect(component.formula.formulaParts.length).toBe(0);
  });

  it('should add a new formula', () => {
    const serviceSpy = spyOn(service, 'addFormula').and.callFake(() => of({ }));
    const routerSpy = spyOn(router, 'navigate');

    createFormula();
    component.formula.title = 'Título';
    component.formula.formulaParts.push({
      order: 1, key: 'CC'
    });
    component.formula.formulaParts.push({
      order: 2, operator: FormulaOperator.Plus
    });
    component.formula.formulaParts.push({
      order: 3, key: 'CCS'
    });
    fixture.detectChanges();

    component.save();
    fixture.detectChanges();

    expect(serviceSpy).toHaveBeenCalled();
    expect(routerSpy).toHaveBeenCalledWith([ 'configuracoes/formulas' ]);
  });

  it('should not add an invalid new formula', () => {
    const serviceSpy = spyOn(service, 'addFormula').and.callFake(() => of({ }));

    createFormula();
    component.formula.title = 'Título';
    component.formula.formulaParts.push({
      order: 1, key: 'CC'
    });
    component.formula.formulaParts.push({
      order: 2, operator: FormulaOperator.Plus
    });
    fixture.detectChanges();

    component.save();
    fixture.detectChanges();

    expect(serviceSpy).not.toHaveBeenCalled();
  });

  function createFormula() {
    spyOn(activatedRoute.snapshot.paramMap, 'get').and.callFake(() => '0');
    component.ngOnInit();
    fixture.detectChanges();
  }

});
