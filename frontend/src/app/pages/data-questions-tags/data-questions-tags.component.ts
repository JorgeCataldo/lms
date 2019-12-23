import { Component, OnInit } from '@angular/core';
import { NotificationClass } from '../../shared/classes/notification';
import { MatSnackBar } from '@angular/material';
import { ActivatedRoute, Router } from '@angular/router';

@Component({
  selector: 'app-data-questions-tags',
  templateUrl: './data-questions-tags.component.html',
  styleUrls: ['./data-questions-tags.component.scss']
})
export class DataQuestionsTagsComponent extends NotificationClass implements OnInit {

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
