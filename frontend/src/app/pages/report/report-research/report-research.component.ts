import { Component, Input } from '@angular/core';
import { Requirement, RequirementProgress } from '../../../settings/modules/new-module/models/new-requirement.model';
import { Level } from '../../../models/shared/level.interface';
import { Router } from '@angular/router';
import { ExcelService } from 'src/app/shared/services/excel.service';
import { SettingsModulesService } from 'src/app/settings/_services/modules.service';
import { SettingsTracksService } from 'src/app/settings/_services/tracks.service';
import { TestTrack } from 'src/app/models/valuation-test.interface';
import { TrackPreview } from 'src/app/models/previews/track.interface';
import { SettingsUsersService } from 'src/app/settings/_services/users.service';
import { NotificationClass } from 'src/app/shared/classes/notification';
import { MatSnackBar } from '@angular/material';

@Component({
  selector: 'app-report-research',
  templateUrl: './report-research.component.html',
  styleUrls: ['./report-research.component.scss']
})
export class ReportResearchComponent extends NotificationClass {

  @Input() requirement: Requirement;
  @Input() levels: Array<Level>;
  @Input() last: boolean = false;

  public tracks: Array<TestTrack> = [];
  public selectedTracks: Array<TestTrack> = [];

  constructor(
    private _router: Router,
    private _excelService: ExcelService,
    private _modulesService: SettingsModulesService,
    private _tracksService: SettingsTracksService,
    private _usersService: SettingsUsersService,
    protected _snackBar: MatSnackBar,
  ) {
    super(_snackBar);
  }

  public triggerTrackSearch(searchValue: string) {
    this._loadTracks(searchValue);
  }

  public removeSelectedTrack(id: string) {
    this._removeFromCollection(
      this.tracks, this.selectedTracks, id
    );
  }

  private _removeFromCollection(collection, selected, id: string): void {
    const selectedTrackIndex = selected.findIndex(x => x.id === id);
    selected.splice(selectedTrackIndex , 1);

    const trackIndex = collection.findIndex(x => x.id === id);
    collection[trackIndex].checked = false;
  }

  private _loadTracks(searchValue: string = ''): void {
    if (searchValue === '') {
      this.tracks = [];
      return;
    }

    this._tracksService.getPagedFilteredTracksList(
      1, 20, searchValue
    ).subscribe(response => {
      response.data.tracks.forEach((tck: TrackPreview) => {
        tck.checked = this.selectedTracks.find(t => t.id === tck.id) && true;
      });
      this.tracks = response.data.tracks;
    });
  }

  public updateTracks(): void {
    this.selectedTracks = this._updateCollection(
      this.tracks, this.selectedTracks
    );
  }

  private _updateCollection(collection, selected): Array<any> {
    const prevSelected = selected.filter(x =>
      !collection.find(t => t.id === x.id)
    );
    const selectedColl = collection.filter(track =>
      track.checked
    );
    return [ ...prevSelected, ...selectedColl];
  }

  public getNpsInfo(): void {
    this.notify('A exportação pode levar alguns minutos');
    this._tracksService.getTrackNps().subscribe((res) => {
        this._excelService.exportAsExcelFile(
          this._excelService.buildExportNpsReport(res.data),
          'Nps'
        );
    }, (error) => this.notify( this.getErrorNotification(error) ));
  }
}
