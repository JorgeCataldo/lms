import { Component, Input } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { UserCareer } from '../../user-models/user-career';

@Component({
  selector: 'app-user-career',
  templateUrl: './user-career.component.html',
  styleUrls: ['./user-career.component.scss']
})
export class SettingsUserCareerComponent {

  @Input() career: UserCareer;

  constructor(
    private _activatedRoute: ActivatedRoute,
    private _router: Router) {
  }

  public editUserCareer() {
    this._router.navigate(['configuracoes/usuarios/carreira/' + this._activatedRoute.snapshot.paramMap.get('userId')]);
  }
}
