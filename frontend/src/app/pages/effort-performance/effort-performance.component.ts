import { Component, OnInit } from '@angular/core';
import { MatSnackBar } from '@angular/material';
import { NotificationClass } from '../../shared/classes/notification';
import { Router } from '@angular/router';
import { ContentTracksService } from '../_services/tracks.service';
import { TrackPreview } from 'src/app/models/previews/track.interface';
import { UtilService } from 'src/app/shared/services/util.service';
import { Subject } from 'rxjs';
import { debounceTime } from 'rxjs/operators';

@Component({
  selector: 'app-effort-performance',
  templateUrl: './effort-performance.component.html',
  styleUrls: ['./effort-performance.component.scss']
})
export class EffortPerformanceComponent extends NotificationClass implements OnInit {

  public readonly displayedColumns: string[] = [
    'name', 'modules', 'events', 'time', 'action'
  ];

  public tracks: TrackPreview[] = [];
  public itemsCount: number = 0;

  private _currentPage: number = 1;
  private _searchTest: string = '';
  private _searchSubject: Subject<string> = new Subject();

  constructor(
    protected _snackBar: MatSnackBar,
    private _router: Router,
    private _utilService: UtilService,
    private _tracksService: ContentTracksService
  ) {
    super(_snackBar);
  }

  ngOnInit() {
    this._loadTracks(this._currentPage);
    this._setSearchSubscription();
  }

  public goToPage(page: number): void {
    if (page !== this._currentPage) {
      this._currentPage = page;
      this._loadTracks(this._currentPage);
    }
  }

  private _loadTracks(page: number): void {
    this._tracksService.getPagedFilteredEffortPerformancesTracksList(
      page, this._searchTest
    ).subscribe((response) => {
      this.tracks = response.data.tracks;
      this.itemsCount = response.data.itemsCount;

    }, (error) => this.notify( this.getErrorNotification(error) ));
  }

  public getDuration(track: TrackPreview): string {
    return this._utilService.formatSecondsToHour( track.duration );
  }

  public viewDetails(track: TrackPreview) {
    this._router.navigate([ 'configuracoes/trilha-de-curso/' + track.id ]);
  }

  public updateSearch(searchTextValue: string) {
    this._searchSubject.next( searchTextValue );
  }

  private _setSearchSubscription() {
    this._searchSubject.pipe(
      debounceTime(500)
    ).subscribe((searchValue: string) => {
      this._searchTest = searchValue;
      this._currentPage = 1;
      this._loadTracks(this._currentPage);
    });
  }
}
