import { Component, OnInit } from '@angular/core';
import { MatSnackBar } from '@angular/material';
import { NotificationClass } from '../../shared/classes/notification';
import { Router, ActivatedRoute } from '@angular/router';
import { JobPositionPriorityEnum, JobPosition, UserJobApplication } from 'src/app/models/previews/user-job-application.interface';
import { RecruitingCompanyService } from '../_services/recruiting-company.service';

@Component({
  selector: 'app-manage-job-applications',
  templateUrl: './job-applications.component.html',
  styleUrls: ['./job-applications.component.scss']
})
export class ManageJobApplicationsComponent extends NotificationClass implements OnInit {

  public readonly pageSize: number = 10;
  public jobStatus: number = 0;
  public job: JobPosition;
  public jobPositionId: string;
  public acceptedUsers: UserJobApplication[] = [];
  public pendingUsers: UserJobApplication[] = [];
  public allUsers: UserJobApplication[] = [];

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
      this.acceptedUsers = res.data.candidates.filter(x => x.accepted);
      this.pendingUsers = res.data.candidates.filter(x => !x.accepted);
      this.allUsers = res.data.candidates;
    }, err => {
      this.notify(this.getErrorNotification(err));
    });
  }

  public back() {
    this._router.navigate(['configuracoes/vagas-empresa']);
  }

  public searchJobApplicants() {
    localStorage.setItem('jobApplication', JSON.stringify(this.job));
    this._router.navigate(['configuracoes/buscar-talentos']);
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
      const index = this.allUsers.findIndex(x => x.userId === application.userId);
      if (index !== -1) {
        this.allUsers.splice(index, 1);
        this.acceptedUsers = this.allUsers.filter(x => x.accepted);
        this.pendingUsers = this.allUsers.filter(x => !x.accepted);
      }
    }, err => {
      this.notify(this.getErrorNotification(err));
    });
  }

  public approveUserJob(userId: string) {
    this._recruitingCompanyService.approveUserJobPosition(userId, this.jobPositionId).subscribe(res => {
      this._loadApplications();
    });
}

}
