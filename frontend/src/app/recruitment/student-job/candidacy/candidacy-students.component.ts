import { Component, OnInit } from '@angular/core';
import { MatSnackBar } from '@angular/material';
import { Router, ActivatedRoute } from '@angular/router';
import { NotificationClass } from 'src/app/shared/classes/notification';
import { RecruitingCompanyService } from '../../_services/recruiting-company.service';
import { JobPosition, AvailableCandidate, UserJobApplication } from 'src/app/models/previews/user-job-application.interface';
import { UserStatusEnum } from '../../../models/enums/user-status.enum';
import { UserTab } from './user-tabs/user-tab.model';
import { StudentStatusJobEnum } from 'src/app/models/enums/student-status-jobs.enum';
import { UserJobsPosition, UserApplications, UserJobsNotifications } from 'src/app/models/user-jobs-position.interface';
import { AuthService } from 'src/app/shared/services/auth.service';

@Component({
  selector: 'app-candidacy-students',
  templateUrl: './candidacy-students.component.html',
  styleUrls: ['./candidacy-students.component.scss']
})
export class CandidacyStudentsComponent extends NotificationClass implements OnInit {

    public totalOpenJobs: number = 0;
    public userJob: UserApplications[] = [];
    public pendingUsers: UserApplications[] = [];
    public selectedTabIndex: number = 0;
    public jobPositionId: string;
    public userId: string;
    public notifications: UserJobsNotifications[] = [];

    public userAccpet: number = 0;
    public notUserAccpet: number = 0;

    public tabs: Array<UserTab> = [
        new UserTab('OPORTUNIDADES', 'QUE É CANDIDATO', '#05e295', '#05e295',
            UserStatusEnum.Active, 0),
        new UserTab('OPORTUNIDADES', 'PENDENTES DE AVALIAÇÂO', 'var(--danger-color)', 'var(--danger-color)',
            UserStatusEnum.Blocked, 0)
    ];

    public readonly candidateDisplayedColumns: string[] = [
        'job_name', 'conclusion_date', 'result', 'null'
    ];

    public readonly waitApproveDisplayedColumns: string[] = [
        'job_name', 'company', 'action', 'null'
    ];

    constructor(
        protected _snackBar: MatSnackBar,
        private _router: Router,
        private _authService: AuthService,
        private _recruitingCompanyService: RecruitingCompanyService
      ) {
        super(_snackBar);
    }

    ngOnInit() {
        this.userId = this._authService.getLoggedUserId();
        this._loadUserJobPosition();
        // this._loadNotifications();
    }

    public selectTab(index: number) {
        this.selectedTabIndex = index;
    }

    private _loadUserJobPosition() {
        this._recruitingCompanyService.getUserJobPosition().subscribe(res => {
            this.totalOpenJobs = res.data.totalOpenJobs;
            this.userJob = res.data.userApplications.filter(x => x.accepted);
            this.pendingUsers = res.data.userApplications.filter(x => !x.accepted);
            this.tabs[0].count = this.userJob.length;
            this.tabs[1].count = this.pendingUsers.length;
            this.userAccpet = this.userJob.length;
            this.notUserAccpet = this.pendingUsers.length;
        }, err => {
            this.notify(this.getErrorNotification(err));
        });
    }

    public checkUserId(candidateById: string): boolean {
        return this.userId === candidateById;
    }

    public approveUserJob(jobPositionId: string) {
        this._recruitingCompanyService.approveUserJobPosition(this.userId, jobPositionId).subscribe(res => {
            this._loadUserJobPosition();
        });
    }

    public notAccept() {
    if (this.selectedTabIndex === 1 && this.notUserAccpet === 0 ) {
            return true;
        } else {
            return false;
        }
    }

    public Accept() {
        if (this.selectedTabIndex === 0 && this.userAccpet === 0 ) {
            return true;
        } else {
            return false;
        }
    }

    public viewJobDetails(jobPositionId: string) {
        this._router.navigate(['gerenciar-vaga-alunos/' + jobPositionId]);
    }

    public newJob() {
        this._router.navigate(['buscar-vagas-aluno/']);
    }

    public searchTalents() {
        localStorage.removeItem('jobApplication');
        this._router.navigate(['buscar-vagas-aluno/']);
    }

    private _loadNotifications() {
        this._recruitingCompanyService.getUserJobNotifications().subscribe(res => {
            this.notifications = res.data;
        }, err => {
            this.notify(this.getErrorNotification(err));
        });
    }
}
