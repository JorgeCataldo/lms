import { Component, OnInit } from '@angular/core';
import { MatSnackBar } from '@angular/material';
import { Router } from '@angular/router';
import { NotificationClass } from 'src/app/shared/classes/notification';
import { RecruitingCompanyService } from '../_services/recruiting-company.service';
import { JobPosition, AvailableCandidate, JobApplication } from 'src/app/models/previews/user-job-application.interface';

@Component({
  selector: 'app-manage-company-jobs',
  templateUrl: './manage-jobs.component.html',
  styleUrls: ['./manage-jobs.component.scss']
})
export class ManageJobsCompanyFormComponent extends NotificationClass implements OnInit {
    public openJobs: JobPosition[] = [];
    public closedJobs: JobPosition[] = [];
    public availableCandidates: AvailableCandidate[] = [];
    public numCadidatesFavorite: AvailableCandidate[] = [];
    public num_openJobs = 0;
    public num_closedJobs = 0;
    public logoUrl: string = '';

    public readonly openDisplayedColumns: string[] = [
        'priority', 'name', 'date_due', 'candidate_num', 'edit', 'arrow'
    ];

    public readonly closedDisplayedColumns: string[] = [
        'priority', 'name', 'date_due', 'candidate_num', 'arrow'
    ];

    constructor(
        protected _snackBar: MatSnackBar,
        private _router: Router,
        private _recruitingCompanyService: RecruitingCompanyService
      ) {
        super(_snackBar);
    }

    ngOnInit() {
        // this._loadCompanyInfo();
        this._loadCompanyJobs();
        this._loadCandidates();
    }

    public newJob() {
        this._router.navigate(['configuracoes/cadastro-vaga/0']);
    }

    public editJob(jobId: string) {
        this._router.navigate(['configuracoes/cadastro-vaga/' + jobId]);
    }

    private _loadCompanyInfo() {
        this._recruitingCompanyService.getRecruitingCompany().subscribe(res => {
            this.logoUrl = res.data.logoUrl;
        }, err => {
            this.notify(this.getErrorNotification(err));
        });
    }

    private _loadCompanyJobs() {
        this._recruitingCompanyService.getAllJobPositions().subscribe(res => {
            this.openJobs = res.data ? res.data.filter(x => x.status === 1) : [];
            this.num_openJobs = this.openJobs.length;
            this.closedJobs = res.data ? res.data.filter(x => x.status === 2) : [];
            this.num_closedJobs = this.closedJobs.length;
        }, err => {
            this.notify(this.getErrorNotification(err));
        });
    }

    private _loadCandidates() {
        this._recruitingCompanyService.getAvailableCandidates().subscribe(res => {
            this.availableCandidates = res.data ? res.data : [];
            this.numCadidatesFavorite = res.data ? res.data.filter(x => x.isFavorite) : [];
        }, err => {
            this.notify(this.getErrorNotification(err));
        });
    }

    public viewJobDetails(position: JobPosition) {
        this._router.navigate(['configuracoes/gerenciar-inscricoes-vagas/' + position.id]);
    }

    public searchTalents() {
        localStorage.removeItem('jobApplication');
        localStorage.removeItem('isFavorited');
        this._router.navigate(['configuracoes/buscar-talentos/']);
    }
}
