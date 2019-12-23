import { Component, OnInit } from '@angular/core';
import { MatSnackBar } from '@angular/material';
import { ActivatedRoute, Router } from '@angular/router';
import { NotificationClass } from 'src/app/shared/classes/notification';
import { Formula, FormulaPart, FormulaOperator, FormulaType, FormulaVariables } from 'src/app/models/formula.model';
import { SettingsFormulasService } from '../../_services/formulas.service';

@Component({
  selector: 'app-manage-formula',
  templateUrl: './manage-formula.component.html',
  styleUrls: ['./manage-formula.component.scss']
})
export class ManageFormulaComponent extends NotificationClass implements OnInit {

  public formula: Formula;
  public formulaVariables: Array<FormulaVariables> = [];
  public selectedType: FormulaType;
  public variable: string;
  public operator: FormulaOperator;
  public integralNumber: number;
  public operators = {
    '1': '+',
    '2': '-',
    '3': '*',
    '4': '/',
    '5': '(',
    '6': ')'
  };

  constructor(
    protected _snackBar: MatSnackBar,
    private _activatedRoute: ActivatedRoute,
    private _formulasService: SettingsFormulasService,
    private _router: Router
  ) {
    super(_snackBar);
  }

  ngOnInit() {
    this._loadFormulaVariables();

    const testId = this._activatedRoute.snapshot.paramMap.get('formulaId');
    testId === '0' ?
      this.formula = new Formula() :
      this._loadFormula(testId);
  }

  public selectType(selectedType: FormulaType): void {
    this.formula.type = selectedType;
    this.formula.formulaParts = [];
  }

  public getVariablesByType(selectedType: FormulaType): Array<string> {
    const variablesType = this.formulaVariables.find(fv => fv.type === selectedType);
    return variablesType ? variablesType.variables : [];
  }

  public addVariable(key: string) {
    const isValid = this._checkVariable(this.formula.formulaParts);
    if (isValid) {
      const order = this.formula.formulaParts.length + 1;
      this.formula.formulaParts.push(
        new FormulaPart(order, null, key)
      );
    } else {
      this.notify('Variável não pode ser adicionada nesta posição. Vefifique a fórmula.');
    }
    setTimeout(() => this.variable = null);
  }

  public addIntegralNumber(integralNumber: number) {
    const isValid = this._checkNumber(this.formula.formulaParts);
    if (isValid) {
      const order = this.formula.formulaParts.length + 1;
      this.formula.formulaParts.push(
        new FormulaPart(order, null, null, integralNumber)
      );
    } else {
      this.notify('Número não pode ser adicionada nesta posição. Vefifique a fórmula.');
    }
    setTimeout(() => this.integralNumber = null);
  }

  public addOperator(operatorId: FormulaOperator) {
    const isValidOperator = this._checkOperator(this.formula.formulaParts, operatorId);
    if (isValidOperator) {
      const order = this.formula.formulaParts.length + 1;
      this.formula.formulaParts.push(
        new FormulaPart(order, operatorId)
      );
    } else {
      this.notify('Este operador não pode ser adicionado nesta posição. Vefifique a fórmula.');
    }
    setTimeout(() => this.operator = null);
  }

  public removeOperator(): void {
    const lastIndex = this.formula.formulaParts.length - 1;
    this.formula.formulaParts.splice(lastIndex, 1);
  }

  public save(): void {
    const formulaIsValid = this._checkFormula( this.formula );
    if (formulaIsValid) {
      this.formula.id ?
        this._updateFormula( this.formula ) :
        this._addFormula( this.formula );
    }
  }

  private _addFormula(formula: Formula): void {
    this._formulasService.addFormula(
      formula
    ).subscribe(() => {
      this.notify('Salvo com sucesso!');
      this._router.navigate([ 'configuracoes/formulas' ]);

    }, (error) => this.notify( this.getErrorNotification(error) ));
  }

  private _updateFormula(formula: Formula): void {
    this._formulasService.manageFormula(
      formula
    ).subscribe(() => {
      this.notify('Salvo com sucesso!');
      this._router.navigate([ 'configuracoes/formulas' ]);

    }, (error) => this.notify( this.getErrorNotification(error) ));
  }

  private _loadFormula(testId: string): void {
    this._formulasService.getFormulaById(
      testId
    ).subscribe((response) => {
      this.formula = response.data;
      this.selectedType = this.formula.type;

    }, (error) => this.notify( this.getErrorNotification(error) ));
  }

  private _loadFormulaVariables(): void {
    this._formulasService.getFormulaVariables().subscribe((response) => {
      this.formulaVariables = response.data;

    }, (err) => this.notify( this.getErrorNotification(err) ) );
  }

  private _checkVariable(formulaParts: Array<FormulaPart>): boolean {
    if (formulaParts.length === 0)
      return true;

    const lastPart = formulaParts[formulaParts.length - 1];
    return !lastPart.key &&
      !lastPart.integralNumber &&
      lastPart.operator !== FormulaOperator.CloseParenthesis;
  }

  private _checkNumber(formulaParts: Array<FormulaPart>): boolean {
    if (formulaParts.length === 0)
      return true;

    const lastPart = formulaParts[formulaParts.length - 1];
    return !lastPart.key && lastPart.operator !== FormulaOperator.CloseParenthesis;
  }

  private _checkOperator(formulaParts: Array<FormulaPart>, operator: FormulaOperator): boolean {
    if (formulaParts.length === 0)
      return operator === FormulaOperator.OpenParenthesis;

    const lastPart = formulaParts[formulaParts.length - 1];
    const lastIsKey = lastPart.key && true;
    const lastIsNumber = lastPart.integralNumber !== null && lastPart.integralNumber !== undefined;

    switch (operator) {
      case 1:
      case 2:
      case 3:
      case 4:
        return lastIsKey || lastIsNumber || (lastPart.operator === FormulaOperator.CloseParenthesis);
      case 6:
        const openParenthesisCount = this._getParenthesisCount(
          formulaParts, FormulaOperator.OpenParenthesis
        );
        const closedParenthesisCount = this._getParenthesisCount(
          formulaParts, FormulaOperator.CloseParenthesis
        );
        return openParenthesisCount > closedParenthesisCount && (
          lastIsKey || lastIsNumber || (lastPart.operator === FormulaOperator.CloseParenthesis)
        );
      case 5:
        return lastPart.operator && lastPart.operator !== FormulaOperator.CloseParenthesis;
      default:
        return false;
    }
  }

  private _checkFormula(formula: Formula): boolean {
    if (!formula || !formula.title) {
      this.notify('Preencha o título para salvar');
      return false;
    }

    if (!formula.formulaParts || formula.formulaParts.length < 2) {
      this.notify('Fórmula inválida');
      return false;
    }

    const lastPart = formula.formulaParts[formula.formulaParts.length - 1];
    if (!lastPart.key && lastPart.operator !== FormulaOperator.CloseParenthesis) {
      this.notify('Fórmula inválida');
      return false;
    }

    const openParenthesisCount = this._getParenthesisCount(
      formula.formulaParts, FormulaOperator.OpenParenthesis
    );

    const closedParenthesisCount = this._getParenthesisCount(
      formula.formulaParts, FormulaOperator.CloseParenthesis
    );

    if (openParenthesisCount !== closedParenthesisCount) {
      this.notify('Fórmula inválida. Verifique os parênteses.');
      return false;
    }

    return true;
  }

  private _getParenthesisCount(formulaParts: Array<FormulaPart>, type: FormulaOperator): number {
    return formulaParts.reduce((sum, p) => {
      if (p.operator && p.operator === type)
        sum++;
      return sum;
    }, 0);
  }
}
