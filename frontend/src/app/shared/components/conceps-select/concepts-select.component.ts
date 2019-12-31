import { Component, Input, Output, EventEmitter, OnInit } from '@angular/core';
import { SelectConcept } from './concept.model';

@Component({
  selector: 'app-concepts-select',
  template: `
    <div class="concepts" >
      <p>Clique nos conceitos abaixo para adicioná-los ou removê-los</p>
      <div class="tags" >
        <div class="concept"
          *ngFor="let concept of concepts"
          [ngClass]="{ 'selected': concept.selected }"
          (click)="toggleConcept(concept)"
        >
          {{ concept.concept }}
        </div>
      </div>
    </div>`,
  styleUrls: ['./concepts-select.component.scss']
})
export class ConceptsSelectComponent implements OnInit {

  @Input() set setConcepts(concepts: Array<string>) {
    this.concepts = concepts.map(c => new SelectConcept(c));
  }
  @Input() selectedConcepts: Array<string> = [];
  @Output() updateConcepts = new EventEmitter<Array<string>>();

  public concepts: Array<SelectConcept> = [];

  ngOnInit() {
    if (this.selectedConcepts) {
      this.selectedConcepts.forEach((c) => {
        const concept = this.concepts.find(sConcept => sConcept.concept === c);
        if (concept) { concept.selected = true; }
      });
    }
  }

  public toggleConcept(concept: SelectConcept): void {
    concept.selected = !concept.selected;
    this.updateConcepts.emit(
      this.concepts.filter(c => c.selected).map(c => c.concept)
    );
  }

}
