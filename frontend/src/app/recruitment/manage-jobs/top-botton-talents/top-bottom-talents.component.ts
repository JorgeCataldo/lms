import { Component, Input } from '@angular/core';
import { MatDialog } from '@angular/material';
import { AvailableCandidate } from 'src/app/models/previews/user-job-application.interface';
import { Router } from '@angular/router';


@Component({
  selector: 'app-top-bottom-talents',
  template: `
    <div class="students" >
      <p class="title" >
        <span style="font-size: 50px">{{candidates.length}}</span><br />
        TALENTOS <span style="font-size: 14px; font-weight: 700;">FAVORITOS</span>
      </p>
      <p class="no-students" *ngIf="!candidates || candidates.length === 0" >
        Ainda não há candidatos
        <p *ngIf="!candidates || candidates.length === 0">
        como favoritos.
        <p *ngIf="!candidates || candidates.length === 0">
        <button class="btn-test buttonSearch cursor-pointer" (click)="searchTalents()">
            Buscar Talento
        </button>
      </p>
      <ul *ngIf="candidates" >
        <li *ngFor="let item of candidates; let index = index">
          <img class="photo"
            [src]=" item.imageUrl || './assets/img/user-image-placeholder.png'"
          />
          <p class="item-title" >
            {{ item.name }}
          </p>
        </li>
      </ul>
      <div class="action" *ngIf="candidates && candidates.length > 0" >
        <button class="cursor-pointer" (click)="viewAllTalents()" >
          VER TUDO
        </button>
      </div>
    </div>`,
  styleUrls: ['./top-bottom-talents.component.scss']
})
export class TrackOverviewTopBottomTalentsComponent {

  public candidates: AvailableCandidate[] = [];


  @Input() set talents(ptalents: AvailableCandidate[]) {
    this.candidates = ptalents.slice(0, 9);
  }

  constructor(
    private _dialog: MatDialog,
    private _router: Router
  ) { }

  public viewAllTalents(): void {
    localStorage.removeItem('jobApplication');
    localStorage.setItem('isFavorited', 'true');
    this._router.navigate(['configuracoes/buscar-talentos']);
  }

  public searchTalents() {
    localStorage.removeItem('isFavorited');
    this._router.navigate(['configuracoes/buscar-talentos/']);
  }
}
