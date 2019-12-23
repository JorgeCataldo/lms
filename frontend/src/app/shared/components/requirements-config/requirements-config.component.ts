import { Component, Output, EventEmitter, OnInit, Input } from '@angular/core';
import { MatSnackBar } from '@angular/material';
import { NotificationClass } from '../../classes/notification';
import { Requirement } from '../../../settings/modules/new-module/models/new-requirement.model';
import { SettingsModulesService } from '../../../settings/_services/modules.service';
import { debounceTime } from 'rxjs/operators';
import { Subject } from 'rxjs';
import { ModulePreview } from '../../../models/previews/module.interface';
import { Module } from '../../../models/module.model';
import { Level } from '../../../models/shared/level.interface';

@Component({
  selector: 'app-requirements-config',
  templateUrl: './requirements-config.component.html',
  styleUrls: ['./requirements-config.component.scss']
})
export class RequirementsConfigComponent extends NotificationClass implements OnInit {

  @Input() requirements?: Array<Array<Requirement>> = [];
  @Input() currentModule?: Module;
  @Input() levels: Array<Level>;
  @Output() setRequirements = new EventEmitter<Array<Array<Requirement>>>();

  public levelEnum = {
    0: 'Iniciante',
    1: 'Intermediário',
    2: 'Avançado',
    3: 'Expert'
  };

  public requiredModules: Array<Requirement> = [];
  public newModule = new Requirement('', 0, false);
  public loading: boolean = false;
  public editing: boolean = false;
  public searchRequiredResults: Array<ModulePreview> = [];
  private _searchRequiredSubject: Subject<string> = new Subject();

  public optionalModules: Array<Requirement> = [];
  public newOptionalModule = new Requirement('', 0, true);
  public loadingOptional: boolean = false;
  public editingOptional: boolean = false;
  public searchOptionalResults: Array<ModulePreview> = [];
  private _searchOptionalSubject: Subject<string> = new Subject();

  constructor(
    protected _snackBar: MatSnackBar,
    private _modulesService: SettingsModulesService
  ) {
    super(_snackBar);
  }

  ngOnInit() {
    if (this.requirements && this.requirements.length === 2) {
      this.requiredModules = this.requirements[0];
      this.optionalModules = this.requirements[1];
    }

    this._setRequiredSearchSubscription();
    this._setOptionalSearchSubscription();
  }

  public saveModule(optional: boolean = false): void {
    const module = optional ? this.newOptionalModule : this.newModule;

    if (!module.moduleId || module.level == null) {
      this.notify('Selecione o módulo e defina o nível para adicioná-lo como requisito');
      return;
    }

    if (module.module)
      module.title = module.module.title;

    module.percentage = 1;

    (optional && this.editingOptional) || (!optional && this.editing) ?
      this._adjustModule(optional) :
      this._addModule(optional);

    if (optional) {
      this.newOptionalModule = new Requirement('', 0);
      (document.getElementById('optionalInput') as HTMLInputElement).value = '';
    } else {
      this.newModule = new Requirement('', 0);
      (document.getElementById('requiredInput') as HTMLInputElement).value = '';
    }

    this.setRequirements.emit(
      [ this.requiredModules, this.optionalModules ]
    );
  }

  public editModule(module, optional: boolean = false): void {
    module.editing = true;

    if (optional) {
      this.newOptionalModule = module;
      this.editingOptional = true;
    } else {
      this.newModule = module;
      this.editing = true;
    }
  }

  public removeModule(index: number, optional: boolean = false): void {
    optional ?
      this.optionalModules.splice(index, 1) :
      this.requiredModules.splice(index, 1);
  }

  public updateRequiredSearch(searchTextValue: string) {
    this._searchRequiredSubject.next( searchTextValue );
  }

  public updateOptionalSearch(searchTextValue: string) {
    this._searchOptionalSubject.next( searchTextValue );
  }

  public setModule(module: ModulePreview) {
    this.newModule.module = module;
    this.newModule.moduleId = module.id;
    this.searchRequiredResults = [];
    (document.getElementById('requiredInput') as HTMLInputElement).value = module.title;
  }

  public setOptionalModule(module: ModulePreview) {
    this.newOptionalModule.module = module;
    this.newOptionalModule.moduleId = module.id;
    this.searchOptionalResults = [];
    (document.getElementById('optionalInput') as HTMLInputElement).value = module.title;
  }

  private _addModule(optional: boolean = false): void {
    optional ?
      this.optionalModules.push( this.newOptionalModule ) :
      this.requiredModules.push( this.newModule );
  }

  private _adjustModule(optional: boolean = false): void {
    if (optional) {
      this.newOptionalModule.editing = false;
      this.editingOptional = false;
    } else {
      this.newModule.editing = false;
      this.editing = false;
    }
  }

  private _setRequiredSearchSubscription() {
    this._searchRequiredSubject.pipe(
      debounceTime(500)
    ).subscribe((searchValue: string) => {
      this.loading = true;
      this._modulesService.getPagedFilteredModulesList(1, 4, searchValue).subscribe((response) => {
        this.searchRequiredResults = this._filterCurrentModule(response);
        this.loading = false;
        }, () => this.loading = false);
    });
  }

  private _setOptionalSearchSubscription() {
    this._searchOptionalSubject.pipe(
      debounceTime(500)
    ).subscribe((searchValue: string) => {
      this.loadingOptional = true;
      this._modulesService.getPagedFilteredModulesList(1, 4, searchValue).subscribe((response) => {
        this.searchOptionalResults = this._filterCurrentModule(response);
        this.loadingOptional = false;
      }, () => this.loadingOptional = false);
    });
  }

  private _filterCurrentModule(response): Array<ModulePreview> {
    if (this.currentModule && this.currentModule.id) {
      response.data.modules = response.data.modules.filter((mod: Module) => {
        return mod.id !== this.currentModule.id;
      });
    }
    return response.data.modules;
  }

}
