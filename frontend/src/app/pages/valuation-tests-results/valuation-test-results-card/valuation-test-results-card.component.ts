import { Component, Input, Output, EventEmitter } from '@angular/core';
import { ValuationTest } from 'src/app/models/valuation-test.interface';

@Component({
  selector: 'app-settings-valuation-test-results-card',
  template: `
    <div class="test-card" >
      <div class="preview" >
        <h3>
          {{ test.title }}
        </h3>
        <p class="content" >
          {{ test.questions.length }} PERGUNTAS<br>
          <span (click)="getAnswersExcel.emit(test)" >
            exportar respostas
          </span>
        </p>
      </div>

      <div class="edit" >
        <img src="./assets/img/view.png" (click)="viewTest.emit(test)" />
      </div>
    </div>
  `,
  styleUrls: ['./valuation-test-results-card.component.scss']
})
export class SettingsValuationTestResultsCardComponent {

  @Input() test: ValuationTest;
  @Output() viewTest = new EventEmitter<ValuationTest>();
  @Output() getAnswersExcel = new EventEmitter<ValuationTest>();

}
