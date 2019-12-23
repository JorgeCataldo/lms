import { Component, Inject } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material';
import { WrongConcept } from 'src/app/models/track-overview.interface';

@Component({
  selector: 'app-wrong-concepts-dialog',
  template: `
    <div class="concepts" >
      <ul>
        <li *ngFor="let item of concepts; let index = index" >
          <p class="concept" >
            {{ item.concept }}<br>
            <small>{{ item.moduleName }}</small>
          </p>
          <p class="count" >
            {{ item.count }}
          </p>
        </li>
      </ul>
    </div>
    <p class="dismiss" (click)="dismiss()" >
      fechar
    </p>`,
  styleUrls: ['./wrong-concepts.dialog.scss']
})
export class TrackOverviewWrongConceptsDialogComponent {

  constructor(
    public dialogRef: MatDialogRef<TrackOverviewWrongConceptsDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public concepts: Array<WrongConcept>
  ) { }

  public dismiss(): void {
    this.dialogRef.close();
  }

}
