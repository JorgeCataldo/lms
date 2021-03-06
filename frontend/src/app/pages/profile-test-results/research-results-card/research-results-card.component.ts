import { Component, Input, Output, EventEmitter } from '@angular/core';
import { Activation } from 'src/app/models/activation.model';

@Component({
  selector: 'app-settings-research-card',
  template: `
    <div class="research-card" >
      <div class="preview" >
        <h3>
          {{ research.title }}
        </h3>
        <p class="content" >
          TIPO -
          <span *ngIf="research.type === 1"> NPS</span>
          <span *ngIf="research.type === 2"> PADRÃO</span>
          <br>
          <span class="click" (click)="viewResearch.emit(research)" >
            visualizar respostas
          </span>
        </p>
      </div>

      <div class="edit" >
        <img src="./assets/img/view.png" (click)="viewResearch.emit(research)" />
      </div>
    </div>
  `,
  styleUrls: ['./research-results-card.component.scss']
})
export class SettingsResearchResultsCardComponent {

  @Input() research: Activation;
  @Output() viewResearch = new EventEmitter<Activation>();
  @Output() editResearch = new EventEmitter<Activation>();
  @Output() deleteResearch = new EventEmitter<Activation>();
}
