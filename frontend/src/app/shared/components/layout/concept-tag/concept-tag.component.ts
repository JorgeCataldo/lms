import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-concept-tag',
  template: `
    <p class="concept-tag" [style.color]="color" >
      {{ concept }}
    </p>`,
  styleUrls: ['./concept-tag.component.scss']
})
export class ConceptTagComponent {

  @Input() concept: string;
  @Input() color: string = '#3c3c3c';

}
