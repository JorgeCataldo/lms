import { Component, Input, Output, EventEmitter } from '@angular/core';
import { Formula } from 'src/app/models/formula.model';

@Component({
  selector: 'app-formula-card',
  template: `
    <div class="formula-card" >
      <div class="preview" >
        <h3>
          {{ formula.title }}
        </h3>
        <p class="content" >
          criada em {{ formula.createdAt | date : 'dd/MM/yyyy' }}
        </p>
      </div>

      <div class="edit" >
        <img src="./assets/img/edit.png" (click)="editFormula.emit(formula)" />
        <img style="margin-top: 24px;" src="./assets/img/trash.png" (click)="deleteFormula.emit(formula)" />
      </div>
    </div>
  `,
  styleUrls: ['./formula-card.component.scss']
})
export class FormulaCardComponent {

  @Input() formula: Formula;
  @Output() editFormula = new EventEmitter<Formula>();
  @Output() deleteFormula = new EventEmitter<Formula>();

}
