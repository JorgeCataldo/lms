import { Component, Output, EventEmitter, Input } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-content-footer',
  template: `
    <div class="footer inner-content" >
      <p [ngClass]="{ 'previous': canGoBack() }" (click)="goBack()" >
        {{ canGoBack() ? 'anterior' : '' }}
      </p>
      <div class="center-content">
        <button *ngIf="hasQuestions" class="btn-test"
          [disabled]="!hasFinishedRequirements || reachedMaxLevel"
          (click)="goToExam()" >
          {{
            reachedMaxLevel ? 'Nível Máximo' :
              hasFinishedRequirements ? 'Testar meu conhecimento' : 'Pré-requisitos pendentes'
          }}
        </button>
        <div class="forum" (click)="forumQuestion.emit()">
          <img src="./assets/img/question.png" />
          <p>Fórum de Dúvidas</p>
        </div>
      </div>
      <p *ngIf="!hasMultiple" ></p>
      <p class="next" *ngIf="hasMultiple"
        (click)="goToNext.emit()" >
        {{ isLast ? 'concluído' : 'próximo' }}
      </p>
    </div>`,
  styleUrls: ['./footer.component.scss']
})
export class ContentFooterComponent {

  @Input() readonly hasMultiple: boolean = true;
  @Input() readonly isFirst: boolean = false;
  @Input() readonly isLast: boolean = false;
  @Input() readonly hasQuestions: boolean = false;
  @Input() readonly contentId: string = '';
  @Input() readonly moduleId: string;
  @Input() readonly subjectId: string;
  @Input() readonly reachedMaxLevel: boolean = false;
  @Input() readonly hasFinishedRequirements: boolean = false;
  @Output() forumQuestion = new EventEmitter();
  @Output() goToPrevious = new EventEmitter();
  @Output() goToNext = new EventEmitter();

  constructor(private _router: Router) { }

  public goToExam(): void {
    this._router.navigate([ '/avaliacao/' + this.moduleId + '/' + this.subjectId ]);
  }

  public goBack(): void {
    if ( this.canGoBack() )
      this.goToPrevious.emit();
  }

  public canGoBack(): boolean {
    return this.hasMultiple && !this.isFirst;
  }
}
