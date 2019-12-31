import { Component, OnInit } from '@angular/core';
import { JobPosition, JobPositionPriorityEnum, UserJobApplication } from 'src/app/models/previews/user-job-application.interface';
import { Router, ActivatedRoute } from '@angular/router';
import { RecruitingCompanyService } from 'src/app/recruitment/_services/recruiting-company.service';
import { NotificationClass } from 'src/app/shared/classes/notification';
import { MatSnackBar } from '@angular/material';

@Component ({
    selector: 'app-rec-confirmed',
  templateUrl: './rec-confirmed.component.html',
  styleUrls: ['./rec-confirmed.component.scss']

})

export class RecConfirmedComponent extends NotificationClass implements OnInit {

  public readonly pageSize: number = 10;
  public readonly displayedColumns: string[] = [
    'remove', 'name', 'date', 'card', 'status'
  ];
  public jobStatus: number = 0;
  public job: JobPosition;
  public applications: Array<UserJobApplication> = [];
  public applicationsCount: number = 0;
  public jobPositionId: string;

  constructor(
    protected _snackBar: MatSnackBar,
    private _router: Router,
    private _activatedRoute: ActivatedRoute,
    private _recruitingCompanyService: RecruitingCompanyService
  ) {
    super(_snackBar);
  }

  ngOnInit() {
    this.jobPositionId = this._activatedRoute.snapshot.paramMap.get('jobPositionId');
    this._loadApplications();
  }

  private _loadApplications() {
    this._recruitingCompanyService.getJobPosition(this.jobPositionId).subscribe(res => {
      this.job = res.data;
      this.jobStatus = res.data.status;
      this.applications = res.data.candidates;
      this.applicationsCount = res.data.candidates.length;
    }, err => {
      this.notify(this.getErrorNotification(err));
    });
  }

  public back() {
    this._router.navigate(['configuracoes/vagas-empresa']);
  }

  public getApprovalImgSrc(application: UserJobApplication, presence: boolean): string {
    if (application.approved == null)
      return presence ? './assets/img/approved-disabled.png' : './assets/img/denied-disabled.png';

    return application.approved ?
      (presence ? './assets/img/approved.png' : './assets/img/denied-disabled.png') :
      (presence ? './assets/img/approved-disabled.png' : './assets/img/denied.png');
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

  public searchJobApplicants() {
    localStorage.setItem('jobApplication', JSON.stringify(this.job));
    this._router.navigate(['configuracoes/buscar-talentos']);
  }

  public goToUserRecommendationCard(id: string) {
    this._router.navigate(['configuracoes/card-recomendacao/' + id]);
  }

  public changeJobStatus() {
    this.job.status = this.jobStatus;
    this._recruitingCompanyService.updateJobPositionStatus(this.job).subscribe(() => {
      this.notify('Status atualizado com sucesso');
    }, err => {
      this.notify(this.getErrorNotification(err));
    });
  }

  public getPriorityStatus() {
    if (this.job && this.job.priority === JobPositionPriorityEnum.Low) {
      return 'Baixa';
    } else if (this.job && this.job.priority === JobPositionPriorityEnum.Medium) {
      return 'Media';
    } else if (this.job && this.job.priority === JobPositionPriorityEnum.High) {
      return 'Alta';
    } else {
      return '-';
    }
  }

  public removeUser(application: UserJobApplication) {
    this._recruitingCompanyService.deleteCandidateJobPosition(this.jobPositionId, application.userId).subscribe(res => {
      this.notify('Usuário deletado com sucesso');
      const index = this.applications.indexOf(application);
      this.applications.splice(index, 1);
      this.applicationsCount = this.applicationsCount - 1;
      const newObject = JSON.stringify(this.applications);
      this.applications = [];
      this.applications = JSON.parse(newObject);
    }, err => {
      this.notify(this.getErrorNotification(err));
    });
  }
}
