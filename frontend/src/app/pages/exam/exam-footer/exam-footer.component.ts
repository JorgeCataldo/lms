import { Component, Input, EventEmitter, Output } from '@angular/core';
import { Progress } from '../../../models/shared/progress.interface';
import { UserService } from '../../_services/user.service';
// ROLLBACK BDQ
@Component({
  selector: 'app-exam-footer',
  template: `
    <div class="exam-footer" >
      <div class="progress" [style.color]="getProgressBarColor()" *ngIf="progress">
        <div class="bar" >
          <app-progress-bar
            [roundedBorder]="true"
            [completedPercentage]="progress.progress*100"
            [height]="29"
            [color]="getProgressBarColor()"
            fillingColor="#C3C3C3"
          ></app-progress-bar>
        </div>
        <img [src]="getProgressIconSrc()" />
        {{ 100 - (progress.progress*100) | number:'1.0-0' }}% para completar nível {{ levels[progress.level] }}
        <!--{{progress.progress * 100 | number:'1.0-0' }}% para alcançar o nível {{ levels[progress.level] }}-->
      </div>

      <button
        class="btn-test"
        (click)="backToModule.emit()"
      > Voltar para o módulo
      </button>

      <a *ngIf="hasMoreQuestions"
        [ngClass]="{ 'disabled': !hasAnswered }"
        (click)="next()"
      > próxima </a>
    </div>`,
  styleUrls: ['./exam-footer.component.scss']
})
export class ExamFooterComponent {

  @Input() readonly progress: Progress;
  @Input() readonly levels: any;
  @Input() readonly hasAnswered: boolean = false;
  @Input() readonly hasMoreQuestions: boolean = false;
  @Output() goToNextQuestion = new EventEmitter();
  @Output() backToModule = new EventEmitter();

  constructor(private _userService: UserService) { }

  public next(): void {
    if (this.hasAnswered)
      this.goToNextQuestion.emit();
  }

  public getProgressBarColor(): string {
    return this._userService.getLevelColor(this.progress.level);
  }

  public getProgressIconSrc(): string {
    return this._userService.getLevelGreyImage(this.progress.level);
  }

}
