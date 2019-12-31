import { Component, OnInit } from '@angular/core';
import { NotificationClass } from '../../shared/classes/notification';
import { MatSnackBar } from '@angular/material';
import { ActivatedRoute, Router } from '@angular/router';

@Component({
  selector: 'app-competence-map',
  templateUrl: './competence-map.component.html',
  styleUrls: ['./competence-map.component.scss']
})
export class CompetenceMapComponent extends NotificationClass implements OnInit {

  constructor(
    protected _snackBar: MatSnackBar,
    private _activatedRoute: ActivatedRoute,
    private _router: Router
  ) {
    super(_snackBar);
  }

  ngOnInit() {
  }

}
