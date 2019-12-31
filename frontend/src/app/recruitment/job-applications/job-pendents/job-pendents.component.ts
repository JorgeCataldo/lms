import { Component, Input, EventEmitter, Output } from '@angular/core';
import {  UserJobApplication } from 'src/app/models/previews/user-job-application.interface';
import { MatSnackBar } from '@angular/material';
import { RecruitingCompanyService } from '../../_services/recruiting-company.service';
import { NotificationClass } from 'src/app/shared/classes/notification';
import { AuthService } from 'src/app/shared/services/auth.service';

@Component({
    selector: 'app-job-pendents',
  templateUrl: './job-pendents.component.html',
  styleUrls: ['./job-pendents.component.scss']
})
export class JobPendentsComponent extends NotificationClass {

  public readonly displayedColumns: string[] = [
    'remove', 'name', 'date', 'null', 'status'
  ];
  @Input() readonly applications: Array<UserJobApplication> = [];
  @Output() approveUserJob = new EventEmitter<string>();
  @Output() removeUser = new EventEmitter<UserJobApplication>();

  constructor(
    protected _snackBar: MatSnackBar,
    private _authService: AuthService,
    private _recruitingCompanyService: RecruitingCompanyService
  ) {
    super(_snackBar);
}

  public checkUserId(candidateById: string): boolean {
    return this._authService.getLoggedUserId() === candidateById;
  }

}
