import { Component, OnInit } from '@angular/core';
import { MatSnackBar } from '@angular/material';
import { User } from '../../../models/user.model';
import { SettingsUsersService } from '../../_services/users.service';
import { ActivatedRoute } from '@angular/router';
import { NotificationClass } from '../../../shared/classes/notification';
import { AuthService } from 'src/app/shared/services/auth.service';
import { environment } from '../../../../environments/environment';

@Component({
  selector: 'app-user-details',
  templateUrl: './user-details.component.html',
  styleUrls: ['./user-details.component.scss']
})
export class SettingsUserDetailsComponent extends NotificationClass implements OnInit {

  public user: User;
  public userId: string;
  public currentTab: string = 'BasicInfo';
  public isAdmin: boolean = false;
  public showCareer: boolean = environment.features.career;

  constructor(
    protected _snackBar: MatSnackBar,
    private _usersService: SettingsUsersService,
    private _activatedRoute: ActivatedRoute,
    private _authService: AuthService
  ) {
    super(_snackBar);
  }

  ngOnInit() {
    this.isAdmin = this._authService.isAdmin();
    document.documentElement.style.setProperty('--user-tabs-num', this.isAdmin ? '3' : '2');
    this.userId = this._activatedRoute.snapshot.paramMap.get('userId');
    this._loadUser( this.userId );
    if (this.isAdmin && this.showCareer) {
      const doc = document.getElementsByClassName('switch-content');
      doc[0].setAttribute('style', 'grid-template-columns: repeat(3, 1fr);');
    }
  }

  public changeUserInfoDisplay(tab: string) {
    this.currentTab = tab;
  }

  private _loadUser(userId: string): void {
    this._usersService.getUserProfile(
      userId
    ).subscribe((response) => {
      this.user = response.data;
      this._loadUserCareer( userId );

    }, (error) => {
      this.notify(
        this.getErrorNotification(error)
      );
    });
  }

  private _loadUserCareer(userId: string): void {
    this._usersService.getUserCareer(
      userId
    ).subscribe((response) => {
      this.user.career = response.data;
    }, () => { });
  }

  public upDateUser() {
    this._loadUser( this.userId );
  }

}
