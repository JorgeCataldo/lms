import { Component, Input, EventEmitter, Output } from '@angular/core';
import { Pagination } from './pagination.interface';

@Component({
  selector: 'app-pagination',
  template: `
    <div class="pagination" >
    <span (click)="setPage(currentPage - 1)" *ngIf="currentPage != 1" >
    <i class="arrow icon-seta_direita"></i>
  </span>

      <ng-container *ngIf="pagesArray.length <= 10" >
        <span *ngFor="let page of pagesArray; let index = index"
          [ngClass]="{ 'active': currentPage === index + 1 }"
          (click)="setPage(index + 1)"
        >
          {{ index + 1 }}
        </span>
      </ng-container>

      <mat-select *ngIf="pagesArray.length > 10" [(ngModel)]="currentPage" >
        <mat-option
          *ngFor="let p of (pagesArray.length | doLoop); let index = index"
          [value]="index+1"
          (click)="setPage(index + 1)"
        >
          {{ index + 1 }}
        </mat-option>
      </mat-select>

      <span (click)="setPage(currentPage + 1)" *ngIf="currentPage != pagesArray.length" >
        <i class="arrow icon-seta_esquerda"></i>
      </span>
    </div>`,
  styleUrls: ['./pagination.component.scss']
})
export class PaginationComponent {

  @Input() set setPagination(pagination: Pagination) {
    if (pagination) {
      const pagesAmount = Math.ceil(pagination.itemsCount / pagination.pageSize);
      this.pagesArray = new Array(pagesAmount).fill(1);
    }
  }
  @Output() goToPage = new EventEmitter<number>();

  public pagesArray: Array<number> = [];
  public currentPage: number = 1;

  public setPage(pageNumber: number) {
    if (pageNumber > this.pagesArray.length)
      pageNumber = 1;
    else if (pageNumber < 1)
      pageNumber = this.pagesArray.length;

    this.currentPage = pageNumber;
    this.goToPage.emit(pageNumber);
  }

}
