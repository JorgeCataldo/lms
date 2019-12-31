import { Component, Input, Output, EventEmitter } from '@angular/core';
import { ValuationTest } from 'src/app/models/valuation-test.interface';

@Component({
  selector: 'app-settings-valuation-test-card',
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
        <img style="margin-top: 15px;" src="./assets/img/edit.png" (click)="editTest.emit(test)" />
        <img style="margin-top: 12px;" src="./assets/img/trash.png" (click)="deleteTest.emit(test)" />
      </div>
    </div>
  `,
  styleUrls: ['./valuation-test-card.component.scss']
})
export class SettingsValuationTestCardComponent {

  @Input() test: ValuationTest;
  @Output() viewTest = new EventEmitter<ValuationTest>();
  @Output() editTest = new EventEmitter<ValuationTest>();
  @Output() deleteTest = new EventEmitter<ValuationTest>();
  @Output() getAnswersExcel = new EventEmitter<ValuationTest>();

}
