import { Component, Input, Output, EventEmitter } from '@angular/core';
import { ProfileTest } from 'src/app/models/profile-test.interface';

@Component({
  selector: 'app-settings-test-card',
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
        <img src="./assets/img/view.png" (click)="getAnswersExcel.emit(test)" />
      </div>
    </div>
  `,
  styleUrls: ['./test-results-card.component.scss']
})
export class SettingsTestResultsCardComponent {

  @Input() test: ProfileTest;
  @Output() editTest = new EventEmitter<ProfileTest>();
  @Output() deleteTest = new EventEmitter<ProfileTest>();
  @Output() getAnswersExcel = new EventEmitter<ProfileTest>();

}
