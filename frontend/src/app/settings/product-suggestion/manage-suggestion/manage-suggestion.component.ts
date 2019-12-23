import { Component, OnInit } from '@angular/core';
import { MatSnackBar, MatDialog } from '@angular/material';
import { Router, ActivatedRoute } from '@angular/router';
import { ProfileTestResponse } from 'src/app/models/profile-test.interface';
import { SettingsProfileTestsService } from '../../_services/profile-tests.service';
import { NotificationClass } from 'src/app/shared/classes/notification';
import { TrackPreview } from 'src/app/models/previews/track.interface';
import { ModulePreview } from 'src/app/models/previews/module.interface';
import { EventPreview } from 'src/app/models/previews/event.interface';
import { SettingsModulesService } from '../../_services/modules.service';
import { SettingsEventsService } from '../../_services/events.service';
import { SettingsTracksService } from '../../_services/tracks.service';
import { SuggestedProduct } from 'src/app/models/previews/suggested-product.interface';
import { SuggestedProductType } from 'src/app/models/enums/suggested-product-type.enum';
import { ExcelService } from 'src/app/shared/services/excel.service';
import { NotifyDialogComponent } from 'src/app/shared/dialogs/notify/notify.dialog';

@Component({
  selector: 'app-settings-manage-suggestion',
  templateUrl: './manage-suggestion.component.html',
  styleUrls: ['./manage-suggestion.component.scss']
})
export class ManageSuggestionComponent extends NotificationClass implements OnInit {

  public response: ProfileTestResponse;
  private _associatedModules: any[];
  private _associatedTracks: any[];
  private _associatedEvents: any[];
  public displayedContent = {
    'top': false,
    'middle': true,
    'bottom': false,
    'gradesInput': false
  };

  public tracks: Array<TrackPreview> = [];
  public tracksCount: number;
  public selectedTracks: Array<TrackPreview> = [];

  public modules: Array<ModulePreview> = [];
  public modulesCount: number;
  public selectedModules: Array<ModulePreview> = [];

  public events: Array<EventPreview> = [];
  public eventsCount: number;
  public selectedEvents: Array<EventPreview> = [];

  private _responseId: string;
  private _repProducts: Array<any> = [];

  constructor(
    protected _snackBar: MatSnackBar,
    private _dialog: MatDialog,
    private _activatedRoute: ActivatedRoute,
    private _router: Router,
    private _profileTestService: SettingsProfileTestsService,
    private _modulesService: SettingsModulesService,
    private _eventsService: SettingsEventsService,
    private _tracksService: SettingsTracksService,
    private _excelService: ExcelService
  ) {
    super(_snackBar);
  }

  ngOnInit() {
    this._responseId = this._activatedRoute.snapshot.paramMap.get('responseId');
    this._loadResponse(this._responseId);
    this._loadTracks();
    this._loadModules();
    this._loadEvents();
  }

  public gradeProfileTestAnswers(): void {
    if (this._checkGrades( this.response )) {
      this._profileTestService.gradeProfileTestAnswers(
        this.response.id, this.response.answers
      ).subscribe(() => {
        this.notify('Notas atribuídas com sucesso!');
        this.displayedContent.gradesInput = false;
        this.response = this._setFinalGrade( this.response );

      }, (err) => this.notify( this.getErrorNotification(err) ) );
    }
  }

  private _checkGrades(response: ProfileTestResponse): boolean {
    if (response.answers.some(a => a.grade === null)) {
      this.notify('Atribua todas as notas para continuar');
      return false;
    }

    const hasInvalidGrades = response.answers.some(a =>
      a.grade < 0 || a.grade > 100 || a.grade > a.percentage
    );

    if (hasInvalidGrades) {
      this.notify('As notas devem ser valores positivos e menores ou iguais ao valor da questão');
      return false;
    }

    return true;
  }

  private _suggestProducts(): void {
    const products = this._fillSuggestions();

    this._profileTestService.suggestProducts(
      this._responseId, products
    ).subscribe(() => {
      this.notify('Produtos recomendados com sucesso!');
      this._router.navigate([ 'configuracoes/recomendacoes-produtos' ]);

    }, (error) => this.notify( this.getErrorNotification(error) ));
  }

  private _openSuggestionDialog() {
    const stringArray = [];
    this._repProducts.forEach( x =>
      stringArray.push('<br>' + x.name)
    );
    if (this._repProducts.length > 0) {
      this._dialog.open(NotifyDialogComponent, {
      width: '400px',
      data: {
        message: 'Há produtos recomendados já associados ao aluno; <br>' +
          stringArray
      }
    });
    }
  }

  public filterSugestions() {
    this.selectedTracks.forEach((t) => {
      const track = this._associatedTracks.find(x => x.id === t.id);
      if (track)
        this._repProducts.push(track);
    });

    this.selectedModules.forEach((m) => {
      const modl = this._associatedModules.find(x => x.id === m.id);
      if (modl)
        this._repProducts.push(modl);
    });

    this.selectedEvents.forEach((e) => {
      this._associatedEvents.forEach((ae) => {
        const eve = e.schedules.find(y => y.id === ae.id);
        if (eve)
          this._repProducts.push(ae);
      });
    });
    this._repProducts = this._repProducts.filter((v, i) => this._repProducts.indexOf(v) === i);

    if (this._repProducts.length === 0)
      this._suggestProducts();
    else
      this._openSuggestionDialog();
  }

  private _fillSuggestions(): Array<SuggestedProduct> {
    const products: Array<SuggestedProduct> = [];

    this.selectedModules.forEach(m =>
      products.push({
        productId: m.id,
        type: SuggestedProductType.Module
      })
    );

    this.selectedEvents.forEach(e =>
      products.push({
        productId: e.id,
        type: SuggestedProductType.Event
      })
    );

    this.selectedTracks.forEach(t =>
      products.push({
        productId: t.id,
        type: SuggestedProductType.Track
      })
    );

    return products;
  }

  public exportAnswers(): void {
    this._excelService.exportAsExcelFile(
      this.response.answers, 'Resposta - ' + this.response.testTitle
    );
  }

  public triggerModuleSearch(searchValue: string) {
    this._loadModules(searchValue);
  }

  public triggerEventSearch(searchValue: string) {
    this._loadEvents( searchValue );
  }

  public triggerTrackSearch(searchValue: string) {
    this._loadTracks( searchValue );
  }

  public removeSelectedModule(id: string): void {
    this._removeFromCollection(
      this.modules, this.selectedModules, id
    );
    this._removeFromRep(id);
  }

  public removeSelectedEvent(id: string) {
    this._removeFromCollection(
      this.events, this.selectedEvents, id
    );
    this._removeFromRep(id);
  }

  public removeSelectedTrack(id: string) {
    this._removeFromCollection(
      this.tracks, this.selectedTracks, id
    );
    this._removeFromRep(id);
  }

  private _removeFromRep(id: string) {
    const index = this._repProducts.findIndex(x => x.id === id);
    this._repProducts.splice(index, 1);
  }

  private _removeFromCollection(collection, selected, id: string): void {
    const selectedTrackIndex = selected.findIndex(x => x.id === id);
    selected.splice(selectedTrackIndex , 1);

    const trackIndex = collection.findIndex(x => x.id === id);
    collection[trackIndex].checked = false;
  }

  public updateModules(): void {
    this.selectedModules = this._updateCollection(
      this.modules, this.selectedModules
    );
  }

  public updateEvents(): void {
    this.selectedEvents = this._updateCollection(
      this.events, this.selectedEvents
    );
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
    selected.forEach((e) => {
      const event = this._repProducts.find(x => x.id === e.id);
      if (event)
      this._removeFromRep(event.id);
    });
    return [ ...prevSelected, ...selectedColl];
  }

  private _loadResponse(responseId: string): void {
    this._profileTestService.getProfileTestResponseById(
      responseId
    ).subscribe((response) => {
      this.response = this._setFinalGrade( response.data );
      this._associatedEvents = response.data.eventsInfo;
      this._associatedTracks = response.data.tracksInfo;
      this._associatedModules = response.data.modulesInfo;

    }, (error) => this.notify( this.getErrorNotification(error) ));
  }

  private _setFinalGrade(response: ProfileTestResponse): ProfileTestResponse {
    response.answers.forEach(a => {
      a.gradeIsSet = a.grade !== null;
    });

    if (response.answers.every(a => a.grade && a.grade > 0)) {
      response.finalGrade = response.answers.reduce(
        (sum, a) => sum + a.grade
      , 0);
    }
    return response;
  }

  private _loadTracks(searchValue: string = ''): void {
    this._tracksService.getPagedFilteredTracksList(
      1, 2, searchValue
    ).subscribe(response => {
      response.data.tracks.forEach(track => {
        track.checked = this.selectedTracks.find(t => t.id === track.id) && true;
      });
      this.tracks = response.data.tracks;
      this.tracksCount = response.data.itemsCount;
    });
  }

  private _loadModules(searchValue: string = ''): void {
    this._modulesService.getPagedFilteredModulesList(
      1, 2, searchValue
    ).subscribe(response => {
      response.data.modules.forEach((mod: ModulePreview) => {
        mod.checked = this.selectedModules.find(t => t.id === mod.id) && true;
      });
      this.modules = response.data.modules;
      this.modulesCount = response.data.itemsCount;
    });
  }

  private _loadEvents(searchValue: string = ''): void {
    this._eventsService.getPagedFilteredEventsList(
      1, 2, searchValue, true
    ).subscribe(response => {
      response.data.events.forEach((ev: EventPreview) => {
        ev.checked = this.selectedEvents.find(t => t.id === ev.id) && true;
      });
      this.events = response.data.events;
      this.eventsCount = response.data.itemsCount;
    });
  }
}
