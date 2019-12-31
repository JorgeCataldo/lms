import { Component, Input } from '@angular/core';
import { WrongConcept } from 'src/app/models/track-overview.interface';
import { TrackOverviewWrongConceptsDialogComponent } from '../wrong-concepts-dialog/wrong-concepts.dialog';
import { MatDialog } from '@angular/material';

@Component({
  selector: 'app-track-overview-wrong-concepts',
  template: `
    <div class="concepts" >
      <p class="title" >
        CONCEITOS COM MAIS ERROS
      </p>
      <ul>
        <li *ngFor="let item of getChoppedConcepts(); let index = index" >
          <p class="concept" >
            {{ item.concept }}<br>
            <small>{{ item.moduleName }}</small>
          </p>
          <p class="count" >
            {{ item.count }}
          </p>
        </li>
      </ul>
      <p class="no-concepts" *ngIf="concepts && concepts.length === 0" >
        Ainda não há conceitos errados para este {{ emptyStateEntity }}.
      </p>
      <div class="action" *ngIf="showAll && concepts && concepts.length > 0" >
        <button (click)="viewAllConcepts()" >
          VER TODOS OS CONCEITOS
        </button>
      </div>
    </div>`,
  styleUrls: ['./wrong-concepts.component.scss']
})
export class TrackOverviewWrongConceptsComponent {

  @Input() readonly concepts: Array<WrongConcept>;
  @Input() readonly showAll: boolean = true;
  @Input() readonly emptyStateEntity: string = 'aluno';

  constructor(
    private _dialog: MatDialog
  ) { }

  public getChoppedConcepts(): Array<WrongConcept> {
    return this.concepts ? this.concepts.slice(0, 8) : [];
  }

  public viewAllConcepts(): void {
    this._dialog.open(TrackOverviewWrongConceptsDialogComponent, {
      width: '600px',
      data: this.concepts
    });
  }

}
