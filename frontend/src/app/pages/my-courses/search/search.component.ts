import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { Subject } from 'rxjs';
import { debounceTime } from 'rxjs/operators';
import { ContentModulesService } from '../../_services/modules.service';
import { NotificationClass } from '../../../shared/classes/notification';
import { MatSnackBar } from '@angular/material';
import { ModulePreview } from '../../../models/previews/module.interface';
import { TrackPreview } from 'src/app/models/previews/track.interface';
import { ContentTracksService } from '../../_services/tracks.service';

@Component({
  selector: 'app-my-courses-search',
  templateUrl: './search.component.html',
  styleUrls: ['./search.component.scss']
})
export class MyCoursesSearchComponent extends NotificationClass implements OnInit {

  public searchValue: string = '';
  private searchSubject: Subject<string> = new Subject();
  public showSearchFullscren: boolean = false;
  public results: Array<ModulePreview> = [];
  public tracks: Array<TrackPreview> = [];

  constructor(
    protected _snackBar: MatSnackBar,
    private _router: Router,
    private _modulesService: ContentModulesService,
    private _tracksService: ContentTracksService
  ) {
    super(_snackBar);
  }

  ngOnInit() {
    this._setSearchSubscription();
  }

  public openSearch(): void {
    this.showSearchFullscren = true;
    document.getElementById('SearchDisplay').blur();
    setTimeout(() => {
      const searchEl = document.getElementById('Search');
      if (searchEl)
        searchEl.focus();
    });
  }

  public doSearch(): void {
    this.searchSubject.next(this.searchValue);
  }

  public goToAllResults(result: string): void {
    this._router.navigate(['/busca/' + result]);
  }

  public goToModuleResult(result: ModulePreview): void {
    this._router.navigate(['/modulo/' + result.id]);
  }

  public goToTrackResult(result: TrackPreview): void {
    this._router.navigate(['/trilha/' + result.id]);
  }

  public closeSearch(): void {
    this.searchValue = '';
    this.results = [];
    this.tracks = [];
    this.showSearchFullscren = false;
  }

  private _setSearchSubscription() {
    this.searchSubject.pipe(debounceTime(500))
      .subscribe((searchTextValue) => {
        this._searchModules( searchTextValue );
      }
    );
  }

  private _searchModules(searchValue: string) {
    this._modulesService.getPagedHomeModulesList(
      1, 4, searchValue, null
    ).subscribe((response) => {
      this._loadTracks( searchValue );
      this.results = response.data.modules;
    });
  }

  private _loadTracks(searchValue: string): void {
    this._tracksService.getPagedFilteredTracksList(
      1, 4, searchValue
    ).subscribe((response) => {
      this.tracks = response.data.tracks;
    });
  }
}
