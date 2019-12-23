import { Component, Input, EventEmitter, Output, OnInit } from '@angular/core';
import { UserTab } from './user-tab.model';

@Component({
  selector: 'app-users-tabs',
  template: `
    <div class="users-tabs" [ngStyle]="getBorderColors(selectedTab)">
      <div
        class="tab"
        *ngFor="let tab of tabs; let index = index"
        (click)="setTab(tab, index)"
        [ngStyle]="getTabColors(tab)"
        [ngClass]="{ selected: tab === selectedTab }"
      >
        <p class="count" [ngStyle]="getTabSubTitleColors(tab)" >
          {{ tab.count }}
        </p>
        <p class="title">
          {{ tab.title }}
          <span class="subTitle"
            [ngStyle]="getTabSubTitleColors(tab)">
            {{ tab.subTitle }}
          </span>
        </p>
      </div>
    </div>
  `,
  styleUrls: ['./users-tabs.component.scss']
})
export class UsersTabsComponent implements OnInit {
  @Input() readonly tabs: Array<UserTab>;
  @Output() selectTab: EventEmitter<number> = new EventEmitter();

  public selectedTab: UserTab;

  ngOnInit() {
    if (this.tabs && this.tabs.length > 0) this.selectedTab = this.tabs[0];
  }

  public getTabColors(tab: UserTab) {
    if (tab === this.selectedTab) {
      return {
        backgroundColor: tab.color,
        color: 'white'
      };
    }
    return {
      backgroundColor: 'white',
      color: '#797979'
    };
  }

  public getTabSubTitleColors(tab: UserTab) {
    if (tab === this.selectedTab) {
      return {
        backgroundColor: tab.subColor,
        color: 'white'
      };
    }
    return {
      backgroundColor: 'white',
      color: tab.subColor
    };
  }

  public getTabCountColors(tab: UserTab) {
    if (tab === this.selectedTab) {
      return {
        color: 'white'
      };
    }
    return {
      color: tab.subColor
    };
  }

  public getBorderColors(tab: UserTab) {
    return {
      'border-color': tab.color
    };
  }
  public setTab(userTab: UserTab, index: number) {
    this.selectedTab = userTab;
    this.selectTab.emit(index);
  }
}
