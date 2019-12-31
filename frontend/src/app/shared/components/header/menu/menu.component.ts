import { Component, Output, EventEmitter } from '@angular/core';
import { AuthService, LoginResponse } from 'src/app/shared/services/auth.service';
import { environment } from 'src/environments/environment';
import { MenuSection, MenuItem } from './menu-item.interface';
import { allMenuItems } from './menu-items';
import { Router } from '@angular/router';

@Component({
  selector: 'app-menu',
  styleUrls: ['./menu.component.scss'],
  template: `
    <h1>Menu</h1>
    <ng-container *ngFor="let section of menuItems" >
      <h3>{{ section.title }}</h3>
      <ul>
        <li *ngFor="let item of section.items" >
          <a href="javascript:;"
            [ngClass]="{'selected': checkRoute(item)}"
            (click)="navigateTo(item.url)">
            <i [class]="item.iconClass" ></i>
            {{ item.title }}
          </a>
        </li>
      </ul>
    </ng-container>`
})
export class MenuComponent {

  public menuItems: Array<MenuSection> = [];
  public user: LoginResponse;

  @Output() closeMenu = new EventEmitter();

  constructor(
    private _authService: AuthService,
    private _router: Router
  ) {
    this._authService.isLoggedIn().subscribe(() => {
      this.user = _authService.getLoggedUser();
      if (this.user)
        this._buildUserMenu( this.user );
    });
  }

  private _buildUserMenu(user: LoginResponse): void {
    this.menuItems = [];

    allMenuItems().forEach(section => {
      if (this._checkSection(section, user)) {
        const newSection: MenuSection = {
          'title': section.title,
          'items': []
        };

        section.items.forEach(item => {
          if (this._checkAccess(item, user)) {
            newSection.items.push(item);
          }
        });

        if (newSection.items.length > 0)
          this.menuItems.push(newSection);
      }
    });
  }

  private _checkSection(section: MenuSection, user: LoginResponse): boolean {
    if (section.checkDependents && user.role === 'Student' && !user.have_dependents)
      return false;
    return this._checkConfigAccess(section, user) &&
      section.permittedRoles.includes(user.role);
  }

  private _checkAccess(menuItem: MenuItem, user: LoginResponse): boolean {

    return this._checkFeatures(menuItem) &&
      this._checkConfigAccess(menuItem, user) &&
      !this._checkBlockAccess(menuItem, user) &&
      menuItem.permittedRoles.includes(user.role);
  }

  private _checkFeatures(menuItem: MenuItem): boolean {
    return (!menuItem.checkProfileTest || environment.features.profileTest) &&
      (!menuItem.checkFormulas || environment.features.formulas) &&
      menuItem.isRunningFeature;
  }

  private _checkConfigAccess(menuItem: MenuSection | MenuItem, user: LoginResponse): boolean {
    if (!menuItem.checkAccess || menuItem.checkAccess.length === 0)
      return true;

    if (user.role === 'Admin' || user.role === 'HumanResources' || user.role === 'Author')
      return true;

    if (!user.adminAccesses || !(user.adminAccesses instanceof Array))
      return false;

    return user.adminAccesses.some(access => menuItem.checkAccess.includes(access));
  }

  private _checkBlockAccess(menuItem: MenuSection | MenuItem, user: LoginResponse): boolean {
    if (!menuItem.blockAccess || menuItem.blockAccess.length === 0)
      return false;

    return user.adminAccesses.some(access => menuItem.blockAccess.includes(access));
  }

  public checkRoute(item: MenuItem): boolean {
    let path: string = this._router.url;
    path = path.substr(1);
    if (path.includes(':userId')) {
      path = path.replace(this.user.user_id, ':userId');
    }
    if (item.url === path)
      return true;
    return false;
  }

  public navigateTo(url: string) {
    url = url.replace(':userId', this.user.user_id);
    this._router.navigate([ url ]);
    this.closeMenu.emit();
  }

}
