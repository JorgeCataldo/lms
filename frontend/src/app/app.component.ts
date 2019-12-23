import { Component, OnInit, HostListener, ElementRef, OnDestroy } from '@angular/core';
import { animations } from './app.animations';
import { AuthService } from './shared/services/auth.service';
import { AnalyticsService } from './shared/services/analytics.service';
import { ActionInfo } from './shared/directives/save-action/action-info.interface';
import { ColorKey } from './models/color-key';
import { Subscription } from 'rxjs';
import { UserPreviewInterceptor } from './shared/interfaces/user-preview-interceptor.interface';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss'],
  animations: animations
})
export class AppComponent implements OnInit, OnDestroy {

  public opened = false;

  public isImpersonating: boolean = false;
  public impersonatedInfo: UserPreviewInterceptor;
  private _impersonationSubscription: Subscription;

  constructor(
    private _authService: AuthService,
    private _actionsService: AnalyticsService,
    private elementRef: ElementRef
  ) { }

  ngOnInit() {
    this._authService.isLoggedIn().subscribe((isLogged) => {
      if (!isLogged)
        this.opened = false;
    });

    this._authService.setUserColorPalette().subscribe((set) => {
      this._changeTheme(set);
    });

    this.impersonatedInfo = this._authService.getImpersonatedUser();
    this.isImpersonating = this.impersonatedInfo && true;

    this._impersonationSubscription = this._authService.personificationChanged$.subscribe(
      (isImpersonating) => {
        this.isImpersonating = isImpersonating;
        this.impersonatedInfo = this._authService.getImpersonatedUser();
      }
    );
  }

  public getPage(outlet) {
    return outlet.activatedRouteData['page'] || 'home';
  }

  public toogleSideNav() {
    this.opened = !this.opened;
  }

  @HostListener('window:beforeunload', ['$event'])
  unloadNotification($event: any) {
    // $event.returnValue = true;
    const actions = this._actionsService.getStorageWaitingActions();
    actions.forEach(action => this._setExamFinishAction( action ));
    this._actionsService.clearAllActions();
  }

  private _setExamFinishAction(actionInfo: ActionInfo): void {
    this._actionsService.saveAction(actionInfo).subscribe();
  }

  private _changeTheme(isLogged: boolean) {
    const colorList = localStorage.getItem('color-palette');
    if (isLogged) {
      if (colorList) {
        const parsedColorList: ColorKey[] = JSON.parse(colorList);
        parsedColorList.forEach(colorKey => {
          if (colorKey.key && colorKey.color) {
            this.elementRef.nativeElement.style.setProperty(colorKey.key, colorKey.color);
          }
        });
      }
    } else {
      this._setDefaultColors();
      localStorage.removeItem('color-palette');
    }
  }

  private _setDefaultColors() {
    this.elementRef.nativeElement.style.setProperty('--primary-color', '#23BCD1');
    this.elementRef.nativeElement.style.setProperty('--alternate-primary-color', '#239BD1');
    this.elementRef.nativeElement.style.setProperty('--semi-primary-color', '#89D2DC');
    this.elementRef.nativeElement.style.setProperty('--card-primary-color', '#23BCD1');
    this.elementRef.nativeElement.style.setProperty('--third-primary-color', '#1988c8');
    this.elementRef.nativeElement.style.setProperty('--divider-color', '#1c96df');
    this.elementRef.nativeElement.style.setProperty('--selected-color', '#43b3f3');
    this.elementRef.nativeElement.style.setProperty('--progress-bar-uncompleted', '#e8e8e8');
    this.elementRef.nativeElement.style.setProperty('--progress-bar-completed', '#80d1dc');
    this.elementRef.nativeElement.style.setProperty('--danger-color', '#FF8D9E');
    this.elementRef.nativeElement.style.setProperty('--warn-color', '#FFE08D');
    this.elementRef.nativeElement.style.setProperty('--success-color', '#A8F5B4');
    this.elementRef.nativeElement.style.setProperty('--text-color', '#5D5D5D');
    this.elementRef.nativeElement.style.setProperty('--alt-text-color', '#4f4f4f');
    this.elementRef.nativeElement.style.setProperty('--box-shadow', '1px 2px 6px 0px rgba(180, 180, 180, 1)');
    this.elementRef.nativeElement.style.setProperty('--header-accent', '#efefef');
    this.elementRef.nativeElement.style.setProperty('--main-background', '#ffffff');
    this.elementRef.nativeElement.style.setProperty('--light-color', '#1aAFc9');
    this.elementRef.nativeElement.style.setProperty('--footer-background', '#EDEDED');
  }

  ngOnDestroy() {
    this._impersonationSubscription.unsubscribe();
  }
}
