import { Component, OnInit, Input } from '@angular/core';
import { ModulePreview } from '../../models/previews/module.interface';
import { ContentModulesService } from '../_services/modules.service';
import { NotificationClass } from '../../shared/classes/notification';
import { MatSnackBar, MatDialog } from '@angular/material';
import { ContentEventsService } from '../_services/events.service';
import { ContentTracksService } from '../_services/tracks.service';
import { TrackPreview } from '../../models/previews/track.interface';
import { EventPreview } from '../../models/previews/event.interface';
import { UserService } from '../_services/user.service';
import { SharedService } from 'src/app/shared/services/shared.service';
import { ModuleProgress } from 'src/app/models/previews/module-progress.interface';
import { SettingsProfileTestsService } from 'src/app/settings/_services/profile-tests.service';
import { Suggestion } from 'src/app/models/suggestions.model';
import { ProfileTestResponse } from 'src/app/models/profile-test.interface';
import { Router, ActivatedRoute } from '@angular/router';
import { SettingsUsersService } from 'src/app/settings/_services/users.service';
import { User } from 'src/app/models/user.model';
import { AuthService } from 'src/app/shared/services/auth.service';
import { NpsDialogComponent } from 'src/app/shared/dialogs/nps/nps.dialog';
import {SlideshowModule} from 'ng-simple-slideshow';
import { environment } from 'src/environments/environment';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss']
})
export class HomeComponent extends NotificationClass implements OnInit {

  public modules: Array<ModulePreview>;
  public events: Array<EventPreview>;
  public suggestions: Array<Suggestion>;
  public test: ProfileTestResponse;
  public defaultTest: ProfileTestResponse;

  public tracks: Array<TrackPreview>;
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
  public imageUrlArray: string[];

  public height: string;
  public minHeight: string;
  public autoPlay: string;
  public showArrows: boolean;
  public autoPlayInterval: string;
  public stopAutoPlayOnSlide: boolean;
  public imageUrls: string;
  public lazyLoad: boolean;
  public autoPlayWaitForLazyLoad: boolean;
  public showVersionChanges: boolean = false;

  @Input() performance: boolean = true;
  public userId: string;
  public user: User;

  constructor(
    protected _snackBar: MatSnackBar,
    private _modulesService: ContentModulesService,
    private _eventsService: ContentEventsService,
    private _tracksService: ContentTracksService,
    private _userService: UserService,
    private _usersService: SettingsUsersService,
    private _sharedService: SharedService,
    private _profileTestsService: SettingsProfileTestsService,
    private _router: Router,
    private _authService: AuthService,
    private _dialog: MatDialog
  ) {
    super(_snackBar);
  }

  ngOnInit() {
    this._checkMessages();
    this._loadLevels();
    this._loadModules();
    this._loadEvents();
    this._loadTracks();
    this._loadProgress();
    this._loadSuggestions();
    this.userId = this._authService.getLoggedUserId();
    this._loadUser( this.userId );
    this.checkUserType();
    this._clearCaches();
    this._checkNps();
    this._loadBannerImages();

    this.showVersionChanges = environment.production === false && (
                              JSON.parse(localStorage.getItem('seenVersionChanges')) === false ||
                              JSON.parse(localStorage.getItem('seenVersionChanges')) === null);
  }

  public hasContent(): boolean {
    return (this.allModules && this.allModules.length > 0) ||
           (this.events && this.events.length > 0) ||
           (this.tracks && this.tracks.length > 0);
  }

  public hasNoContent(): boolean {
    if (!this.allModules || !this.events || !this.tracks)
      return false;

    return this.allModules.length === 0 &&
           this.events.length === 0 &&
           this.tracks.length === 0;
  }

  public goToProfileTest(): void {
    if (this.hasSuggestedTest())
      this._router.navigate([ 'teste-de-perfil/' + this.test.testId ]);
    else if (this.hasDefaultTest())
      this._router.navigate([ 'teste-de-perfil/' + this.defaultTest.testId ]);
  }

  public goToProductsCatalog(): void {
    this._router.navigate([ 'catalogo-de-cursos' ]);
  }

  public goToStore(suggestion: Suggestion): void {
    if (suggestion.storeUrl && suggestion.storeUrl.trim() !== '') {
      const hasProtocol = suggestion.storeUrl.split('http').length > 1;
      window.location.href = hasProtocol ? suggestion.storeUrl : '//' + suggestion.storeUrl;
    }
  }

  private _loadBannerImages(): void {
    this.imageUrlArray = [
      './assets/img/BannerLms1.png',
      './assets/img/BannerLms2.png'
    ];
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

  public slideRight(): void {
    const modulesElement: HTMLElement = document.querySelector('.modules');
    const width = screen.width >= 1200 ? 1200 : (screen.width - 30);
    this.scrollLimit = modulesElement.scrollWidth - width;

    if (-this.translatedValue < this.scrollLimit) {
      this.translatedValue = this.translatedValue - 100;
      const elements: Array<HTMLElement> = Array.from(document.querySelectorAll('.module'));

      let offset = 0;
      if (-this.translatedValue > this.scrollLimit)
        offset = -this.translatedValue - this.scrollLimit;

      elements.forEach((el: HTMLElement) => {
        el.style.left = this.translatedValue + offset + 'px';
      });
    }
  }

  public slideLeft(): void {
    if (this.translatedValue <= -100) {
      this.translatedValue = this.translatedValue + 100;
      const elements: Array<HTMLElement> = Array.from(document.querySelectorAll('.module'));

      elements.forEach((el: HTMLElement) => {
        el.style.left = this.translatedValue + 'px';
      });
    }
  }

  public showSlider(): boolean {
    const width = screen.width >= 1200 ? 1200 : (screen.width - 30);
    return this.events && (width < (this.events.length * 350) + 350);
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

  public hasSuggestions(): boolean {
    return this.suggestions && this.suggestions.length > 0;
  }

  public hasTest(): boolean {
    return !this.hasSuggestions() && (
      this.hasDefaultTest() ||
      this.hasSuggestedTest()
    );
  }

  public hasDefaultTest(): boolean {
    return this.hasNoContent() &&
      this.defaultTest &&
      this.defaultTest.testId != null;
  }

  public hasSuggestedTest(): boolean {
    return this.test &&
      this.test.testId != null && (
        !this.test.answers ||
        this.test.answers.length === 0
      );
  }

  private _loadModules(): void {
    this._modulesService.getPagedHomeModulesList(
      this._modulesPage, 8
    ).subscribe((response) => {
      this.allModules = response.data.modules;
      this.modulesCount = response.data.itemsCount;

    }, () => {
      this.notify('Ocorreu um erro, por favor tente novamente mais tarde');
    });
  }

  private _loadEvents(): void {
    this._eventsService.getHomeEvents().subscribe((response) => {
      const events = response.data;
      this.events = [];
      events.forEach((event: EventPreview) => {
        const moduleProgress = {};
        if (event.moduleProgressInfo) {
          event.moduleProgressInfo.forEach((mP: ModuleProgress) =>
            moduleProgress[mP.moduleId] = mP
          );
        }

        event.schedules.forEach(schedule => {
          if (new Date(schedule.eventDate).getTime() >  new Date().getTime() )
            this.events.push({
              id: event.id,
              title: event.title,
              instructor: '',
              date: schedule.eventDate,
              imageUrl: event.imageUrl,
              subscriptionDue: null,
              status: null,
              nextSchedule: schedule,
              published: event.published,
              requirements: event.requirements,
              moduleProgressInfo: moduleProgress,
              location: event.location
            });
        });
      });
      this.events.sort((a, b) => {
        return new Date(a.date).getTime() - new Date(b.date).getTime();
      });

    }, () => {
      this.notify('Ocorreu um erro, por favor tente novamente mais tarde');
    });
  }

  private _loadTracks(): void {
    this._tracksService.getNotRecommendedTracks().subscribe((response) => {
      console.log('response.data.tracks -> ', response.data.tracks);
      // const tracksRecommened = response.data.tracks.filter(x => !x.recommended);
      const tracksRecommened = response.data.tracks;
      const limitTrackRecommened = tracksRecommened.slice(0, 3);
      this.tracks = limitTrackRecommened.sort(function(a, b) {
        return a.title > b.title ? 1 : -1;
      });
      // this.tracksCount = response.data.itemsCount;
      this.tracksCount = 0;

    }, () => {
      this.notify('Ocorreu um erro, por favor tente novamente mais tarde');
    });
  }

  private _loadSuggestions(): void {
    this._profileTestsService.getSuggestedProducts().subscribe((response) => {
      this.suggestions = [
        ...response.data.modules,
        ...response.data.events,
        ...response.data.tracks
      ];
      this.test = response.data.test;
      this.defaultTest = response.data.defaultTest;

    }, () => {
      this.notify('Ocorreu um erro, por favor tente novamente mais tarde');
    });
  }

  private _loadUser(userId: string): void {
    this._usersService.getUserProfile(
      userId
    ).subscribe((response) => {
      this.user = response.data;
      // this._loadUserCareer( userId );

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

  private _checkNps() {
    this._usersService.getUserNpsAvailability().subscribe(res => {
      if (res.data.active) {
        const dialogRef = this._dialog.open(NpsDialogComponent, {
          width: '1000px',
          data: res.data
        });
        dialogRef.afterClosed().subscribe((result: string) => {
          if (result != null) {
            this.notify(result);
          }
        });
      }
    });
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

  public goTrack(track: TrackPreview) {
    if (track.published) {
      this._router.navigate([ 'trilha/' + track.id ]);
    } else {
      this._router.navigate([ 'trilha-de-curso/' + track.id ]);
    }
  }

  public editTrack(track: TrackPreview) {
    this._tracksService.getTrackById(track.id).subscribe((response) => {
      localStorage.setItem('editingTrack', JSON.stringify(response.data));
      this._router.navigate([ '/configuracoes/trilha' ]);

    }, () => this.notify('Ocorreu um erro, por favor tente novamente mais tarde'));
  }

  public sumarryTrack(track: TrackPreview) {
    this._router.navigate([ 'configuracoes/trilha-de-curso/' + track.id ]);
  }

  public closeVersionBox() {
    localStorage.setItem('seenVersionChanges', JSON.stringify(true));
    this.showVersionChanges = false;
  }
}
