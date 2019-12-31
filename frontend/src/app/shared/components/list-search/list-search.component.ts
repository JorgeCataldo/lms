import { Component, EventEmitter, Output, OnInit, Input } from '@angular/core';
import { Subject } from 'rxjs';
import { debounceTime } from 'rxjs/operators';

@Component({
  selector: 'app-list-search',
  template: `
    <div class="list-search" [style.background]="background" [ngClass]="{ 'no-padding': noPadding, 'white': white }" >
      <i class="icon icon-lupa icon-search"></i>
      <input id="list-search-input"
        type="text"
        [placeholder]="placeholder"
        (keyup)="updateSearch($event.target.value)"
        [value]="inputValue"
        [disabled]="disabled"
      />
    </div>`,
  styleUrls: ['./list-search.component.scss']
})
export class ListSearchComponent implements OnInit {

  @Input() placeholder: string = 'Buscar';
  @Input() background: string = 'transparent';
  @Input() noPadding: boolean = false;
  @Input() white: boolean = false;
  @Input() disabled: boolean = false;
  @Input() inputValue: string = '';
  @Output() triggerSearch: EventEmitter<string> = new EventEmitter();

  private _searchSubject: Subject<string> = new Subject();

  ngOnInit() {
    this._setSearchSubscription();
  }

  public updateSearch(searchTextValue: string) {
    this._searchSubject.next( searchTextValue );
  }

  private _setSearchSubscription() {
    this._searchSubject.pipe(
      debounceTime(500)
    ).subscribe((searchValue: string) => {
      this.triggerSearch.emit( searchValue );
    });
  }

}
