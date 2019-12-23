import { Component, OnInit } from '@angular/core';
import { MatSnackBar } from '@angular/material';
import { User } from '../../../../models/user.model';
import { SettingsUsersService } from '../../../_services/users.service';
import { NotificationClass } from '../../../../shared/classes/notification';
import { AuthService } from 'src/app/shared/services/auth.service';

@Component({
  selector: 'app-user-performace',
  templateUrl: './user-performace.component.html',
  styleUrls: ['./user-performace.component.scss']
})
export class SettingsUserPerformaceComponent extends NotificationClass implements OnInit {

  public user: User;
  public userId: string;

  constructor(
    protected _snackBar: MatSnackBar,
    private _usersService: SettingsUsersService,
    private _authPerformaceService: AuthService,
  ) {
    super(_snackBar);
  }

  ngOnInit() {
    this.userId = this._authPerformaceService.getLoggedUserId();
    this._loadUser( this.userId );
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

}
