export class SelectConcept {
  public concept: string;
  public selected: boolean;

  constructor(concept: string) {
    this.concept = concept;
    this.selected = false;
  }
}
