export class VideoMarker {
  public concept: string;
  public position: number;
  public hovered: boolean;

  constructor(concept: string, position: number) {
    this.concept = concept;
    this.position = position;
    this.hovered = false;
  }
}
