import { Component, Inject } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material';

@Component({
  selector: 'app-badges-dialog',
  template: `
    <div class="concepts" >
      <p class="concept-title">{{ data.title }}</p>
      <ul *ngFor="let moduleBadge of data.modulesBadge">
        <p class="list-title">
          {{ moduleBadge.moduleName }}
        </p>
        <li *ngFor="let item of moduleBadge.students">
          <img class="photo"
            [src]="item.imageUrl || './assets/img/user-image-placeholder.png'"
          />
          <p class="item-title" >
            {{ item.userName || item.name }}
          </p>
          <p class="point">
            {{ item.points }}
          </p>
        </li>
      </ul>
    </div>
    <p class="dismiss" (click)="dismiss()" >
      fechar
    </p>`,
  styleUrls: ['./badges.dialog.scss']
})
export class TrackOverviewBadgesDialogComponent {

  constructor(
    public dialogRef: MatDialogRef<TrackOverviewBadgesDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: { title: string, modulesBadge: any[] }
  ) { }

  public dismiss(): void {
    this.dialogRef.close();
  }

}
