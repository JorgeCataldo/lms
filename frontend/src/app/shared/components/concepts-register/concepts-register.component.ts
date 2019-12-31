import { Component, Input, Output, EventEmitter } from '@angular/core';

@Component({
  selector: 'app-concepts-register',
  template: `
    <div class="concepts" >
      <img src="./assets/img/search-gray.png" />
      <input type="text" [placeholder]="placeholder"
        (keyup.enter)="addConcept($event)"
      />

      <div class="tags" >
        <div class="concept" *ngFor="let concept of concepts; let index = index" >
          {{ concept }}
          <img src="./assets/img/close.png"
            (click)="removeConcept(index, concept)"
          />
        </div>
      </div>
    </div>`,
  styleUrls: ['./concepts-register.component.scss']
})
export class ConceptsRegisterComponent {

  @Input() placeholder?: string = 'Conceitos';
  @Input() concepts: Array<string>;
  @Output() updateConcepts = new EventEmitter<Array<string>>();

  public addConcept(event): void {
    const concept: string = event.target.value;
    if (concept && concept.trim() !== '') {
      (this.concepts = this.concepts || []).push( concept );
      event.target.value = '';
    }
    this.updateConcepts.emit( this.concepts );
  }

  public removeConcept(index: number): void {
    this.concepts.splice(index, 1);
    this.updateConcepts.emit( this.concepts );
  }

}
