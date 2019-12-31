import { Component, OnInit, Input } from '@angular/core';
import { ModulePreview } from '../../models/previews/module.interface';
import { ContentModulesService } from '../_services/modules.service';
import { NotificationClass } from '../../shared/classes/notification';
import { MatSnackBar } from '@angular/material';
import { ContentTracksService } from '../_services/tracks.service';
import { TrackPreview } from '../../models/previews/track.interface';
import { UserService } from '../_services/user.service';
import { SharedService } from 'src/app/shared/services/shared.service';
import { Suggestion } from 'src/app/models/suggestions.model';
import { ProfileTestResponse } from 'src/app/models/profile-test.interface';
import { SettingsUsersService } from 'src/app/settings/_services/users.service';
import { User } from 'src/app/models/user.model';
import { AuthService } from 'src/app/shared/services/auth.service';
import { EventPreview } from 'src/app/models/previews/event.interface';
import { ContentEventsService } from '../_services/events.service';
import { ModuleProgress } from 'src/app/models/previews/module-progress.interface';

@Component({
  selector: 'app-my-courses',
  templateUrl: './my-courses.component.html',
  styleUrls: ['./my-courses.component.scss']
})
export class MyCoursesComponent extends NotificationClass implements OnInit {

  public events: Array<EventPreview>;
  public modules: Array<ModulePreview>;
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

  @Input() performance: boolean = true;
  public userId: string;
  public user: User;

  constructor(
    protected _snackBar: MatSnackBar,
    private _modulesService: ContentModulesService,
    private _eventsService: ContentEventsService,
    private _tracksService: ContentTracksService,
    private _userService: UserService,
    private _sharedService: SharedService,
    private _authService: AuthService,
    private _usersService: SettingsUsersService
  ) {
    super(_snackBar);
  }

  ngOnInit() {
    this._loadLevels();
    this._loadModules();
    this._loadTracks();
    this._loadProgress();
    this._loadEvents();
    this.userId = this._authService.getLoggedUserId();
    this._loadUser( this.userId );
  }

  public hasContent(): boolean {
    return (this.allModules && this.allModules.length > 0) ||
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
    this._modulesService.getPagedFilteredMyCoursesModulesList(
      this._modulesPage, 8
    ).subscribe((response) => {
      this.allModules = response.data.modules;
      this.modulesCount = response.data.itemsCount;
    }, () => {
      this.notify('Ocorreu um erro, por favor tente novamente mais tarde');
    });
  }

  private _loadTracks(): void {
    this._tracksService.getPagedFilteredTracksList(this._tracksPage, 8).subscribe((response) => {
      this.tracks = response.data.tracks;
      console.log('this.tracks -> ', this.tracks);
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
    }, (error) => {
      this.notify(
        this.getErrorNotification(error)
      );
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

  public slideRight(): void {
    const modulesElement: HTMLElement = document.querySelector('.modules');
    const width = screen.width >= 1200 ? 1200 : (screen.width - 30);
    this.scrollLimit = -modulesElement.scrollWidth - width;

    if (-this.translatedValue > this.scrollLimit) {
      this.translatedValue = this.translatedValue - 100;
      const elements: Array<HTMLElement> = Array.from(document.querySelectorAll('.module'));

      let offset = 0;
      if (-this.translatedValue < this.scrollLimit)
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
}
