import { Component, OnInit } from '@angular/core';
import { ModulePreview } from '../../models/previews/module.interface';
import { MatSnackBar } from '@angular/material';
import { ContentModulesService } from '../_services/modules.service';
import { ContentTracksService } from '../_services/tracks.service';
import { NotificationClass } from '../../shared/classes/notification';
import { ActivatedRoute } from '@angular/router';
import { TrackPreview } from '../../models/previews/track.interface';
import { TagFilter } from './models/tag-filter.interface';
import { Level } from 'src/app/models/shared/level.interface';
import { SharedService } from 'src/app/shared/services/shared.service';
import { UtilService } from 'src/app/shared/services/util.service';

@Component({
  selector: 'app-search-results',
  templateUrl: './search-results.component.html',
  styleUrls: ['./search-results.component.scss']
})
export class SearchResultsComponent extends NotificationClass implements OnInit {

  public levels: Array<Level> = [];
  public levelDict;

  public modules: Array<ModulePreview> = [];
  public modulesCount: number = 0;
  private _modulesPage: number = 1;

  public tracks: Array<TrackPreview> = [];
  public tracksCount: number = 0;
  private _tracksPage: number = 1;

  public tags: Array<TagFilter>;
  public viewType: string = 'cards';
  private _searchValue: string;

  constructor(
    protected _snackBar: MatSnackBar,
    private _activatedRoute: ActivatedRoute,
    private _modulesService: ContentModulesService,
    private _tracksService: ContentTracksService,
    private _sharedService: SharedService,
    private _utilService: UtilService
  ) {
    super(_snackBar);
  }

  ngOnInit() {
    this._searchValue = this._activatedRoute.snapshot.paramMap.get('searchValue');
    this._loadLevels();
    this._loadModules( this._searchValue, [] );
    this._loadTracks( this._searchValue, [] );
  }

  public changeView(viewType: string): void {
    this.viewType = viewType;
  }

  public updateFilter(tag: TagFilter) {
    tag.checked = !tag.checked;

    const selectedTags =  this.tags.filter(
      t => t.checked
    ).map( t => t.tag );

    this._loadModules( this._searchValue, selectedTags );
    this._loadTracks( this._searchValue, selectedTags );
  }

  public goToModulePage(page: number) {
    if (page !== this._modulesPage) {
      this._modulesPage = page;
      this._loadModules(
        this._searchValue,
        this.tags.filter(tag => tag.checked).map(tag => tag.tag)
      );
    }
  }

  public goToTrackPage(page: number) {
    if (page !== this._modulesPage) {
      this._tracksPage = page;
      this._loadTracks(
        this._searchValue,
        this.tags.filter(tag => tag.checked).map(tag => tag.tag)
      );
    }
  }

  private _loadModules(searchValue: string, tags: Array<string>) {
    this._modulesService.getPagedHomeModulesList(
      this._modulesPage, 6, searchValue, tags
    ).subscribe((response) => {
      this.modules = response.data.modules;
      this.modulesCount = response.data.itemsCount;

      if (!this.tags) {
        this.tags = this._utilService.removeDuplicates(
          this._getTags( this.modules), 'tag'
        );
      }

    }, () => this.notify('Ocorreu um erro, por favor tente novamente mais tarde') );
  }

  private _loadTracks(searchValue: string, tags: Array<string>) {
    this._tracksService.getPagedFilteredTracksList(
      this._tracksPage, 6, searchValue, tags
    ).subscribe((response) => {
      this.tracks = response.data.tracks;
      this.tracksCount = response.data.itemsCount;

    }, () => this.notify('Ocorreu um erro, por favor tente novamente mais tarde') );
  }

  private _getTags(modules: Array<ModulePreview>): Array<TagFilter> {
    return [].concat.apply([], modules.map(m => {
        return m.tags.map(tag => {
          return {
            'tag': tag,
            'checked': false
          };
        });
      })
    );
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

}
