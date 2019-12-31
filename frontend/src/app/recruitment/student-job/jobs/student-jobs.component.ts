import { Component, OnInit } from '@angular/core';
import { MatSnackBar } from '@angular/material';
import { NotificationClass } from '../../../shared/classes/notification';
import { ActivatedRoute } from '@angular/router';
import { UserJobPositionById } from 'src/app/models/previews/user-job-application.interface';
import { RecruitingCompanyService } from '../../_services/recruiting-company.service';

@Component({
  selector: 'app-student-jobs',
  templateUrl: './student-jobs.component.html',
  styleUrls: ['./student-jobs.component.scss']
})
export class StudentJobsComponent extends NotificationClass implements OnInit {

  public job: UserJobPositionById;
  public jobPositionId: string;

  constructor(
    protected _snackBar: MatSnackBar,
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
    this._recruitingCompanyService.getUserJobPositionId(this.jobPositionId).subscribe(res => {
      this.job = res.data;
      if (this.job && this.job.employment && this.job.employment.preRequirements &&
        this.job.employment.preRequirements.others && this.job.employment.preRequirements.others.length > 0) {
        if (!this.job.employment.preRequirements.complementaryInfo) {
          this.job.employment.preRequirements.complementaryInfo = [];
        }
        this.job.employment.preRequirements.others.forEach(other => {
          this.job.employment.preRequirements.complementaryInfo.push({
            name: other.name,
            done: true,
            level: other.level
          });
        });
      }
    }, err => {
      this.notify(this.getErrorNotification(err));
    });
  }

  public applyToJob() {
    this._recruitingCompanyService.applyTojob(this.job.jobPositionId).subscribe(() => {
      this.job.applied = true;
    }, err => {
      this.notify(this.getErrorNotification(err));
    });
  }
}
