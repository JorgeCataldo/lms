import { Component, Input, TemplateRef, ViewChild } from '@angular/core';

@Component({
  selector: 'app-cards-slider',
  template: `
    <div class="cards-slider" >
      <div class="slider" #customId >
        <ng-container
          *ngTemplateOutlet="templateContent" >
        </ng-container>
      </div>
      <img class="slider-arrow back"
        *ngIf="translatedValue < 0"
        src="./assets/img/slider-arrow.png"
        (click)="slideLeft()"
      />
      <img class="slider-arrow forward"
        *ngIf="showSlider && (-translatedValue < scrollLimit)"
        src="./assets/img/slider-arrow.png"
        (click)="slideRight()"
      />
    </div>`,
  styleUrls: [ './cards-slider.component.scss' ]
})
export class CardsSliderComponent {

  @Input() set cardsContent(content: TemplateRef<any>) {
    this.templateContent = content;
    setTimeout(() => {
      this.scrollLimit =
        this.slider.nativeElement.scrollWidth -
        this.slider.nativeElement.clientWidth - 30;
    });
  }
  @Input() showSlider: boolean = true;

  @ViewChild('customId') slider;

  public templateContent: TemplateRef<any>;
  public translatedValue: number = 0;
  public scrollLimit: number = 1;

  public slideRight(): void {
    const w = window.innerWidth;
    if (-this.translatedValue < this.scrollLimit) {
      if (w <= 414) {
        this.translatedValue = this.translatedValue - 200;

        let offsett = 0;
        if (-this.translatedValue > this.scrollLimit)
          offsett = -this.translatedValue - this.scrollLimit;

        this._updateElements( offsett );
      } else {
        this.translatedValue = this.translatedValue - 100;

        let offset = 0;
        if (-this.translatedValue > this.scrollLimit)
          offset = -this.translatedValue - this.scrollLimit;

        this._updateElements( offset );

      }

    }
  }

  public slideLeft(): void {
    const w = window.innerWidth;
    if (this.translatedValue <= -100) {
      if (w <= 414) {
        this.translatedValue = this.translatedValue + 200;
        this._updateElements();
      } else {
        this.translatedValue = this.translatedValue + 100;
        this._updateElements();
      }
    }
  }

  private _updateElements(offset: number = 0) {
    const elements = Array.from( this.slider.nativeElement.children );
    elements.forEach((el: HTMLElement) => {
      el.style.position = 'relative';
      el.style.left = this.translatedValue + offset + 'px';
    });
  }

}
