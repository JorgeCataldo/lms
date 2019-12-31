import { Component, Input } from '@angular/core';
import { Track } from 'src/app/models/track.model';
import { Warning } from 'src/app/models/track-overview.interface';
import { Router } from '@angular/router';

@Component({
  selector: 'app-track-overview-warnings',
  template: `
    <div class="warnings" >
      <p class="warnings-title" >
        AVISOS
      </p>
      <ul *ngIf="warnings && warnings.length > 0" >
        <li *ngFor="let warn of warnings" (click)="goToWarning(warn)" >
          {{ warn.text }}
          <div>
            <p class="due" *ngIf="warn.dueTo" >
              Prazo:<br>
              {{ warn.dueTo | date : 'dd/MM/yyyy' }}
            </p>
            <i class="icon-seta_esquerda"></i>
          </div>
        </li>
      </ul>
      <p class="no-warnings"  *ngIf="warnings && warnings.length === 0" >
        Por enquanto, nada por aqui!
      </p>
    </div>`,
  styleUrls: ['./track-warnings.component.scss']
})
export class TrackOverviewWarningsComponent {

  @Input() readonly warnings: Array<Warning>;

  constructor(
    private _router: Router
  ) { }

  public goToWarning(warning: Warning) {
    if (warning.redirectTo)
      this._router.navigate([ warning.redirectTo ]);
  }

}
