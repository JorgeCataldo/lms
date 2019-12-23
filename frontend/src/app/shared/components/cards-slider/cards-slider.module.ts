import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { CardsSliderComponent } from './cards-slider.component';

@NgModule({
  declarations: [
    CardsSliderComponent
  ],
  imports: [
    BrowserModule
  ],
  exports: [
    CardsSliderComponent
  ]
})
export class CardsSliderModule { }
