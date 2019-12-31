import { Component, Input, Output, EventEmitter } from '@angular/core';
import { ModulePreview } from 'src/app/models/previews/module.interface';

@Component({
  selector: 'app-settings-module-card-select',
  template: `
    <div class="module-card" >
      <img class="main-img" [src]="module.imageUrl" />

      <div class="preview" >
        <div>
          <h3>{{ module.title }}</h3>
          <p *ngIf="module.requirements && module.requirements.length > 0" >
            {{ module.requirements.length + ' pré-requisito(s) será(ão) associado(s)' }}
          </p>
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
  styleUrls: ['./module-card-select.component.scss']
})
export class SettingsModuleCardSelectComponent {

  @Input() module: ModulePreview;
  @Output() updateCollection = new EventEmitter();

}
