import { Component } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';

@Component({
  selector: 'app-error',
  templateUrl: './error.component.html',
  styleUrls: ['./error.component.scss']
})
export class ErrorComponent {

  public route: string;

  constructor(
    _activatedRoute: ActivatedRoute,
    private _router: Router
  ) {
    _activatedRoute.queryParams.subscribe(params => {
      this.route = params.url;
    });
  }

  public goToSupport(): void {
    this._router.navigate([ 'atendimento' ]);
  }

  public goBackHome(): void {
    this._router.navigate([ 'home' ]);
  }

}
