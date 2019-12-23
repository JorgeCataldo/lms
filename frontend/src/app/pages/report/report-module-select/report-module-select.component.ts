import { Component, Input, Output, EventEmitter } from '@angular/core';
import { ModulePreview } from 'src/app/models/previews/module.interface';

@Component({
  selector: 'app-report-module-select',
  template: `
    <div class="module-card" >
      <img class="main-img" [src]="module.imageUrl" />

      <div class="preview" >
        <div>
          <h3>{{ module.title }}</h3>
        </div>
      </div>

      <div class="edit" >
        <mat-checkbox
          [(ngModel)]="module.checked"
          (ngModelChange)="updateCollection.emit()"
        ></mat-checkbox>
      </div>
    </div>
  `,
  styleUrls: ['./report-module-select.component.scss']
})
export class ReportModuleSelectComponent {

  @Input() module: ModulePreview;
  @Output() updateCollection = new EventEmitter();

}
