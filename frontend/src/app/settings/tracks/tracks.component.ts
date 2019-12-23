import { Component, OnInit } from '@angular/core';
import { MatSnackBar, MatDialog } from '@angular/material';
import { NotificationClass } from '../../shared/classes/notification';
import { Router } from '@angular/router';
import { SettingsTracksService } from '../_services/tracks.service';
import { TrackPreview } from '../../models/previews/track.interface';
import { debounceTime } from 'rxjs/operators';
import { Subject } from 'rxjs';
import { DeleteTrackDialogComponent } from './delete-track/delete-track.dialog';

@Component({
  selector: 'app-settings-tracks',
  templateUrl: './tracks.component.html',
  styleUrls: ['./tracks.component.scss']
})
export class SettingsTracksComponent extends NotificationClass implements OnInit {

  public tracks: Array<TrackPreview> = [];
  public tracksCount: number = 0;
  private _tracksPage: number = 1;
  private _searchSubject: Subject<string> = new Subject();

  constructor(
    protected _snackBar: MatSnackBar,
    private _tracksService: SettingsTracksService,
    private _router: Router,
    private _dialog: MatDialog
  ) {
    super(_snackBar);
  }

  ngOnInit() {
    this._loadTracks(this._tracksPage);
    this._setSearchSubscription();
  }

  public goToPage(page: number) {
    if (page !== this._tracksPage) {
      this._tracksPage = page;
      this._loadTracks(this._tracksPage);
    }
  }

  public updateSearch(searchTextValue: string) {
    this._searchSubject.next( searchTextValue );
  }

  public createNewTrack(): void {
    localStorage.removeItem('editingTrack');
    this._router.navigate([ '/configuracoes/trilha' ]);
  }

  public editTrack(track: TrackPreview) {
    this._tracksService.getTrackById(track.id).subscribe((response) => {
      localStorage.setItem('editingTrack', JSON.stringify(response.data));
      this._router.navigate([ '/configuracoes/trilha' ]);

    }, () => this.notify('Ocorreu um erro, por favor tente novamente mais tarde'));
  }

  public deleteTrack(track: TrackPreview) {
    const dialogRef = this._dialog.open(DeleteTrackDialogComponent, {
      width: '1000px'
    });

    dialogRef.afterClosed().subscribe((result: boolean) => {
      if (result) {
        this._tracksService.deleteTrackById(track.id).subscribe((response) => {
          this.notify('Trilha deletada com sucesso');
          const index = this.tracks.findIndex(x => x.id === track.id);
          this.tracks.splice(index, 1);
        }, () => this.notify('Ocorreu um erro, por favor tente novamente mais tarde'));
      }
    });
  }

  private _loadTracks(page: number, searchValue: string = ''): void {
    this._tracksService.getPagedFilteredTracksList(
      page, 20, searchValue
    ).subscribe((response) => {
      this.tracks = response.data.tracks;
      this.tracksCount = response.data.itemsCount;
    }, () => this.notify('Ocorreu um erro, por favor tente novamente mais tarde'));
  }

  private _setSearchSubscription() {
    this._searchSubject.pipe(
      debounceTime(500)
    ).subscribe((searchValue: string) => {
      this._loadTracks(this._tracksPage, searchValue);
    });
  }

}
