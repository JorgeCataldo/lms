import { Component, Input } from '@angular/core';
import { Suggestion } from 'src/app/models/suggestions.model';
import { UserService } from '../../_services/user.service';
import { SuggestedProductType } from 'src/app/models/enums/suggested-product-type.enum';

@Component({
  selector: 'app-suggestion-card',
  template: `
    <div class="suggested-module"
      (mouseenter)="suggestion.hovered = true"
      (mouseleave)="suggestion.hovered = false"
    >
      <img class="course-img"
        [src]="suggestion.imageUrl"
      />

      <img class="main-image" [src]="userService.getCompletedLevelImage(0, 0)" />
      <div class="content" >
        <div class="top-decoration" ></div>
        <p class="title" >
          {{ suggestion.title }}<br>
          <small>{{ getTypeDescription(suggestion.type) }}</small>
        </p>
      </div>
    </div>`,
  styleUrls: ['./suggestion-card.component.scss']
})
export class SuggestionCardComponent {

  @Input() suggestion: Suggestion;

  constructor(
    public userService: UserService
  ) { }

  public getTypeDescription(type: SuggestedProductType): string {
    switch (type) {
      case SuggestedProductType.Module:
        return 'Módulo';
      case SuggestedProductType.Event:
        return 'Evento';
      case SuggestedProductType.Track:
        return 'Trilha de Conhecimento';
      default:
        return '';
    }
  }

}
