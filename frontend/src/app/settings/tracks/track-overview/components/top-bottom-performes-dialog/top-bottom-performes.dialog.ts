import { Component, Inject } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material';
import { User } from 'src/app/models/user.model';

@Component({
  selector: 'app-top-bottom-performes-dialog',
  template: `
    <div class="concepts" >
      <ul>
        <li *ngFor="let item of data.students; let index = index" >
          <img class="photo"
            [src]="item.imageUrl || './assets/img/user-image-placeholder.png'"
          />
          <p class="item-title" >
            {{ item.name }}
          </p>
          <p class="top-performer-point"
            [ngClass]="{ 'bottom-performer-point': !data.isTop }" >
            {{ item.points }}
          </p>
        </li>
      </ul>
    </div>
    <p class="dismiss" (click)="dismiss()" >
      fechar
    </p>`,
  styleUrls: ['./top-bottom-performes.dialog.scss']
})
export class TrackOverviewTopBottomPerformesDialogComponent {

  constructor(
    public dialogRef: MatDialogRef<TrackOverviewTopBottomPerformesDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: { students: Array<User>, isTop: boolean }
  ) { }

  public dismiss(): void {
    this.dialogRef.close();
  }

}
