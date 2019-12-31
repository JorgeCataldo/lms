import { Component, OnInit } from '@angular/core';
import { MatSnackBar, MatDialog } from '@angular/material';
import { SettingsUsersService } from '../../_services/users.service';
import { ActivatedRoute, Router } from '@angular/router';
import { NotificationClass } from '../../../shared/classes/notification';
import { UserRecommendation, UserSkill } from 'src/app/models/user-recommendation.model';
import { SharedService } from 'src/app/shared/services/shared.service';
import { Level } from 'src/app/models/shared/level.interface';
import { RecruitingCompanyService } from 'src/app/recruitment/_services/recruiting-company.service';
import { AuthService } from 'src/app/shared/services/auth.service';
import { JobApplication } from 'src/app/models/previews/user-job-application.interface';
import { JobRecommendationDialogComponent } from './job-recommendation-dialog/job-recommendation.dialog';

@Component({
  selector: 'app-user-details',
  templateUrl: './user-recommendation.component.html',
  styleUrls: ['./user-recommendation.component.scss']
})
export class SettingsUserRecommendationComponent extends NotificationClass implements OnInit {

  public user: UserRecommendation;
  public userId: string;
  public currentUserRole: string;
  public userSkills: UserSkill[] = [];
  public levels: Array<Level> = [];
  public canFavorite: boolean = false;
  public isCurrentUserCard: boolean = false;
  private _savingFavorite: boolean = false;

  public fromJobApplication: boolean = false;
  public jobApplication: JobApplication;

  constructor(
    protected _snackBar: MatSnackBar,
    private _usersService: SettingsUsersService,
    private _activatedRoute: ActivatedRoute,
    private _sharedService: SharedService,
    private _recruitmentService: RecruitingCompanyService,
    private _router: Router,
    private _authService: AuthService,
    private _dialog: MatDialog
  ) {
    super(_snackBar);
  }

  ngOnInit() {
    this.userId = this._activatedRoute.snapshot.paramMap.get('userId');
    this._loadLevels();
    this._loadUser(this.userId);
    this._loadUserSkills(this.userId);
    this.currentUserRole = this._authService.getLoggedUserRole();
    this.jobApplication = JSON.parse(localStorage.getItem('jobApplication'));
    if (this.currentUserRole === 'Recruiter' || this.currentUserRole === 'HumanResources') {
      this.fromJobApplication = true;
    }
  }

  public canApproveProfile(): boolean {
    const hasPermission = ['Admin', 'HumanResources', 'Secretary', 'Recruiter'];
    return hasPermission.includes( this.currentUserRole );
  }

  public allowRecommendation(allow: boolean): void   {
    if (!this.isCurrentUserCard) return;

    this._usersService.allowRecommendation(
      this.userId, allow
    ).subscribe(() => {
      this.notify('Salvo com sucesso');

    }, (error) => this.notify( this.getErrorNotification(error) ));
  }

  public allowSecretaryRecommendation(allow: boolean): void   {
    if (!this.canApproveProfile()) return;

    this._usersService.allowSecretaryRecommendation(
      this.userId, allow
    ).subscribe(() => {
      this.notify('Salvo com sucesso');

    }, (error) => this.notify( this.getErrorNotification(error) ));
  }

  public toggleUserToFavorites(): void {
    if (!this._savingFavorite) {
      this._savingFavorite = true;
      this.user.isFavorite ?
        this.removeUserToFavorites() :
        this.addUserToFavorites();
    }
  }

  public addUserToFavorites(): void {
    this.user.isFavorite = true;
    this._recruitmentService.addRecruitmentFavorite(
      this.userId
    ).subscribe(() => {
      this.notify('Usuário adicionado aos favoritos com sucesso');
      this._savingFavorite = false;

    }, (err) => {
      this.notify( this.getErrorNotification(err) );
      this.user.isFavorite = false;
      this._savingFavorite = false;
    });
  }

  public removeUserToFavorites(): void {
    this.user.isFavorite = false;
    this._recruitmentService.removeRecruitmentFavorite(
      this.userId
    ).subscribe(() => {
      this.notify('Usuário removido dos favoritos com sucesso');
      this._savingFavorite = false;

    }, (err) => {
      this.notify( this.getErrorNotification(err) );
      this.user.isFavorite = true;
      this._savingFavorite = false;
    });
  }

  private _loadUser(userId: string): void {
    this._usersService.getUserRecommendation(
      userId
    ).subscribe((response) => {
      this.user = response.data;
      console.log('this.user -> ', this.user);
    }, (error) => {
      this.notify(
        this.getErrorNotification(error)
      );
    });
  }

  private _loadUserSkills(userId: string): void {
    this._usersService.getUserSkills(
      userId
    ).subscribe((response) => {
      this.userSkills = response.data;
      const currentUserId = this._authService.getLoggedUserId();
      this.isCurrentUserCard = currentUserId === userId;

    }, (error) => this.notify( this.getErrorNotification(error) ));
  }

  private _loadLevels(): void {
    this._sharedService.getLevels(true).subscribe((response) => {
      this.levels = response.data;
    }, () => this.notify('Ocorreu um erro, por favor tente novamente mais tarde'));
  }

  public addApplicant() {
    const candidates = [{'UserId': this.userId, 'UserName': this.user.userInfo.name}];

    if (this.jobApplication) {
      this._recruitmentService.addCandidateToJobPosition(candidates, this.jobApplication.id).subscribe(() => {
        this._router.navigate(['configuracoes/gerenciar-inscricoes-vagas/' + this.jobApplication.id]);
      }, err => {
        this.notify(this.getErrorNotification(err));
      });
    } else {
      this._recruitmentService.getAllJobPositions().subscribe(res => {
          const openJobs = res.data ? res.data.filter(x => x.status === 1) : [];
          const dialogRef = this._dialog.open(JobRecommendationDialogComponent, {
            width: '400px',
            data: { jobs: openJobs }
          });

          dialogRef.afterClosed().subscribe((jobApplicationId: string) => {
            if (jobApplicationId) {
              this._recruitmentService.addCandidateToJobPosition(candidates, jobApplicationId).subscribe(() => {
                this._router.navigate(['configuracoes/gerenciar-inscricoes-vagas/' + jobApplicationId]);
              }, err => {
                this.notify(this.getErrorNotification(err));
              });
            }
          });
      }, err => {
          this.notify(this.getErrorNotification(err));
      });
    }
  }

  public getAge() {
    if (this.user && this.user.userInfo.dateBorn) {
      const today = new Date();
      const birthDate = this.user.userInfo.dateBorn;
      const birthDateNew = new Date(birthDate);
      let age = today.getFullYear() - birthDateNew.getFullYear();
      if (today.getMonth() && today.getDay() >= birthDateNew.getMonth() && birthDateNew.getDay()) {
        return age;
      } else {
        return age--;
      }
    }
  }

}
