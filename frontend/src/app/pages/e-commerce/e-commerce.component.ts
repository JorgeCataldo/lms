import { Component, OnInit, Input } from '@angular/core';
import { ModulePreview } from '../../models/previews/module.interface';
import { ContentModulesService } from '../_services/modules.service';
import { NotificationClass } from '../../shared/classes/notification';
import { MatSnackBar, MatDialog } from '@angular/material';
import { ContentTracksService } from '../_services/tracks.service';
import { TrackPreview } from '../../models/previews/track.interface';
import { EventPreview } from '../../models/previews/event.interface';
import { UserService } from '../_services/user.service';
import { SharedService } from 'src/app/shared/services/shared.service';
import { Suggestion } from 'src/app/models/suggestions.model';
import { ProfileTestResponse } from 'src/app/models/profile-test.interface';
import { SettingsUsersService } from 'src/app/settings/_services/users.service';
import { User } from 'src/app/models/user.model';
import { AuthService } from 'src/app/shared/services/auth.service';

@Component({
  selector: 'app-e-commerce',
  templateUrl: './e-commerce.component.html',
  styleUrls: ['./e-commerce.component.scss']
})
export class EcommerceComponent extends NotificationClass implements OnInit {

  public modules: Array<ModulePreview>;
  public events: Array<EventPreview>;
  public suggestions: Array<Suggestion>;
  public test: ProfileTestResponse;
  public defaultTest: ProfileTestResponse;

  public tracks: Array<TrackPreview>;
  public suggestedTracks: Array<TrackPreview>;
  public noSuggestedTracks: Array<TrackPreview>;
  public tracksCount: number = 0;
  private _tracksPage: number = 1;

  public allModules: Array<ModulePreview>;
  public modulesCount: number = 0;
  private _modulesPage: number = 1;

  public translatedValue: number = 0;
  public scrollLimit: number = 1;
  public viewType: string = 'cards';
  public moduleProgress: any = {};
  public trackProgress: any = {};
  public levels: any;
  public levelDict: any;
  public teste: string;
  public _checkUserType: boolean = true;
  public _checkUserAdmin: boolean = false;

  @Input() performance: boolean = true;
  public userId: string;
  public user: User;

  constructor(
    protected _snackBar: MatSnackBar,
    private _modulesService: ContentModulesService,
    private _tracksService: ContentTracksService,
    private _userService: UserService,
    private _sharedService: SharedService,
    private _authService: AuthService,
    private _usersService: SettingsUsersService
  ) {
    super(_snackBar);
  }

  ngOnInit() {
    this._checkMessages();
    this._loadLevels();
    this._loadModules();
    this._loadTracks();
    this._loadSuggestedTracks();
    this._loadProgress();
    this.userId = this._authService.getLoggedUserId();
    this._loadUser( this.userId );
    this.checkUserType();
    this._clearCaches();
  }

  public hasContent(): boolean {
    return (this.allModules && this.allModules.length > 0) ||
           (this.events && this.events.length > 0) ||
           (this.tracks && this.tracks.length > 0);
  }

  public goToStore(suggestion: Suggestion): void {
    if (suggestion.storeUrl && suggestion.storeUrl.trim() !== '') {
      const hasProtocol = suggestion.storeUrl.split('http').length > 1;
      window.location.href = hasProtocol ? suggestion.storeUrl : '//' + suggestion.storeUrl;
    }
  }

  private _loadProgress(): any {
    this._userService.getUserModulesProgress().subscribe((response) => {
      this.moduleProgress = {};
      response.data.forEach(x => {
        this.moduleProgress[x.moduleId] = x;
      });

    }, () => {
      this.notify('Ocorreu um erro, por favor tente novamente mais tarde');
    });

    this._userService.getUserTracksProgress().subscribe((response) => {
      this.trackProgress = {};
      response.data.forEach(x => {
        this.trackProgress[x.trackId] = x;
      });

    }, () => {
      this.notify('Ocorreu um erro, por favor tente novamente mais tarde');
    });
  }

  private _loadLevels(): void {
    this._sharedService.getLevels(true).subscribe((response) => {
      this.levels = response.data;
      this.levelDict = {};
      this.levels.forEach(level => {
        this.levelDict[level.id] = level.description;
      });
    }, () => this.notify('Ocorreu um erro, por favor tente novamente mais tarde'));
  }

  public changeViewType(): void {
    this.viewType = this.viewType === 'cards' ? 'list' : 'cards';
  }

  public goToPage(page: number) {
    if (page !== this._modulesPage) {
      this._modulesPage = page;
      this._loadModules();
    }
  }

  public goToPageTrack(page: number) {
    if (page !== this._tracksPage) {
      this._tracksPage = page;
      this._loadTracks();
    }
  }

  private _loadModules(): void {
    // this._modulesService.getPagedHomeModulesList(
    this._modulesService.getPagedFilteredModulesList(
      this._modulesPage, 0
    ).subscribe((response) => {
      this.allModules = response.data.modules;
      this.modulesCount = response.data.itemsCount;
    }, () => {
      this.notify('Ocorreu um erro, por favor tente novamente mais tarde');
    });
  }

  private _loadTracks(): void {
    this._tracksService.getPagedFilteredTracksList(this._tracksPage, 0).subscribe((response) => {
      this.tracks = response.data.tracks;
      this.tracksCount = response.data.itemsCount;
    }, () => {
      this.notify('Ocorreu um erro, por favor tente novamente mais tarde');
    });
  }

  private _loadSuggestedTracks(): void {
    this._tracksService.getPagedFilteredTracksList(this._tracksPage, 4, null, null, null, null).subscribe((response) => {
      console.log('response.data.tracks -> ', response.data.tracks);
      this.suggestedTracks = response.data.tracks.sort(function(a, b) {
        return a.title > b.title ? 1 : -1;
      });
      console.log('suggestedTracks -> ', this.suggestedTracks);
      this.noSuggestedTracks = this.suggestedTracks.filter( x => !x.recommended);
      this.tracksCount = response.data.itemsCount;
    }, () => {
      this.notify('Ocorreu um erro, por favor tente novamente mais tarde');
    });
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
  public checkUserType() {
    const userId = this._authService.getLoggedUserId();
    const type = this._authService.getLoggedUserRole();
    if (userId && type) {
        if (type === 'Student') {
          this._checkUserType = this._checkUserType;
        } else {
          this._checkUserAdmin = this._checkUserAdmin;
      }
    }
  }

  private _clearCaches() {
    localStorage.removeItem('filters-manage-team');
    localStorage.removeItem('filters-manage-team-selected');
    localStorage.removeItem('filters-student-search-job');
    localStorage.removeItem('filters-student-search-job-selected');
    localStorage.removeItem('filters-manage-talents');
    localStorage.removeItem('filters-manage-talents-selected');
  }

  private _checkMessages(): void {
    const expiredModule = localStorage.getItem('expiredModule');
    const expiredTrack = localStorage.getItem('expiredTrack');

    if (expiredModule) {
      this.notify(expiredModule);
      localStorage.removeItem('expiredModule');
    }

    if (expiredTrack) {
      this.notify(expiredTrack);
      localStorage.removeItem('expiredTrack');
    }

  }
}
