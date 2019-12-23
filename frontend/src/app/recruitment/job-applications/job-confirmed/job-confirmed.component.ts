import { Component, Input, Output, EventEmitter } from '@angular/core';
import { UserJobApplication } from 'src/app/models/previews/user-job-application.interface';
import { NotificationClass } from 'src/app/shared/classes/notification';
import { MatSnackBar } from '@angular/material';
import { Router } from '@angular/router';
import { RecruitingCompanyService } from '../../_services/recruiting-company.service';


@Component ({
  selector: 'app-job-confirmed',
  templateUrl: './job-confirmed.component.html',
  styleUrls: ['./job-confirmed.component.scss']

})

export class JobConfirmedComponent extends NotificationClass {

  public readonly displayedColumns: string[] = [
    'remove', 'name', 'date', 'card', 'status'
  ];

  @Input() readonly applications: UserJobApplication[] = [];
  @Input() readonly jobPositionId: string;
  @Output() removeUser = new EventEmitter<UserJobApplication>();

  constructor(
    protected _snackBar: MatSnackBar,
    private _router: Router,
    private _recruitingCompanyService: RecruitingCompanyService
  ) {
    super(_snackBar);
  }

  public getApprovalImgSrc(application: UserJobApplication, presence: boolean): string {
    if (application.approved == null)
      return presence ? './assets/img/approved-disabled.png' : './assets/img/denied-disabled.png';

    return application.approved ?
      (presence ? './assets/img/approved.png' : './assets/img/denied-disabled.png') :
      (presence ? './assets/img/approved-disabled.png' : './assets/img/denied.png');
  }

  public goToUserRecommendationCard(id: string) {
    this._router.navigate(['configuracoes/card-recomendacao/' + id]);
  }

  public manageUserApproval(application: UserJobApplication, approval: boolean) {
    if (approval) {
      this._recruitingCompanyService.approveCandidateToJobPosition(application.userId, this.jobPositionId).subscribe(res => {
        this.notify('Usuário aprovado');
        application.approved = true;
      }, err => {
        this.notify(this.getErrorNotification(err));
      });
    } else {
      this._recruitingCompanyService.rejectCandidateToJobPosition(application.userId, this.jobPositionId).subscribe(res => {
        this.notify('Usuário reprovado');
        application.approved = false;
      }, err => {
        this.notify(this.getErrorNotification(err));
      });
    }
  }
}
