import { Component, Input, EventEmitter, Output } from '@angular/core';
import { ProfileTest } from 'src/app/models/profile-test.interface';

@Component({
  selector: 'app-settings-test-card',
  template: `
    <div class="test-card" >
      <div class="preview" >
        <div>
          <h3>{{ test.title }}</h3>
          <p>
            {{ test.questions.length + ' perguntas' }}
          </p>
        </div>
      </div>

      <div class="edit" >
        <mat-checkbox
          [(ngModel)]="test.checked"
          (ngModelChange)="selectTest.emit(test)"
        ></mat-checkbox>
      </div>
    </div>
  `,
  styleUrls: ['./test-card.component.scss']
})
export class SettingsTestCardSelectComponent {

  @Input() test: ProfileTest;
  @Output() selectTest: EventEmitter<ProfileTest> = new EventEmitter();

}
