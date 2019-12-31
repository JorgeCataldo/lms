import { Component, Input, OnInit } from '@angular/core';
import { User } from 'src/app/models/user.model';
import { MatSnackBar } from '@angular/material';
import { SettingsUsersService } from 'src/app/settings/_services/users.service';
import { AuthService } from 'src/app/shared/services/auth.service';
import { NotificationClass } from 'src/app/shared/classes/notification';
import { EventPreview } from 'src/app/models/previews/event.interface';
import { ContentEventsService } from 'src/app/pages/_services/events.service';
import { ModuleProgress } from 'src/app/models/previews/module-progress.interface';

@Component({
  selector: 'app-user-calendar',
  templateUrl: './user-calendar.component.html',
  styleUrls: ['./user-calendar.component.scss']
})
export class SettingsUserCalendarComponent extends NotificationClass implements OnInit {

  public user: User;
  public userId: string;
  public events: Array<EventPreview>;

  public translatedValue: number = 0;
  public scrollLimit: number = 1;
  public viewType: string = 'cards';
  public moduleProgress: any = {};
  public trackProgress: any = {};
  public levels: any;
  public levelDict: any;

  constructor(
    protected _snackBar: MatSnackBar,
    private _usersService: SettingsUsersService,
    private _eventsService: ContentEventsService,
    private _authServiceRoute: AuthService
  ) {
    super(_snackBar);
  }

  ngOnInit() {
    this.userId = this._authServiceRoute.getLoggedUserId();
    this._loadUser( this.userId );
    this._loadEvents();
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

  private _loadUserCareer(userId: string): void {
    this._usersService.getUserCareer(
      userId
    ).subscribe((response) => {
      this.user.career = response.data;
    }, () => { });
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
